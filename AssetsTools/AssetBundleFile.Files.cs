using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetsTools {
    public partial class AssetBundleFile {
        public struct FileType {
            public string Name; // Only ASCII characters are allowed
            public byte[] Data;

            public int CalcInfoSize() {
                return 8 + 8 + 4 + Name.Length + 1;
            }
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
            // 1.File start must be on block boundary
            // 2.Maximum block size is BLOCK_SIZE

            // Calculate block count
            var blockcount = Files.Sum(f => (f.Data.Length / BLOCK_SIZE) + (f.Data.Length % BLOCK_SIZE != 0 ? 1 : 0));

            // Build blockinfo
            BlockInfo[] blockinfos = new BlockInfo[blockcount];
            short blockflag = EnableCompression ? (short)2 : (short)0;
            int curblock = 0;
            for(int i = 0; i < Files.Length; i++) {
                int size = Files[i].Data.Length;
                while(size > BLOCK_SIZE) {
                    blockinfos[curblock].compressedSize = 0;
                    blockinfos[curblock].uncompressedSize = BLOCK_SIZE;
                    blockinfos[curblock].flag = blockflag;
                    curblock++;
                    size -= BLOCK_SIZE;
                }
                blockinfos[curblock].compressedSize = 0;
                blockinfos[curblock].uncompressedSize = size;
                blockinfos[curblock].flag = blockflag;
                curblock++;
            }

            // Seek Writer (Skip Info)
            int infoheadersize = 4 + 4 + 4;
            int blockinfosize = 0x10 + 4 + (4 + 4 + 2) * blockinfos.Length;
            int fileinfosize = 4 + Files.Sum(f => f.CalcInfoSize());
            int info_offset = writer.Position;
            writer.Position = info_offset + infoheadersize + blockinfosize + fileinfosize;

            // Write Blocks
            curblock = 0;
            int[] fileoffsets = new int[Files.Length];
            for(int i = 0; i < Files.Length; i++) {
                int offset = 0;
                while (offset < Files[i].Data.Length) {
                    switch(blockinfos[curblock].flag & 0x3F) {
                        default: // None
                            writer.WriteBytes(Files[i].Data, offset, blockinfos[curblock].uncompressedSize);
                            offset += blockinfos[curblock].compressedSize = blockinfos[curblock].uncompressedSize;
                            break;
                        case 1:
                            throw new NotSupportedException("LZMA is not supported");
                        case 2: // LZ4
                        case 3: // LZ4HC
                            // Store current position for rollback
                            int file_offset = writer.Position;
                            // Try compression
                            blockinfos[curblock].compressedSize = writer.WriteLZ4Data(Files[i].Data, offset, blockinfos[curblock].uncompressedSize);
                            if(blockinfos[curblock].compressedSize == 0 || blockinfos[curblock].compressedSize >= blockinfos[curblock].uncompressedSize) {
                                writer.Position = file_offset;
                                writer.WriteBytes(Files[i].Data, offset, blockinfos[curblock].uncompressedSize);
                                blockinfos[curblock].compressedSize = blockinfos[curblock].uncompressedSize;
                                blockinfos[curblock].flag &= ~0x3f;
                            }
                            offset += blockinfos[curblock].uncompressedSize;
                            break;
                    }
                    curblock++;
                }
                if (i < Files.Length - 1)
                    fileoffsets[i + 1] = fileoffsets[i] + offset;
            }

            // Write Infos
            writer.Position = info_offset;
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
            for(int i = 0; i < Files.Length; i++) {
                writer.WriteLongBE(fileoffsets[i]);
                writer.WriteLongBE(Files[i].Data.LongLength);
                writer.WriteIntBE(4);
                writer.WriteStringToNull(Files[i].Name);
            }
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
