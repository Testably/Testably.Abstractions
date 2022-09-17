﻿using System.IO;
#if FEATURE_SPAN
#endif

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.IO.Directory" />.
    /// </summary>
    interface IDirectory : IFileSystemExtensionPoint
    {
    }
}