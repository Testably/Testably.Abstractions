using System;
using System.IO;
#if NETFRAMEWORK
using Testably.Abstractions.Testing.Internal;
#endif

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private class FileSystemInfoMock : IFileSystem.IFileSystemInfo
    {
        protected readonly FileSystemMock FileSystem;
        private DateTime _creationTime;
        private DateTime _lastAccessTime;
        private DateTime _lastWriteTime;
        protected readonly string OriginalPath;

        internal FileSystemInfoMock(string fullName, string originalPath, FileSystemMock fileSystem)
        {
            FullName = fullName;
#if NETFRAMEWORK
            OriginalPath = originalPath.TrimOnWindows();
#else
            OriginalPath = originalPath;
#endif
            FileSystem = fileSystem;
            AdjustTimes(TimeAdjustments.All);
        }

#region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => _creationTime.ToLocalTime();
            set => _creationTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => _creationTime.ToUniversalTime();
            set => _creationTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Exists" />
        public bool Exists
        {
            get
            {
                _exists ??= FileSystem.FileSystemContainer.Exists(FullName);
                return _exists.Value;
            }
        }

        private bool? _exists;

        protected void ResetExists()
        {
#if !NETFRAMEWORK
            // The DirectoryInfo is not updated in .NET Framework!
            _exists = null;
#endif
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
        public string Extension
            => FileSystem.Path.GetExtension(FullName);

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.FullName" />
        public string FullName { get; }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => _lastAccessTime.ToLocalTime();
            set => _lastAccessTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => _lastAccessTime.ToUniversalTime();
            set => _lastAccessTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => _lastWriteTime.ToLocalTime();
            set => _lastWriteTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => _lastWriteTime.ToUniversalTime();
            set => _lastWriteTime = ConsiderUnspecifiedKind(value);
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
        public string Name
            => FileSystem.Path.GetFileName(FullName.TrimEnd(
                FileSystem.Path.DirectorySeparatorChar,
                FileSystem.Path.AltDirectorySeparatorChar));

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
        public void CreateAsSymbolicLink(string pathToTarget)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
        public void Delete()
        {
            if (!FileSystem.FileSystemContainer.Delete(FullName))
            {
                throw new DirectoryNotFoundException(
                    $"Could not find a part of the path '{FullName}'.");
            }

            ResetExists();
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
        public IFileSystem.IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
            => throw new NotImplementedException();
#endif

#endregion

#if NETSTANDARD2_0
        /// <inheritdoc cref="object.ToString()" />
#else
        /// <inheritdoc cref="System.IO.FileSystemInfo.ToString()" />
#endif
        public override string ToString()
            => OriginalPath;

        internal FileSystemInfoMock AdjustTimes(TimeAdjustments timeAdjustments)
        {
            DateTime now = FileSystem.TimeSystem.DateTime.UtcNow;
            if (timeAdjustments.HasFlag(TimeAdjustments.CreationTime))
            {
                CreationTime = ConsiderUnspecifiedKind(now);
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastAccessTime))
            {
                LastAccessTime = ConsiderUnspecifiedKind(now);
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastWriteTime))
            {
                LastWriteTime = ConsiderUnspecifiedKind(now);
            }

            return this;
        }

        private static DateTime ConsiderUnspecifiedKind(
            DateTime time,
            DateTimeKind targetKind = DateTimeKind.Utc)
        {
            if (time.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(time, targetKind);
            }

            if (targetKind == DateTimeKind.Local)
            {
                return time.ToLocalTime();
            }

            return time.ToUniversalTime();
        }

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
    }
}