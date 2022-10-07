using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal interface IStorageAccessHandle : IDisposable
    {
    }
}