using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Testably.Abstractions.Testing.Internal;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class FileMock : IFileSystem.IFile
    {
        private readonly FileSystemMock _fileSystem;

        internal FileMock(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #region IFile Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLines(string, IEnumerable{string})" />
        public void AppendAllLines(string path, IEnumerable<string> contents)
            => AppendAllLines(path, contents, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLines(string, IEnumerable{string}, Encoding)" />
        public void AppendAllLines(
            string path,
            IEnumerable<string> contents,
            Encoding encoding)
            => AppendAllText(
                path,
                contents.Aggregate(string.Empty, (a, b) => a + b + Environment.NewLine),
                encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                        CancellationToken cancellationToken = default)
            => AppendAllLinesAsync(path, contents, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents,
                                        Encoding encoding,
                                        CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            AppendAllLines(path, contents, encoding);
            return Task.CompletedTask;
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllText(string, string?)" />
        public void AppendAllText(string path, string? contents)
            => AppendAllText(path, contents, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllText(string, string?, Encoding)" />
        public void AppendAllText(string path, string? contents, Encoding encoding)

        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetOrAddFile(path);
            if (fileInfo != null && contents != null)
            {
                using (fileInfo.RequestAccess(
                    FileAccess.ReadWrite,
                    FileStreamFactoryMock.DefaultShare))
                {
                    fileInfo.AppendBytes(encoding.GetBytes(contents));
                }
            }
        }

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.AppendAllTextAsync(string, string?, CancellationToken)" />
        public Task AppendAllTextAsync(string path, string? contents,
                                       CancellationToken cancellationToken = default)
            => AppendAllTextAsync(path, contents, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.AppendAllTextAsync(string, string?, Encoding, CancellationToken)" />
        public Task AppendAllTextAsync(string path, string? contents, Encoding encoding,
                                       CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            AppendAllText(path, contents, encoding);
            return Task.CompletedTask;
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.AppendText(string)" />
        public StreamWriter AppendText(string path)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.Copy(string, string)" />
        public void Copy(string sourceFileName, string destFileName)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.Copy(string, string, bool)" />
        public void Copy(string sourceFileName, string destFileName, bool overwrite)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.Create(string)" />
        public FileSystemStream Create(string path)
            => new FileStreamMock(
                _fileSystem,
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFile.Create(string, int)" />
        public FileSystemStream Create(string path, int bufferSize)
            => new FileStreamMock(
                _fileSystem,
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize);

        /// <inheritdoc cref="IFileSystem.IFile.Create(string, int, FileOptions)" />
        public FileSystemStream Create(string path, int bufferSize, FileOptions options)
            => new FileStreamMock(
                _fileSystem,
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize,
                options);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFile.CreateSymbolicLink(string, string)" />
        public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFile.CreateText(string)" />
        public StreamWriter CreateText(string path)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.Decrypt(string)" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Decrypt(string path)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetOrAddFile(path);
            if (fileInfo != null &&
                fileInfo.Attributes.HasFlag(FileAttributes.Encrypted))
            {
                fileInfo.Attributes &= ~FileAttributes.Encrypted;
                fileInfo.Decrypt();
            }
        }

        /// <inheritdoc cref="IFileSystem.IFile.Delete(string)" />
        public void Delete(string path)
        {
            if (!_fileSystem.Storage.Delete(path))
            {
                throw ExceptionFactory.FileNotFound(
                    _fileSystem.Path.GetFullPath(path));
            }
        }

        /// <inheritdoc cref="IFileSystem.IFile.Encrypt(string)" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Encrypt(string path)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetOrAddFile(path);
            if (fileInfo != null &&
                !fileInfo.Attributes.HasFlag(FileAttributes.Encrypted))
            {
                fileInfo.Attributes |= FileAttributes.Encrypted;
                fileInfo.Encrypt();
            }
        }

        /// <inheritdoc cref="IFileSystem.IFile.Exists(string?)" />
        public bool Exists([NotNullWhen(true)] string? path)
            => _fileSystem.Storage.Exists(path);

        /// <inheritdoc cref="IFileSystem.IFile.GetAttributes(string)" />
        public FileAttributes GetAttributes(string path)
            => _fileSystem.Storage.GetFile(path)?.Attributes
               ?? throw ExceptionFactory.FileNotFound(FileSystem.Path.GetFullPath(path));

        /// <inheritdoc cref="IFileSystem.IFile.GetCreationTime(string)" />
        public DateTime GetCreationTime(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).CreationTime;

        /// <inheritdoc cref="IFileSystem.IFile.GetCreationTimeUtc(string)" />
        public DateTime GetCreationTimeUtc(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).CreationTimeUtc;

        /// <inheritdoc cref="IFileSystem.IFile.GetLastAccessTime(string)" />
        public DateTime GetLastAccessTime(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).LastAccessTime;

        /// <inheritdoc cref="IFileSystem.IFile.GetLastAccessTimeUtc(string)" />
        public DateTime GetLastAccessTimeUtc(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).LastAccessTimeUtc;

        /// <inheritdoc cref="IFileSystem.IFile.GetLastWriteTime(string)" />
        public DateTime GetLastWriteTime(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).LastWriteTime;

        /// <inheritdoc cref="IFileSystem.IFile.GetLastWriteTimeUtc(string)" />
        public DateTime GetLastWriteTimeUtc(string path)
            => (_fileSystem.Storage.GetFile(path) ??
                _fileSystem.NullFileSystemInfo).LastWriteTimeUtc;

        /// <inheritdoc cref="IFileSystem.IFile.Move(string, string)" />
        public void Move(string sourceFileName, string destFileName)
            => throw new NotImplementedException();

#if FEATURE_FILE_MOVETO_OVERWRITE
        /// <inheritdoc cref="IFileSystem.IFile.Move(string, string, bool)" />
        public void Move(string sourceFileName, string destFileName, bool overwrite)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode)" />
        public FileSystemStream Open(string path, FileMode mode)
            => new FileStreamMock(
                _fileSystem,
                path,
                mode,
                mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode, FileAccess)" />
        public FileSystemStream Open(string path, FileMode mode, FileAccess access)
            => new FileStreamMock(
                _fileSystem,
                path,
                mode,
                access,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileMode, FileAccess, FileShare)" />
        public FileSystemStream Open(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share)
            => new FileStreamMock(
                _fileSystem,
                path,
                mode,
                access,
                share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
        /// <inheritdoc cref="IFileSystem.IFile.Open(string, FileStreamOptions)" />
        public FileSystemStream Open(string path, FileStreamOptions options)
            => new FileStreamMock(
                _fileSystem,
                path,
                options.Mode,
                options.Access,
                options.Share,
                options.BufferSize,
                options.Options);
#endif

        /// <inheritdoc cref="IFileSystem.IFile.OpenRead(string)" />
        public FileSystemStream OpenRead(string path)
            => new FileStreamMock(
                _fileSystem,
                path,
                FileMode.Open,
                FileAccess.Read);

        /// <inheritdoc cref="IFileSystem.IFile.OpenText(string)" />
        public StreamReader OpenText(string path)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.OpenWrite(string)" />
        public FileSystemStream OpenWrite(string path)
            => new FileStreamMock(
                _fileSystem,
                path,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllBytes(string)" />
        public byte[] ReadAllBytes(string path)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo != null)
            {
                using (fileInfo.RequestAccess(
                    FileAccess.Read,
                    FileStreamFactoryMock.DefaultShare))
                {
                    return fileInfo.GetBytes();
                }
            }

            throw ExceptionFactory.FileNotFound(_fileSystem.Path.GetFullPath(path));
        }

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.ReadAllBytesAsync(string, CancellationToken)" />
        public Task<byte[]> ReadAllBytesAsync(string path,
                                              CancellationToken cancellationToken =
                                                  default)
        {
            ThrowIfCancelled(cancellationToken);
            return Task.FromResult(ReadAllBytes(path));
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLines(string)" />
        public string[] ReadAllLines(string path)
            => ReadAllLines(path, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLines(string, Encoding)" />
        public string[] ReadAllLines(string path, Encoding encoding)
            => ReadLines(path, encoding).ToArray();

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLinesAsync(string, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(
            string path,
            CancellationToken cancellationToken = default)
            => ReadAllLinesAsync(path, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(
            string path,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            return Task.FromResult(ReadAllLines(path, encoding));
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllText(string)" />
        public string ReadAllText(string path)
            => ReadAllText(path, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllText(string, Encoding)" />
        public string ReadAllText(string path, Encoding encoding)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo != null)
            {
                using (fileInfo.RequestAccess(
                    FileAccess.Read,
                    FileStreamFactoryMock.DefaultShare))
                {
                    return encoding.GetString(fileInfo.GetBytes());
                }
            }

            throw ExceptionFactory.FileNotFound(_fileSystem.Path.GetFullPath(path));
        }

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.ReadAllTextAsync(string, CancellationToken)" />
        public Task<string> ReadAllTextAsync(
            string path,
            CancellationToken cancellationToken = default)
            => ReadAllTextAsync(path, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.ReadAllTextAsync(string, Encoding, CancellationToken)" />
        public Task<string> ReadAllTextAsync(
            string path,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            return Task.FromResult(ReadAllText(path, encoding));
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.ReadLines(string)" />
        public IEnumerable<string> ReadLines(string path)
            => ReadLines(path, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.ReadLines(string, Encoding)" />
        public IEnumerable<string> ReadLines(string path, Encoding encoding)
            => EnumerateLines(ReadAllText(path, encoding));

        /// <inheritdoc cref="IFileSystem.IFile.Replace(string, string, string)" />
        public void Replace(string sourceFileName, string destinationFileName,
                            string? destinationBackupFileName)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFile.Replace(string, string, string, bool)" />
        public void Replace(string sourceFileName, string destinationFileName,
                            string? destinationBackupFileName, bool ignoreMetadataErrors)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFile.ResolveLinkTarget(string, bool)" />
        public FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFile.SetAttributes(string, FileAttributes)" />
        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.Attributes = fileAttributes;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetCreationTime(string, DateTime)" />
        public void SetCreationTime(string path, DateTime creationTime)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.CreationTime = creationTime;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetCreationTimeUtc(string, DateTime)" />
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.CreationTimeUtc = creationTimeUtc;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetLastAccessTime(string, DateTime)" />
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.LastAccessTime = lastAccessTime;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetLastAccessTimeUtc(string, DateTime)" />
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.LastAccessTimeUtc = lastAccessTimeUtc;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetLastWriteTime(string, DateTime)" />
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.LastWriteTime = lastWriteTime;
        }

        /// <inheritdoc cref="IFileSystem.IFile.SetLastWriteTimeUtc(string, DateTime)" />
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            IFileSystem.IFileInfo? fileInfo =
                _fileSystem.Storage.GetFile(path);
            if (fileInfo == null)
            {
                throw ExceptionFactory.FileNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileInfo.LastWriteTimeUtc = lastWriteTimeUtc;
        }

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllBytes(string, byte[])" />
        public void WriteAllBytes(string path, byte[] bytes)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetOrAddFile(path);
            if (fileInfo != null)
            {
                using (fileInfo.RequestAccess(
                    FileAccess.Write,
                    FileStreamFactoryMock.DefaultShare))
                {
                    fileInfo.WriteBytes(bytes);
                }
            }
        }

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.WriteAllBytesAsync(string, byte[], CancellationToken)" />
        public Task WriteAllBytesAsync(string path, byte[] bytes,
                                       CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            WriteAllBytes(path, bytes);
            return Task.CompletedTask;
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, string[])" />
        public void WriteAllLines(string path, string[] contents)
            => WriteAllLines(path, contents, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, IEnumerable{string})" />
        public void WriteAllLines(string path, IEnumerable<string> contents)
            => WriteAllLines(path, contents, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, string[], Encoding)" />
        public void WriteAllLines(
            string path,
            string[] contents,
            Encoding encoding)
            => WriteAllLines(path, contents.AsEnumerable(), encoding);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLines(string, IEnumerable{string}, Encoding)" />
        public void WriteAllLines(
            string path,
            IEnumerable<string> contents,
            Encoding encoding)
            => WriteAllText(
                path,
                contents.Aggregate(string.Empty, (a, b) => a + b + Environment.NewLine),
                encoding);

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents,
            CancellationToken cancellationToken = default)
            => WriteAllLinesAsync(path, contents, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task WriteAllLinesAsync(
            string path,
            IEnumerable<string> contents,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            WriteAllLines(path, contents, encoding);
            return Task.CompletedTask;
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllText(string, string?)" />
        public void WriteAllText(string path, string? contents)
            => WriteAllText(path, contents, Encoding.Default);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllText(string, string?, Encoding)" />
        public void WriteAllText(string path, string? contents, Encoding encoding)
        {
            IStorage.IFileInfoMock? fileInfo =
                _fileSystem.Storage.GetOrAddFile(path);
            if (fileInfo != null && contents != null)
            {
                using (fileInfo.RequestAccess(
                    FileAccess.Write,
                    FileStreamFactoryMock.DefaultShare))
                {
                    fileInfo.WriteBytes(encoding.GetBytes(contents));
                }
            }
        }

#if FEATURE_FILESYSTEM_ASYNC
        /// <inheritdoc cref="IFileSystem.IFile.WriteAllTextAsync(string, string?, CancellationToken)" />
        public Task WriteAllTextAsync(string path, string? contents,
                                      CancellationToken cancellationToken = default)
            => WriteAllTextAsync(path, contents, Encoding.Default, cancellationToken);

        /// <inheritdoc cref="IFileSystem.IFile.WriteAllTextAsync(string, string?, Encoding, CancellationToken)" />
        public Task WriteAllTextAsync(string path, string? contents, Encoding encoding,
                                      CancellationToken cancellationToken = default)
        {
            ThrowIfCancelled(cancellationToken);
            WriteAllText(path, contents, encoding);
            return Task.CompletedTask;
        }
#endif

        #endregion

#if FEATURE_FILESYSTEM_ASYNC
        private static void ThrowIfCancelled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw ExceptionFactory.TaskWasCanceled();
            }
        }
#endif

        private static IEnumerable<string> EnumerateLines(string contents)
        {
            if (string.IsNullOrEmpty(contents))
            {
                yield break;
            }

            using (StringReader reader = new(contents))
            {
                while (reader.ReadLine() is { } line)
                {
                    yield return line;
                }
            }
        }
    }
}