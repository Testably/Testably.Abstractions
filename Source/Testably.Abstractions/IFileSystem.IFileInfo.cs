using System.IO;
using System.Runtime.Versioning;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.IO.FileInfo" />.
    /// </summary>
    public interface IFileInfo : IFileSystemInfo
    {
        /// <inheritdoc cref="FileInfo.Directory" />
        IDirectoryInfo? Directory { get; }

        /// <inheritdoc cref="FileInfo.DirectoryName" />
        string? DirectoryName { get; }

        /// <inheritdoc cref="FileInfo.IsReadOnly" />
        bool IsReadOnly { get; set; }

        /// <inheritdoc cref="FileInfo.Length" />
        long Length { get; }

        /// <inheritdoc cref="FileInfo.AppendText()" />
        public StreamWriter AppendText();

        /// <inheritdoc cref="FileInfo.CopyTo(string)" />
        IFileInfo CopyTo(string destFileName);

        /// <inheritdoc cref="FileInfo.CopyTo(string, bool)" />
        IFileInfo CopyTo(string destFileName, bool overwrite);

        /// <inheritdoc cref="FileInfo.Create()" />
        FileStream Create();

        /// <inheritdoc cref="FileInfo.CreateText()" />
        public StreamWriter CreateText();

        /// <inheritdoc cref="FileInfo.Decrypt()" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        void Decrypt();

        /// <inheritdoc cref="FileInfo.Encrypt()" />
        /// />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        void Encrypt();

        /// <inheritdoc cref="FileInfo.MoveTo(string)" />
        void MoveTo(string destFileName);

#if FEATURE_FILE_MOVETO_OVERWRITE
        /// <inheritdoc cref="FileInfo.MoveTo(string, bool)" />
        void MoveTo(string destFileName, bool overwrite);
#endif

        /// <inheritdoc cref="FileInfo.Open(FileMode)" />
        FileStream Open(FileMode mode);

        /// <inheritdoc cref="FileInfo.Open(FileMode, FileAccess)" />
        FileStream Open(FileMode mode, FileAccess access);

        /// <inheritdoc cref="FileInfo.Open(FileMode, FileAccess, FileShare)" />
        FileStream Open(FileMode mode, FileAccess access, FileShare share);

        /// <inheritdoc cref="FileInfo.OpenRead()" />
        FileStream OpenRead();

        /// <inheritdoc cref="FileInfo.OpenText()" />
        public StreamReader OpenText();

        /// <inheritdoc cref="FileInfo.OpenWrite()" />
        FileStream OpenWrite();

        /// <inheritdoc cref="FileInfo.Replace(string, string?)" />
        IFileInfo Replace(string destinationFileName,
                          string? destinationBackupFileName);

        /// <inheritdoc cref="FileInfo.Replace(string, string?, bool)" />
        IFileInfo Replace(string destinationFileName,
                          string? destinationBackupFileName,
                          bool ignoreMetadataErrors);
    }
}