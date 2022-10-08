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
        private static readonly StringComparison StringComparisonMode =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

        /// <summary>
        ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
        /// </summary>
        public IDriveInfoMock? Drive { get; }

        /// <summary>
        ///     The friendly name from the location of the file or directory.
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        ///     The full path of the location of the file or directory.
        /// </summary>
        public string FullPath { get; }

        public static InMemoryLocation Null => new();

        private readonly FileSystemMock? _fileSystem;

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
            _fileSystem = fileSystem;
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

        #region IEquatable<InMemoryLocation> Members

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

            return _key.Equals(other._key, StringComparisonMode);
        }

        #endregion

        /// <inheritdoc cref="object.Equals(object?)" />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) ||
               (obj is InMemoryLocation other && Equals(other));

        /// <inheritdoc cref="object.GetHashCode()" />
        public override int GetHashCode()
#if NETSTANDARD2_0
            => _key.ToLowerInvariant().GetHashCode();
#else
            => _key.GetHashCode(StringComparisonMode);
#endif

        public InMemoryLocation? GetParent()
        {
            if (_fileSystem == null)
            {
                return null;
            }

            if (_fileSystem.Path.GetPathRoot(FullPath) == FullPath)
            {
                return null;
            }

            return New(_fileSystem,
                _fileSystem.Path.GetDirectoryName(FullPath),
                _fileSystem.Path.GetDirectoryName(FriendlyName));
        }

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

        /// <inheritdoc cref="object.ToString()" />
        public override string ToString()
            => FullPath;
    }
}