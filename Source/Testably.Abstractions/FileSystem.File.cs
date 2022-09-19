using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class FileFileSystem : IFileSystem.IFile
    {
        internal FileFileSystem(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IFile Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLines(string, IEnumerable{string})" />
        public void AppendAllLines(string path, IEnumerable<string> contents)
            => System.IO.File.AppendAllLines(path, contents);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLines(string, IEnumerable{string}, Encoding)" />
        public void AppendAllLines(string path, IEnumerable<string> contents,
                                   Encoding encoding)
            => System.IO.File.AppendAllLines(path, contents, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                        CancellationToken cancellationToken =
                                            default(CancellationToken))
            => System.IO.File.AppendAllLinesAsync(path, contents, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                        Encoding encoding,
                                        CancellationToken cancellationToken =
                                            default(CancellationToken))
            => System.IO.File.AppendAllLinesAsync(path, contents, encoding,
                cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllText(string, string?)" />
        public void AppendAllText(string path, string? contents)
            => System.IO.File.AppendAllText(path, contents);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllText(string, string?, Encoding)" />
        public void AppendAllText(string path, string? contents, Encoding encoding)
            => System.IO.File.AppendAllText(path, contents, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllTextAsync(string, string?, CancellationToken)" />
        public Task AppendAllTextAsync(string path, string? contents,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken))
            => System.IO.File.AppendAllTextAsync(path, contents, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllTextAsync(string, string?, Encoding, CancellationToken)" />
        public Task AppendAllTextAsync(string path, string? contents, Encoding encoding,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken))
            => System.IO.File.AppendAllTextAsync(path, contents, encoding,
                cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendText(string)" />
        public StreamWriter AppendText(string path)
            => System.IO.File.AppendText(path);

        /// <inheritdoc cref="IFileSystem.IFile.Copy(string, string)" />
        public void Copy(string sourceFileName, string destFileName)
            => System.IO.File.Copy(sourceFileName, destFileName);

        /// <inheritdoc cref="IFileSystem.IFile.Copy(string, string, bool)" />
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
            => System.IO.File.Copy(sourceFileName, destFileName, overwrite);

        /// <inheritdoc cref="IFileSystem.IFile.Create(string)" />
        public FileStream Create(string path)
            => System.IO.File.Create(path);

        /// <inheritdoc cref="IFileSystem.IFile.Create(string, int)" />
        public FileStream Create(string path, int bufferSize)
            => System.IO.File.Create(path, bufferSize);

        /// <inheritdoc cref="IFileSystem.IFile.Create(string, int, FileOptions)" />
        public FileStream Create(string path, int bufferSize, FileOptions options)
            => System.IO.File.Create(path, bufferSize, options);

        /// <inheritdoc cref="IFileSystem.IFile.CreateSymbolicLink(string, string)" />
        public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
            => System.IO.File.CreateSymbolicLink(path, pathToTarget);

        /// <inheritdoc cref="IFileSystem.IFile.CreateText(string)" />
        public StreamWriter CreateText(string path)
            => System.IO.File.CreateText(path);

        /// <inheritdoc cref="IFileSystem.IFile.Decrypt(string)" />
        public void Decrypt(string path)
            => System.IO.File.Decrypt(path);

        /// <inheritdoc cref="IFileSystem.IFile.Delete(string)" />
        public void Delete(string path)
            => System.IO.File.Delete(path);

        /// <inheritdoc cref="IFileSystem.IFile.Encrypt(string)" />
        public void Encrypt(string path)
            => System.IO.File.Encrypt(path);

        /// <inheritdoc cref="IFileSystem.IFile.Exists(string?)" />
        public bool Exists(string? path)
            => System.IO.File.Exists(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetAttributes(string)" />
        public FileAttributes GetAttributes(string path)
            => System.IO.File.GetAttributes(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetCreationTime(string)" />
        public DateTime GetCreationTime(string path)
            => System.IO.File.GetCreationTime(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetCreationTimeUtc(string)" />
        public DateTime GetCreationTimeUtc(string path)
            => System.IO.File.GetCreationTimeUtc(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetLastAccessTime(string)" />
        public DateTime GetLastAccessTime(string path)
            => System.IO.File.GetLastAccessTime(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetLastAccessTimeUtc(string)" />
        public DateTime GetLastAccessTimeUtc(string path)
            => System.IO.File.GetLastAccessTimeUtc(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetLastWriteTime(string)" />
        public DateTime GetLastWriteTime(string path)
            => System.IO.File.GetLastWriteTime(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetLastWriteTimeUtc(string)" />
        public DateTime GetLastWriteTimeUtc(string path)
            => System.IO.File.GetLastWriteTimeUtc(path);

        /// <inheritdoc cref="IFileSystem.IFile.Move(string, string)" />
        public void Move(string sourceFileName, string destFileName)
            => System.IO.File.Move(sourceFileName, destFileName);

        /// <inheritdoc cref="IFileSystem.IFile.Move(string, string, bool)" />
        public void Move(string sourceFileName, string destFileName, bool overwrite)
            => System.IO.File.Move(sourceFileName, destFileName, overwrite);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode)" />
        public FileStream Open(string path, FileMode mode)
            => System.IO.File.Open(path, mode);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode, FileAccess)" />
        public FileStream Open(string path, FileMode mode, FileAccess access)
            => System.IO.File.Open(path, mode, access);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode, FileAccess, FileShare)" />
        public FileStream Open(string path, FileMode mode, FileAccess access,
                               FileShare share)
            => System.IO.File.Open(path, mode, access, share);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileStreamOptions)" />
        public FileStream Open(string path, FileStreamOptions options)
            => System.IO.File.Open(path, options);

        /// <inheritdoc cref="IFileSystem.IFile.OpenRead(string)" />
        public FileStream OpenRead(string path)
            => System.IO.File.OpenRead(path);

        /// <inheritdoc cref="IFileSystem.IFile.OpenText(string)" />
        public StreamReader OpenText(string path)
            => System.IO.File.OpenText(path);

        /// <inheritdoc cref="IFileSystem.IFile.OpenWrite(string)" />
        public FileStream OpenWrite(string path)
            => System.IO.File.OpenWrite(path);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllBytes(string)" />
        public byte[] ReadAllBytes(string path)
            => System.IO.File.ReadAllBytes(path);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllBytesAsync(string, CancellationToken)" />
        public Task<byte[]> ReadAllBytesAsync(string path,
                                              CancellationToken cancellationToken =
                                                  default(CancellationToken))
            => System.IO.File.ReadAllBytesAsync(path, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLines(string)" />
        public string[] ReadAllLines(string path)
            => System.IO.File.ReadAllLines(path);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLines(string, Encoding)" />
        public string[] ReadAllLines(string path, Encoding encoding)
            => System.IO.File.ReadAllLines(path, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLinesAsync(string, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(string path,
                                                CancellationToken cancellationToken =
                                                    default(CancellationToken))
            => System.IO.File.ReadAllLinesAsync(path, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding,
                                                CancellationToken cancellationToken =
                                                    default(CancellationToken))
            => System.IO.File.ReadAllLinesAsync(path, encoding, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllText(string)" />
        public string ReadAllText(string path)
            => System.IO.File.ReadAllText(path);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllText(string, Encoding)" />
        public string ReadAllText(string path, Encoding encoding)
            => System.IO.File.ReadAllText(path, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllTextAsync(string, CancellationToken)" />
        public Task<string> ReadAllTextAsync(string path,
                                             CancellationToken cancellationToken =
                                                 default(CancellationToken))
            => System.IO.File.ReadAllTextAsync(path, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllTextAsync(string, Encoding, CancellationToken)" />
        public Task<string> ReadAllTextAsync(string path, Encoding encoding,
                                             CancellationToken cancellationToken =
                                                 default(CancellationToken))
            => System.IO.File.ReadAllTextAsync(path, encoding, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadLines(string)" />
        public IEnumerable<string> ReadLines(string path)
            => System.IO.File.ReadLines(path);

        /// <inheritdoc cref="IFileSystem.IFile.ReadLines(string, Encoding)" />
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
            => System.IO.File.ReadLines(path, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.Replace(string, string, string)" />
        public void Replace(string sourceFileName, string destinationFileName,
                            string? destinationBackupFileName)
            => System.IO.File.Replace(sourceFileName, destinationFileName,
                destinationBackupFileName);

        /// <inheritdoc cref="IFileSystem.IFile.Replace(string, string, string, bool)" />
        public void Replace(string sourceFileName, string destinationFileName,
                            string? destinationBackupFileName, bool ignoreMetadataErrors)
            => System.IO.File.Replace(sourceFileName, destinationFileName,
                destinationBackupFileName, ignoreMetadataErrors);

        /// <inheritdoc cref="IFileSystem.IFile.ResolveLinkTarget(string, bool)" />
        public FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget)
            => System.IO.File.ResolveLinkTarget(linkPath, returnFinalTarget);

        /// <inheritdoc cref="IFileSystem.IFile.SetAttributes(string, FileAttributes)" />
        public void SetAttributes(string path, FileAttributes fileAttributes)
            => System.IO.File.SetAttributes(path, fileAttributes);

        /// <inheritdoc cref="IFileSystem.IFile.SetCreationTime(string, DateTime)" />
        public void SetCreationTime(string path, DateTime creationTime)
            => System.IO.File.SetCreationTime(path, creationTime);

        /// <inheritdoc cref="IFileSystem.IFile.SetCreationTimeUtc(string, DateTime)" />
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
            => System.IO.File.SetCreationTimeUtc(path, creationTimeUtc);

        /// <inheritdoc cref="IFileSystem.IFile.SetLastAccessTime(string, DateTime)" />
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
            => System.IO.File.SetLastAccessTime(path, lastAccessTime);

        /// <inheritdoc cref="IFileSystem.IFile.SetLastAccessTimeUtc(string, DateTime)" />
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
            => System.IO.File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        /// <inheritdoc cref="IFileSystem.IFile.SetLastWriteTime(string, DateTime)" />
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
            => System.IO.File.SetLastWriteTime(path, lastWriteTime);

        /// <inheritdoc cref="IFileSystem.IFile.SetLastWriteTimeUtc(string, DateTime)" />
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
            => System.IO.File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllBytes(string, byte[])" />
        public void WriteAllBytes(string path, byte[] bytes)
            => System.IO.File.WriteAllBytes(path, bytes);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllBytesAsync(string, byte[], CancellationToken)" />
        public Task WriteAllBytesAsync(string path, byte[] bytes,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken))
            => System.IO.File.WriteAllBytesAsync(path, bytes, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, string[])" />
        public void WriteAllLines(string path, string[] contents)
            => System.IO.File.WriteAllLines(path, contents);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, IEnumerable{string})" />
        public void WriteAllLines(string path, IEnumerable<string> contents)
            => System.IO.File.WriteAllLines(path, contents);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, string[], Encoding)" />
        public void WriteAllLines(string path, string[] contents, Encoding encoding)
            => System.IO.File.WriteAllLines(path, contents, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, IEnumerable{string}, Encoding)" />
        public void WriteAllLines(string path, IEnumerable<string> contents,
                                  Encoding encoding)
            => System.IO.File.WriteAllLines(path, contents, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken))
            => System.IO.File.WriteAllLinesAsync(path, contents, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents,
                                       Encoding encoding,
                                       CancellationToken cancellationToken =
                                           default(CancellationToken))
            => System.IO.File.WriteAllLinesAsync(path, contents, encoding,
                cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllText(string, string?)" />
        public void WriteAllText(string path, string? contents)
            => System.IO.File.WriteAllText(path, contents);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllText(string, string?, Encoding)" />
        public void WriteAllText(string path, string? contents, Encoding encoding)
            => System.IO.File.WriteAllText(path, contents, encoding);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllTextAsync(string, string?, CancellationToken)" />
        public Task WriteAllTextAsync(string path, string? contents,
                                      CancellationToken cancellationToken =
                                          default(CancellationToken))
            => System.IO.File.WriteAllTextAsync(path, contents, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllTextAsync(string, string?, Encoding, CancellationToken)" />
        public Task WriteAllTextAsync(string path, string? contents, Encoding encoding,
                                      CancellationToken cancellationToken =
                                          default(CancellationToken))
            => System.IO.File.WriteAllTextAsync(path, contents, encoding,
                cancellationToken);

        #endregion
    }
}