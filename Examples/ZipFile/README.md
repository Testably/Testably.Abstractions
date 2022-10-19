# Zip-File example
This example highlights how to use the `IFileSystem` to compress and uncompress directories using `System.IO.Compression`.

The `ZipFileHelper` class provides two methods:

- `Stream CreateZipFromDirectory(string directory)`  
  Creates a zip file from all files and sub-directories in the given directory. It returns a stream, which can be stored in the file system:
  ```csharp
      using (Stream zipStream = ZipFileHelper.CreateZipFromDirectory(directory))
      {
          using FileSystemStream fileStream = FileSystem.File.Create("test.zip");
          zipStream.CopyTo(fileStream);
      }
	```

- `void ExtractZipToDirectory(Stream stream, string directory)`  
  Extracts the zip file from the stream to the given directory.
  ```csharp
      ZipFileHelper.ExtractZipToDirectory(
          FileSystem.File.OpenRead("test.zip"), directory);
	```
