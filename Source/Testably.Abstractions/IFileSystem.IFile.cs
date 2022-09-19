using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Text;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.IO.File" />.
    /// </summary>
    interface IFile : IFileSystemExtensionPoint
    {
        /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})" />
        void AppendAllLines(string path, IEnumerable<string> contents);

        /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string}, Encoding)" />
        void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                 CancellationToken cancellationToken =
                                     default(CancellationToken));

        /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                 Encoding encoding,
                                 CancellationToken cancellationToken =
                                     default(CancellationToken));
#endif

        /// <inheritdoc cref="File.AppendAllText(string, string?)" />
        void AppendAllText(string path, string? contents);

        /// <inheritdoc cref="File.AppendAllText(string, string?, Encoding)" />
        void AppendAllText(string path, string? contents, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.AppendAllTextAsync(string, string?, CancellationToken)" />
        Task AppendAllTextAsync(string path, string? contents,
                                CancellationToken cancellationToken =
                                    default(CancellationToken));

        /// <inheritdoc cref="File.AppendAllTextAsync(string, string?, Encoding, CancellationToken)" />
        Task AppendAllTextAsync(string path, string? contents, Encoding encoding,
                                CancellationToken cancellationToken =
                                    default(CancellationToken));
#endif

        /// <inheritdoc cref="File.AppendText(string)" />
        StreamWriter AppendText(string path);

        /// <inheritdoc cref="File.Copy(string, string)" />
        void Copy(string sourceFileName, string destFileName);

        /// <inheritdoc cref="File.Copy(string, string, bool)" />
        void Copy(string sourceFileName, string destFileName, bool overwrite);

        /// <inheritdoc cref="File.Create(string)" />
        FileStream Create(string path);

        /// <inheritdoc cref="File.Create(string, int)" />
        FileStream Create(string path, int bufferSize);

        /// <inheritdoc cref="File.Create(string, int, FileOptions)" />
        FileStream Create(string path, int bufferSize, FileOptions options);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="File.CreateSymbolicLink(string, string)" />
        FileSystemInfo CreateSymbolicLink(string path, string pathToTarget);
#endif

        /// <inheritdoc cref="File.CreateText(string)" />
        StreamWriter CreateText(string path);

        /// <inheritdoc cref="File.Decrypt(string)" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        void Decrypt(string path);

        /// <inheritdoc cref="File.Delete(string)" />
        void Delete(string path);

        /// <inheritdoc cref="File.Encrypt(string)" />
        /// />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        void Encrypt(string path);

        /// <inheritdoc cref="File.Exists(string?)" />
        bool Exists([NotNullWhen(true)] string? path);

        /// <inheritdoc cref="File.GetAttributes(string)" />
        FileAttributes GetAttributes(string path);

        /// <inheritdoc cref="File.GetCreationTime(string)" />
        DateTime GetCreationTime(string path);

        /// <inheritdoc cref="File.GetCreationTimeUtc(string)" />
        DateTime GetCreationTimeUtc(string path);

        /// <inheritdoc cref="File.GetLastAccessTime(string)" />
        DateTime GetLastAccessTime(string path);

        /// <inheritdoc cref="File.GetLastAccessTimeUtc(string)" />
        DateTime GetLastAccessTimeUtc(string path);

        /// <inheritdoc cref="File.GetLastWriteTime(string)" />
        DateTime GetLastWriteTime(string path);

        /// <inheritdoc cref="File.GetLastWriteTimeUtc(string)" />
        DateTime GetLastWriteTimeUtc(string path);

        /// <inheritdoc cref="File.Move(string, string)" />
        void Move(string sourceFileName, string destFileName);

#if FEATURE_FILE_MOVETO_OVERWRITE
        /// <inheritdoc cref="File.Move(string, string, bool)" />
        void Move(string sourceFileName, string destFileName, bool overwrite);
#endif

        /// <inheritdoc cref="File.Open(string, FileMode)" />
        FileStream Open(string path, FileMode mode);

        /// <inheritdoc cref="File.Open(string, FileMode, FileAccess)" />
        FileStream Open(string path, FileMode mode, FileAccess access);

        /// <inheritdoc cref="File.Open(string, FileMode, FileAccess, FileShare)" />
        FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
        /// <inheritdoc cref="File.Open(string, FileStreamOptions)" />
        FileStream Open(string path, FileStreamOptions options);
#endif

        /// <inheritdoc cref="File.OpenRead(string)" />
        FileStream OpenRead(string path);

        /// <inheritdoc cref="File.OpenText(string)" />
        StreamReader OpenText(string path);

        /// <inheritdoc cref="File.OpenWrite(string)" />
        FileStream OpenWrite(string path);

        /// <inheritdoc cref="File.ReadAllBytes(string)" />
        byte[] ReadAllBytes(string path);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.ReadAllBytesAsync(string, CancellationToken)" />
        Task<byte[]> ReadAllBytesAsync(string path,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken));
