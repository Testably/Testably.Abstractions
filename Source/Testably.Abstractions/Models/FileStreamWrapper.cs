using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Models;

internal sealed class FileStreamWrapper : FileSystemStream
{
    public FileStreamWrapper(FileStream fileStream)
        : base(fileStream, fileStream.Name, fileStream.IsAsync)

    {

    }
}