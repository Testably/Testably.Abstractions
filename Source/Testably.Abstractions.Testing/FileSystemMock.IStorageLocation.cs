﻿using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The location where <see cref="IStorageContainer" /> are located in the <see cref="IStorage" />.
    /// </summary>
    internal interface IStorageLocation : IEquatable<IStorageLocation>
    {
        /// <summary>
        ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
        /// </summary>
        IStorageDrive? Drive { get; }

        /// <summary>
        ///     The friendly name from the location of the file or directory.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        ///     The full path of the location of the file or directory.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        ///     Get the parent location.
        /// </summary>
        /// <returns>
        ///     The parent <see cref="IStorageLocation" /> or <see langword="null" /> when the location corresponds to a root
        ///     directory.
        /// </returns>
        IStorageLocation? GetParent();
    }
}