# AssetsTools
This is an AssetBundle manipulation library for C# inspired by [Unity Asset Bundle Extractor](https://7daystodie.com/forums/showthread.php?22675-Unity-Assets-Bundle-Extractor).

## Features
- Supports Loading/Saving AssetBundle file with or without LZ4 compression
- Supports Loading/Storing AssetsFile from AssetBundle
- Supports Loading Object from AssetsFile as a DynamicObject
- Supports Storing Object to AssetsFile with the content of DynamicObject
- Supports .NET 4.6.1/.NET Standard 2.0

## Examples
#### Rewriting `m_IsReadable` flag of Texture2D

```cs
using AssetsTools;
using AssetsTools.Dynamic;

public static class Program {
	public static void Main() {
		// Load AssetBundle
		AssetBundleFile bundle = AssetBundleFile.LoadFromFile("texture.unity3d");
		
		// Load AssetsFile
		AssetsFile assets = bundle.Files[0].ToAssetsFile();
		
		// Rewrite Texture2D Objects
		foreach(var obj in assets.ObjectsWithClass(ClassIDType.Texture2D) {
			DynamicAsset tex = obj.ToDynamicAsset();
			tex.AsDynamic().m_IsReadable = true;
			obj.LoadDynamicAsset(tex);
		}
		
		// Store the modified AssetsFile
		bundle.Files[0].LoadAssetsFile(assets);
		
		// Save the AssetBundle
		bundle.SaveToFile("texture.mod.unity3d");
	}
}
```


## How to build
### Prerequisits
- Visual Studio 2017 or dotnet
- .NET Framework 4.6.1 (for net461 build)
- .NET Core 2.0 (for netstandard2.0 build)
	
### With VS2017
Just load the AssetsTools.sln and build the whole solutions.
	
### With dotnet
Just execute `dotnet build` in the root directory.
	
## License
MIT