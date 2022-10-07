using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class InMemoryLocation : IEquatable<InMemoryLocation>
    {
        /// <summary>
        ///     The friendly name from the location of the file or directory.
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        ///     The full path of the location of the file or directory.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
        /// </summary>
        public IDriveInfoMock? Drive { get; }

        public static InMemoryLocation Null
        => new InMemoryLocation();

        private readonly string _key;

        private InMemoryLocation()
        {
            FullPath = string.Empty;
            FriendlyName = string.Empty;
            _key = string.Empty;
        }
        private InMemoryLocation(FileSystemMock fileSystem,
                                 string fullPath,
                                 string friendlyName)
        {
            FullPath = fileSystem.Path
               .GetFullPath(fullPath)
               .NormalizePath()
               .TrimOnWindows();
            _key = fileSystem.Path.TrimEndingDirectorySeparator(FullPath);
            if (Framework.IsNetFramework)
            {
                friendlyName = friendlyName.TrimOnWindows();
            }
            FriendlyName = friendlyName.RemoveLeadingDot();
            if (string.IsNullOrEmpty(FullPath))
            {
                Drive = fileSystem.Storage.GetDrives().First();
            }
            else
            {
                Drive = fileSystem.Storage.GetDrive(
                    fileSystem.Path.GetPathRoot(FullPath));
            }
        }

        /// <inheritdoc cref="object.ToString()" />
        public override string ToString()
            => FullPath;

        [return: NotNullIfNotNull("path")]
        public static InMemoryLocation? New(FileSystemMock fileSystem,
                                            string? path,
                                            string? friendlyName = null)
        {
            if (path == null)
            {
                return null;
            }

            if (path == string.Empty)
            {
                if (Framework.IsNetFramework)
                {
                    throw ExceptionFactory.PathHasNoLegalForm();
                }

                throw ExceptionFactory.PathIsEmpty(nameof(path));
            }

            friendlyName ??= path;
            return new InMemoryLocation(fileSystem, path, friendlyName);
        }

        /// <inheritdoc cref="IEquatable{InMemoryLocation}.Equals(InMemoryLocation)" />
        public bool Equals(InMemoryLocation? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _key.Equals(other._key,
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc cref="object.Equals(object?)" />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) ||
               (obj is InMemoryLocation other && Equals(other));

        /// <inheritdoc cref="object.GetHashCode()" />
        public override int GetHashCode()
            => _key.GetHashCode();
    }
}