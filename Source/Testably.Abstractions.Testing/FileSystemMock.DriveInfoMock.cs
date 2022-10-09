using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     Mocked instance of a <see cref="IFileSystem.IDriveInfo" />
    /// </summary>
    public sealed class DriveInfoMock : IStorageDrive
    {
        /// <summary>
        ///     The default <see cref="IFileSystem.IDriveInfo.DriveFormat" />.
        /// </summary>
        public const string DefaultDriveFormat = "NTFS";

        /// <summary>
        ///     The default <see cref="IFileSystem.IDriveInfo.DriveType" />.
        /// </summary>
        public const DriveType DefaultDriveType = DriveType.Fixed;

        /// <summary>
        ///     The default total size of a mocked <see cref="IStorageDrive" />.
        ///     <para />
        ///     The number is equal to 1GB (1 Gigabyte).
        /// </summary>
        public const long DefaultTotalSize = 1024 * 1024 * 1024;

        private readonly FileSystemMock _fileSystem;

        private long _usedBytes;

        internal DriveInfoMock(string driveName, FileSystemMock fileSystem)
        {
            if (string.IsNullOrEmpty(driveName))
            {
                throw new ArgumentNullException(nameof(driveName));
            }

            _fileSystem = fileSystem;

            driveName = ValidateDriveLetter(driveName, fileSystem);
            Name = driveName;
            TotalSize = DefaultTotalSize;
            DriveFormat = DefaultDriveFormat;
            DriveType = DefaultDriveType;
            IsReady = true;
        }

        #region IStorageDrive Members

        /// <inheritdoc cref="IFileSystem.IDriveInfo.AvailableFreeSpace" />
        public long AvailableFreeSpace
            => TotalFreeSpace;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveFormat" />
        public string DriveFormat { get; private set; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveType" />
        public DriveType DriveType { get; private set; }

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.IsReady" />
        public bool IsReady { get; private set; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.Name" />
        public string Name { get; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.RootDirectory" />
        public IFileSystem.IDirectoryInfo RootDirectory
            => DirectoryInfoMock.New(_fileSystem.Storage.GetLocation(Name), _fileSystem);

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalFreeSpace" />
        public long TotalFreeSpace
            => TotalSize - _usedBytes;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalSize" />
        public long TotalSize { get; private set; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.VolumeLabel" />
        [AllowNull]
        public string VolumeLabel
        {
            get;
#if NET6_0_OR_GREATER
            [SupportedOSPlatform("windows")]
#endif
            set;
        } = nameof(FileSystemMock);

        /// <inheritdoc cref="IStorageDrive.ChangeUsedBytes(long)" />
        public IStorageDrive ChangeUsedBytes(long usedBytesDelta)
        {
            long newUsedBytes = Math.Max(0, _usedBytes + usedBytesDelta);

            if (newUsedBytes > TotalSize)
            {
                throw ExceptionFactory.NotEnoughDiskSpace(Name);
            }

            _usedBytes = newUsedBytes;

            return this;
        }

        /// <inheritdoc cref="IStorageDrive.SetDriveFormat(string)" />
        public IStorageDrive SetDriveFormat(
            string driveFormat = DefaultDriveFormat)
        {
            DriveFormat = driveFormat;
            return this;
        }

        /// <inheritdoc cref="IStorageDrive.SetDriveType(System.IO.DriveType)" />
        public IStorageDrive SetDriveType(
            DriveType driveType = DefaultDriveType)
        {
            DriveType = driveType;
            return this;
        }

        /// <inheritdoc cref="IStorageDrive.SetIsReady(bool)" />
        public IStorageDrive SetIsReady(bool isReady = true)
        {
            IsReady = isReady;
            return this;
        }

        /// <inheritdoc cref="IStorageDrive.SetTotalSize(long)" />
        public IStorageDrive SetTotalSize(
            long totalSize = DefaultTotalSize)
        {
            TotalSize = totalSize;
            return this;
        }

        #endregion

        /// <inheritdoc cref="object.ToString()" />
        public override string ToString()
            => Name;

        private static string ValidateDriveLetter(string driveName,
                                                  IFileSystem fileSystem)
        {
            if (driveName.Length == 1 &&
                char.IsLetter(driveName, 0))
            {
                return $"{driveName}:\\";
            }

            if (fileSystem.Path.IsPathRooted(driveName))
            {
                return fileSystem.Path.GetPathRoot(driveName)!;
            }

            throw ExceptionFactory.InvalidDriveName();
        }
    }
}