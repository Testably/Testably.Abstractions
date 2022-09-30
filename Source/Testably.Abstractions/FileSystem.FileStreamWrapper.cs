using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class FileStreamWrapper : FileSystemStream
    {
        public FileStreamWrapper(FileStream fileStream)
            : base(fileStream, fileStream.Name, fileStream.IsAsync)

        {
        }
    }
}