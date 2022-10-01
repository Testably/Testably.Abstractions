using System;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The type of the change in the <see cref="FileSystemMock" />
    /// </summary>
    [Flags]
    public enum ChangeTypes
    {
        /// <summary>
        ///     The file or directory is created.
        /// </summary>
        Created = 1 << 0,

        /// <summary>
        ///     The file or directory is removed.
        /// </summary>
        Deleted = 1 << 1,

        /// <summary>
        ///     The file or directory is modified.
        /// </summary>
        Modified = 1 << 2,

        /// <summary>
        ///     The file or directory is renamed.
        /// </summary>
        Renamed = 1 << 3,

        /// <summary>
        ///     The changed entity is a file.
        /// </summary>
        File = 1 << 10,

        /// <summary>
        ///     The changed entity is a directory.
        /// </summary>
        Directory = 1 << 11,

        /// <summary>
        ///     The file is created.
        /// </summary>
        FileCreated = File | Created,

        /// <summary>
        ///     The file is deleted.
        /// </summary>
        FileDeleted = File | Deleted,

        /// <summary>
        ///     The file is created.
        /// </summary>
        FileModified = File | Modified,

        /// <summary>
        ///     The file is created.
        /// </summary>
        FileRenamed = File | Renamed,

        /// <summary>
        ///     The directory is created.
        /// </summary>
        DirectoryCreated = Directory | Created,

        /// <summary>
        ///     The directory is deleted.
        /// </summary>
        DirectoryDeleted = Directory | Deleted,

        /// <summary>
        ///     The directory is created.
        /// </summary>
        DirectoryModified = Directory | Modified,

        /// <summary>
        ///     The directory is created.
        /// </summary>
        DirectoryRenamed = Directory | Renamed,

        /// <summary>
        ///     Any change.
        /// </summary>
        Any = ~0
    }
}