#endif

        /// <inheritdoc cref="File.ReadAllLines(string)" />
        string[] ReadAllLines(string path);

        /// <inheritdoc cref="File.ReadAllLines(string, Encoding)" />
        string[] ReadAllLines(string path, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.ReadAllLinesAsync(string, CancellationToken)" />
        Task<string[]> ReadAllLinesAsync(string path,
                                         CancellationToken cancellationToken =
                                             default(CancellationToken));

        /// <inheritdoc cref="File.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
        Task<string[]> ReadAllLinesAsync(string path, Encoding encoding,
                                         CancellationToken cancellationToken =
                                             default(CancellationToken));
#endif

        /// <inheritdoc cref="File.ReadAllText(string)" />
        string ReadAllText(string path);

        /// <inheritdoc cref="File.ReadAllText(string, Encoding)" />
        string ReadAllText(string path, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)" />
        Task<string> ReadAllTextAsync(string path,
                                      CancellationToken cancellationToken =
                                          default(CancellationToken));

        /// <inheritdoc cref="File.ReadAllTextAsync(string, Encoding, CancellationToken)" />
        Task<string> ReadAllTextAsync(string path, Encoding encoding,
                                      CancellationToken cancellationToken =
                                          default(CancellationToken));
#endif

        /// <inheritdoc cref="File.ReadLines(string)" />
        IEnumerable<string> ReadLines(string path);

        /// <inheritdoc cref="File.ReadLines(string, Encoding)" />
        IEnumerable<string> ReadLines(string path, Encoding encoding);

        /// <inheritdoc cref="File.Replace(string, string, string?)" />
        void Replace(string sourceFileName, string destinationFileName,
                     string? destinationBackupFileName);

        /// <inheritdoc cref="File.Replace(string, string, string?, bool)" />
        void Replace(string sourceFileName, string destinationFileName,
                     string? destinationBackupFileName, bool ignoreMetadataErrors);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="File.ResolveLinkTarget(string, bool)" />
        FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget);
#endif

        /// <inheritdoc cref="File.SetAttributes(string, FileAttributes)" />
        void SetAttributes(string path, FileAttributes fileAttributes);

        /// <inheritdoc cref="File.SetCreationTime(string, DateTime)" />
        void SetCreationTime(string path, DateTime creationTime);

        /// <inheritdoc cref="File.SetCreationTimeUtc(string, DateTime)" />
        void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

        /// <inheritdoc cref="File.SetLastAccessTime(string, DateTime)" />
        void SetLastAccessTime(string path, DateTime lastAccessTime);

        /// <inheritdoc cref="File.SetLastAccessTimeUtc(string, DateTime)" />
        void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);

        /// <inheritdoc cref="File.SetLastWriteTime(string, DateTime)" />
        void SetLastWriteTime(string path, DateTime lastWriteTime);

        /// <inheritdoc cref="File.SetLastWriteTimeUtc(string, DateTime)" />
        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);

        /// <inheritdoc cref="File.WriteAllBytes(string, byte[])" />
        void WriteAllBytes(string path, byte[] bytes);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)" />
        Task WriteAllBytesAsync(string path, byte[] bytes,
                                CancellationToken cancellationToken =
                                    default(CancellationToken));
#endif

        /// <inheritdoc cref="File.WriteAllLines(string, string[])" />
        void WriteAllLines(string path, string[] contents);

        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" />
        void WriteAllLines(string path, IEnumerable<string> contents);

        /// <inheritdoc cref="File.WriteAllLines(string, string[], Encoding)" />
        void WriteAllLines(string path, string[] contents, Encoding encoding);

        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
        void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
                                CancellationToken cancellationToken =
                                    default(CancellationToken));

        /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
                                Encoding encoding,
                                CancellationToken cancellationToken =
                                    default(CancellationToken));
#endif

        /// <inheritdoc cref="File.WriteAllText(string, string)" />
        void WriteAllText(string path, string? contents);

        /// <inheritdoc cref="File.WriteAllText(string, string, Encoding)" />
        void WriteAllText(string path, string? contents, Encoding encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, CancellationToken)" />
        Task WriteAllTextAsync(string path, string? contents,
                               CancellationToken cancellationToken =
                                   default(CancellationToken));

        /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, Encoding, CancellationToken)" />
        Task WriteAllTextAsync(string path, string? contents, Encoding encoding,
                               CancellationToken cancellationToken =
                                   default(CancellationToken));
#endif
    }
}