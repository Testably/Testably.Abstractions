using System;
using System.Diagnostics.CodeAnalysis;
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

        private readonly string _key;

        private InMemoryLocation(IDriveInfoMock? drive,
                                 string fullPath,
                                 string friendlyName)
        {
            FullPath = fullPath
               .NormalizePath()
               .TrimOnWindows();
#if FEATURE_PATH_ADVANCED
            _key = System.IO.Path.TrimEndingDirectorySeparator(FullPath);
#else
            _key = FileFeatureExtensionMethods.TrimEndingDirectorySeparator(
                FullPath,
                System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar);
#endif
            if (Framework.IsNetFramework)
            {
                friendlyName = friendlyName.TrimOnWindows();
            }

            FriendlyName = friendlyName.RemoveLeadingDot();
            Drive = drive;
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
            if (System.IO.Path.GetPathRoot(FullPath) == FullPath)
            {
                return null;
            }

            var parentPath = System.IO.Path.GetDirectoryName(FullPath);
            if (parentPath == null)
            {
                return null;
            }

            return New(Drive,
                parentPath,
                System.IO.Path.GetDirectoryName(FriendlyName));
        }

        [return: NotNullIfNotNull("path")]
        public static InMemoryLocation? New(IDriveInfoMock? drive,
                                            string path,
                                            string? friendlyName = null)
        {
            if (path == string.Empty)
            {
                if (Framework.IsNetFramework)
                {
                    throw ExceptionFactory.PathHasNoLegalForm();
                }

                throw ExceptionFactory.PathIsEmpty(nameof(path));
            }

            friendlyName ??= path;
            return new InMemoryLocation(drive, path, friendlyName);
        }

        /// <inheritdoc cref="object.ToString()" />
        public override string ToString()
            => FullPath;
    }
}