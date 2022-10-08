using System;

namespace Testably.Abstractions.Testing.Storage;

[Flags]
internal enum ContainerType
{
    Directory = 1,
    File = 2,
    Unknown = 4,
    DirectoryOrFile = Directory | File
}