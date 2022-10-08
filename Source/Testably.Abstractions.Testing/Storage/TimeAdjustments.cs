using System;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     Flags indicating which times to adjust for a <see cref="FileSystemMock.FileSystemInfoMock" />.
/// </summary>
/// .
[Flags]
internal enum TimeAdjustments
{
    /// <summary>
    ///     Adjusts no times on the <see cref="FileSystemMock.FileSystemInfoMock" />
    /// </summary>
    None = 0,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.CreationTime" />
    /// </summary>
    CreationTime = 1 << 0,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastAccessTime" />
    /// </summary>
    LastAccessTime = 1 << 1,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastWriteTime" />
    /// </summary>
    LastWriteTime = 1 << 2,

    /// <summary>
    ///     Adjusts all times on the <see cref="FileSystemMock.FileSystemInfoMock" />
    /// </summary>
    All = CreationTime | LastAccessTime | LastWriteTime,
}