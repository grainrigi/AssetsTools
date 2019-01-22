using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetBundleFile {
        /// <summary>
        /// File Entry of AssetBundle
        /// </summary>
        public struct FileType {
            /// <summary>
            /// Name of file.
            /// </summary>
            /// <remarks>Only ASCII characters are allowed.</remarks>
            public string Name;
            /// <summary>
            /// Content of file.
            /// </summary>
            public byte[] Data;

            internal int CalcInfoSize() {
                return 8 + 8 + 4 + Name.Length + 1;
            }
        }

        private struct CompressionInfo {
            public byte[] data;
            public int offset;
            public int length;
        }

        private const int BLOCK_SIZE = 0x20000;

        private void readFiles(UnityBinaryReader reader) {
            int compressedSize = reader.ReadIntBE();
            int uncompressedSize = reader.ReadIntBE();
            int flag = reader.ReadIntBE();

            UnityBinaryReader inforeader;

            if ((flag & 0x80) != 0) // At end of file
                throw new NotImplementedException("BlockInfos are at the end of file");

            // Decompress Infos (if needed)
            int compressiontype = flag & 0x3F;
            switch(compressiontype) {
                default:// None
                    inforeader = reader;
                    break;
                case 1: // LZMA(Not Supported)
                    throw new NotSupportedException("LZMA is not supported");
                case 2: //LZ4
                case 3: //LZ4HC
                    {
                        byte[] infobytes = MemoryPool<AssetBundleFile>.GetBuffer(uncompressedSize);
                        reader.ReadLZ4Data(compressedSize, uncompressedSize, infobytes, 0);
                        inforeader = new UnityBinaryReader(infobytes, 0, uncompressedSize); 
                        break;
                    }
            }

            // Read Block Infos
            inforeader.Position += 0x10;
            int blockcount = inforeader.ReadIntBE();

            var blockinfos = new BlockInfo[blockcount];
            blockinfos.Read(inforeader);

            // Read File Infos
            int filecount = inforeader.ReadIntBE();
            Files = new FileType[filecount];
            long[] fileoffsets = new long[filecount];
            for(int i = 0; i < filecount; i++) {
                fileoffsets[i] = inforeader.ReadLongBE();
                Files[i].Data = new byte[inforeader.ReadLongBE()];
                flag = inforeader.ReadIntBE();
                Files[i].Name = inforeader.ReadStringToNull();
            }

            // Read Directories
            BlockReader blockreader = new BlockReader(blockinfos, reader);
            for(int i = 0; i < filecount; i++) {
                blockreader.Seek((int)fileoffsets[i]);
                blockreader.ReadBytes(Files[i].Data, 0, Files[i].Data.Length);
            }
        }

        private void writeFiles(UnityBinaryWriter writer) {
            // calc total size
            int totalsize = Files.Sum(f => f.Data.Length);

            // teardown into blocks
            // 1.File is not aligned to block boundary (Simple concatation)
            // 2.Maximum block size is BLOCK_SIZE

            // Calculate block count
            var totalbytes = Files.Sum(f => f.Data.Length);
            var blockcount = totalbytes / BLOCK_SIZE + (totalbytes % BLOCK_SIZE != 0 ? 1 : 0);

            // Build blockinfo
            BlockInfo[] blockinfos = new BlockInfo[blockcount];
            short blockflag = EnableCompression ? (short)2 : (short)0;
            for(int i = 0; i < blockcount; i++) {
                blockinfos[i].uncompressedSize = BLOCK_SIZE;
                blockinfos[i].compressedSize = BLOCK_SIZE;
                blockinfos[i].flag = blockflag;
            }
            if (totalbytes % BLOCK_SIZE != 0) {
                blockinfos[blockcount - 1].uncompressedSize = totalbytes % BLOCK_SIZE;
                blockinfos[blockcount - 1].compressedSize = totalbytes % BLOCK_SIZE;
            }

            // Seek Writer (Skip Info)
            int infoheadersize = 4 + 4 + 4;
            int blockinfosize = 0x10 + 4 + (4 + 4 + 2) * blockinfos.Length;
            int fileinfosize = 4 + Files.Sum(f => f.CalcInfoSize());
            int info_offset = writer.Position;

            // Write Blocks

            // If no compression required, just copy all files
            if(!EnableCompression) {
                // Write Header

                // Info Header
                writer.WriteIntBE(blockinfosize + fileinfosize);
                writer.WriteIntBE(blockinfosize + fileinfosize);
                writer.WriteIntBE(0x40);
                writer.Position += 0x10;
                // BlockInfo
                writer.WriteIntBE(blockcount);
                blockinfos.Write(writer);
                // FileInfo
                writer.WriteIntBE(Files.Length);
                int curoffset = 0;
                for(int i = 0; i < Files.Length; i++) {
                    writer.WriteLongBE(curoffset);
                    writer.WriteLongBE(Files[i].Data.LongLength);
                    writer.WriteIntBE(4);
                    writer.WriteStringToNull(Files[i].Name);
                    curoffset += Files[i].Data.Length;
                }

                // Write Files
                for (int i = 0; i < Files.Length; i++)
                    writer.WriteBytes(Files[i].Data);
            }
            // In compression mode, try to parallelize the compression
            else {
                // First of all, Prepare buffer for compression
                byte[] compbuf = MemoryPool<AssetBundleFile>.GetBuffer(blockcount * BLOCK_SIZE);

                // don't parallelize when block count is small
                if (blockcount < 128) {
                    byte[] boundarybuf = MiniMemoryPool<AssetBundleFile>.GetBuffer(BLOCK_SIZE);
                    int remainlength = 0;
                    int curblock = 0;
                    for(int i = 0; i < Files.Length; i++) {
                        // If previous file has overflow, concat and compress
                        if (remainlength > 0) {
                            Buffer.BlockCopy(Files[i].Data, 0, boundarybuf, remainlength, BLOCK_SIZE - remainlength);
                            blockinfos[curblock].compressedSize = TryLZ4Compress(boundarybuf, 0, compbuf, curblock * BLOCK_SIZE, BLOCK_SIZE);
                            if(blockinfos[curblock]. compressedSize == BLOCK_SIZE)
                                blockinfos[curblock].flag &= ~0x3F;
                            curblock++;
                        }

                        // update remainlength
                        int blockstart = 0;
                        if(remainlength > 0)
                            blockstart = BLOCK_SIZE - remainlength;

                        // compress fullblocks
                        int fullblockcount = (Files[i].Data.Length - blockstart) / BLOCK_SIZE;
                        for(int j = 0; j < fullblockcount; j++, curblock++) {
                            blockinfos[curblock].compressedSize = TryLZ4Compress(Files[i].Data, blockstart + j * BLOCK_SIZE, compbuf, curblock * BLOCK_SIZE, BLOCK_SIZE);
                            if (blockinfos[curblock].compressedSize == BLOCK_SIZE)
                                blockinfos[curblock].flag &= ~0x3F;
                        }

                        // If the file has remaindata, buffer them
                        remainlength = (Files[i].Data.Length - blockstart) % BLOCK_SIZE;
                        if(remainlength > 0)
                            Buffer.BlockCopy(Files[i].Data, Files[i].Data.Length - remainlength, boundarybuf, 0, remainlength);
                    }
                    if(remainlength > 0) { // Process last block
                        blockinfos[curblock].compressedSize = TryLZ4Compress(boundarybuf, 0, compbuf, curblock * BLOCK_SIZE, remainlength);
                        if (blockinfos[curblock].compressedSize == remainlength)
                            blockinfos[curblock].flag &= ~0x3F;
                    }
                }
                else {
                    // Create CompressionInfo & Compress file boundary
                    CompressionInfo[] compinfos = new CompressionInfo[blockcount];
                    byte[] boundarybuf = MiniMemoryPool<AssetBundleFile>.GetBuffer(BLOCK_SIZE);
                    int curblock = 0;
                    int remainlength = 0;
                    for(int i = 0; i < Files.Length; i++) {
                        // If previous file has overflow, concat and compress
                        if (remainlength > 0) {
                            Buffer.BlockCopy(Files[i].Data, 0, boundarybuf, remainlength, BLOCK_SIZE - remainlength);
                            blockinfos[curblock].compressedSize = TryLZ4Compress(boundarybuf, 0, compbuf, curblock * BLOCK_SIZE, BLOCK_SIZE);
                            if (blockinfos[curblock].compressedSize == BLOCK_SIZE)
                                blockinfos[curblock].flag &= ~0x3F;
                            curblock++;
                        }

                        int blockstart = 0;
                        if(remainlength > 0)
                            blockstart = BLOCK_SIZE - remainlength;

                        int fullblockcount = (Files[i].Data.Length - blockstart) / BLOCK_SIZE;
                        for(int j = 0; j < fullblockcount; j++, curblock++) {
                            compinfos[curblock].data = Files[i].Data;
                            compinfos[curblock].length = BLOCK_SIZE;
                            compinfos[curblock].offset = blockstart + j * BLOCK_SIZE;
                        }

                        // If the file has remaindata, buffer them
                        remainlength = (Files[i].Data.Length - blockstart) % BLOCK_SIZE;
                        if (remainlength > 0)
                            Buffer.BlockCopy(Files[i].Data, Files[i].Data.Length - remainlength, boundarybuf, 0, remainlength);
                    }
                    if (remainlength > 0) { // Process last block
                        blockinfos[curblock].compressedSize =
                                LZ4.LZ4Codec.Encode(boundarybuf, 0, remainlength,
                                    compbuf, curblock * BLOCK_SIZE, remainlength);
                        // If compression is no use, just copy
                        if (blockinfos[curblock].compressedSize == 0) {
                            blockinfos[curblock].compressedSize = remainlength;
                            blockinfos[curblock].flag &= ~0x3F;
                            Buffer.BlockCopy(boundarybuf, 0, compbuf, curblock * BLOCK_SIZE, BLOCK_SIZE);
                        }
                    }

                    // Parallelly compress the data
                    Parallel.For(0, blockcount, i => {
                        if (compinfos[i].data == null)
                            return;
                        blockinfos[i].compressedSize = TryLZ4Compress(compinfos[i].data, compinfos[i].offset, compbuf, i * BLOCK_SIZE, compinfos[i].length);
                        if (blockinfos[i].compressedSize == BLOCK_SIZE)
                            blockinfos[i].flag &= ~0x3F;
                    });
                }

                // Write Headers
                UnityBinaryWriter headerwriter = new UnityBinaryWriter();
                // Info Header
                headerwriter.Position += 0x10;
                // BlockInfo
                headerwriter.WriteIntBE(blockcount);
                blockinfos.Write(headerwriter);
                // FileInfo
                headerwriter.WriteIntBE(Files.Length);
                int curoffset = 0;
                for (int i = 0; i < Files.Length; i++) {
                    headerwriter.WriteLongBE(curoffset);
                    headerwriter.WriteLongBE(Files[i].Data.LongLength);
                    headerwriter.WriteIntBE(4);
                    headerwriter.WriteStringToNull(Files[i].Name);
                    curoffset += Files[i].Data.Length;
                }

                // Compress and write header
                writer.Position += 4 + 4 + 4;
                int header_compsize = writer.WriteLZ4Data(headerwriter.ToBytes());
                int final_pos = writer.Position;
                writer.Position = info_offset;
                writer.WriteIntBE(header_compsize);
                writer.WriteIntBE(blockinfosize + fileinfosize);
                writer.WriteIntBE(0x42);
                writer.Position = final_pos;

                // Write Blocks
                for (int i = 0; i < blockcount; i++)
                    writer.WriteBytes(compbuf, i * BLOCK_SIZE, blockinfos[i].compressedSize);
            }
        }

        private int TryLZ4Compress(byte[] src, int srcOffset, byte[] dest, int destOffset, int length) {
            int compsize = LZ4.LZ4Codec.Encode(src, srcOffset, length, dest, destOffset, length - 1);
            // If compression is no use, just copy
            if (compsize == 0) {
                Buffer.BlockCopy(src, srcOffset, dest, destOffset, length);
                return length;
            }
            return compsize;
        }

        private struct BlockInfo : ISerializable {
            public int uncompressedSize;
            public int compressedSize;
            public short flag;

            public void Read(UnityBinaryReader reader) {
                uncompressedSize = reader.ReadIntBE();
                compressedSize = reader.ReadIntBE();
                flag = reader.ReadShortBE();
            }

            public void Write(UnityBinaryWriter writer) {
                writer.WriteIntBE(uncompressedSize);
                writer.WriteIntBE(compressedSize);
                writer.WriteShortBE(flag);
            }
        }

        private struct BlockReader {
            UnityBinaryReader _reader;
            BlockInfo[] _infos;
            int idx;
            // Ranged Buffer
            byte[] buffer;
            int begin;
            int end;

            private int nextSize => _infos[idx].uncompressedSize;

            public BlockReader(BlockInfo[] infos, UnityBinaryReader reader) {
                _reader = reader;
                _infos = infos;
                idx = 0;
                buffer = null;
                begin = 0;
                end = 0;
            }

            public void ReadBytes(byte[] dest, int offset, int length) {
                // From buffer
                if (length > end - begin) {
                    int readsize = end - begin;
                    if(readsize > 0) {
                        Buffer.BlockCopy(buffer, begin, dest, offset, readsize);
                        length -= readsize;
                        offset += readsize;
                    }
                }
                else { // Buffer only
                    Buffer.BlockCopy(buffer, begin, dest, offset, length);
                    begin += length;
                    return;
                }

                // Read blocks
                while (true) {
                    if(nextSize <= length) {
                        // Directly Read the block into dest
                        int written = readNextBlock(dest, offset);
                        length -= written;
                        if (length == 0)
                            return;
                        offset += written;
                    }
                    else {
                        // Buffer and copy
                        buffer = MemoryPool<BlockReader>.GetBuffer(nextSize);
                        readNextBlock(buffer, 0);
                        Buffer.BlockCopy(buffer, 0, dest, offset, length);
                        return;
                    }
                }
            }

            public void Seek(int offset) {
                int curoffset = 0;
                // read infos until find the block which contains the specified offset
                int idx = 0;
                for(; idx < _infos.Length; idx++) {
                    if (offset <= curoffset + _infos[idx].uncompressedSize) {
                        if (offset == curoffset) { // exactly on block boundary
                            begin = end = 0;
                            this.idx = idx;
                            return;
                        }
                        else { // Need buffering
                            buffer = MemoryPool<BlockReader>.GetBuffer(_infos[idx].uncompressedSize);
                            begin = offset - curoffset;
                            end = buffer.Length;
                            return;
                        }
                    }
                    else
                        curoffset += _infos[idx].uncompressedSize;
                }
                throw new IndexOutOfRangeException();
            }

            private int readNextBlock(byte[] dest, int offset) {
                if (idx >= _infos.Length)
                    throw new IndexOutOfRangeException();
                switch(_infos[idx].flag) {
                    default:
                        _reader.ReadBytes(dest, offset, _infos[idx].uncompressedSize);
                        break;
                    case 1: // LZMA
                        throw new NotSupportedException("LZMA is not supported");
                    case 2: // LZ4
                    case 3: // LZ4HC
                        _reader.ReadLZ4Data(_infos[idx].compressedSize, _infos[idx].uncompressedSize, dest, offset);
                        break;
                }
                return _infos[idx++].uncompressedSize;
            }
        }
    }
}
