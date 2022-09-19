using System;

namespace Testably.Abstractions.Testing.Internal.Models;

/// <summary>
///     Flags indicating which times to adjust for a <see cref="FileSystemInfoMock" />.
/// </summary>
/// .
[Flags]
internal enum TimeAdjustments
{
    /// <summary>
    ///     Adjusts no times on the <see cref="FileSystemInfoMock" />
    /// </summary>
    None = 0,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemInfoMock.CreationTime" />
    /// </summary>
    CreationTime = 1 << 0,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemInfoMock.LastAccessTime" />
    /// </summary>
    LastAccessTime = 1 << 1,

    /// <summary>
    ///     Adjusts the <see cref="FileSystemInfoMock.LastWriteTime" />
    /// </summary>
    LastWriteTime = 1 << 2,

    /// <summary>
    ///     Adjusts all times on the <see cref="FileSystemInfoMock" />
    /// </summary>
    All = CreationTime | LastAccessTime | LastWriteTime,
}