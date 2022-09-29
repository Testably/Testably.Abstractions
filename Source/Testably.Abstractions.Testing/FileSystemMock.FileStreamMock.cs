using System;
using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file stream in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileStreamMock : FileSystemStream
    {
        /// <inheritdoc cref="FileSystemStream.CanRead" />
        public override bool CanRead
            => _access.HasFlag(FileAccess.Read);

        /// <inheritdoc cref="FileSystemStream.CanWrite" />
        public override bool CanWrite
            => _access.HasFlag(FileAccess.Write);

        private readonly FileAccess _access;
        private readonly IDisposable _accessLock;
        private readonly IInMemoryFileSystem.IWritableFileInfo _file;
        private readonly FileSystemMock _fileSystem;
        private bool _isDisposed;
        private readonly FileMode _mode;
        private readonly FileOptions _options;
        private readonly MemoryStream _stream;

        internal FileStreamMock(FileSystemMock fileSystem,
                                string? path,
                                FileMode mode,
                                FileAccess access,
                                FileShare share,
                                int bufferSize,
                                FileOptions options)
            : this(new MemoryStream(), fileSystem, path, mode, access, share, bufferSize,
                options)
        {
        }

        private FileStreamMock(MemoryStream stream,
                               FileSystemMock fileSystem,
                               string? path,
                               FileMode mode,
                               FileAccess access,
                               FileShare share,
                               int bufferSize,
                               FileOptions options)
            : base(stream, path, (options & FileOptions.Asynchronous) != 0)
        {
            ThrowIfInvalidModeAccess(mode, access);

            _stream = stream;
            _fileSystem = fileSystem;
            _mode = mode;
            _access = access;
            _ = bufferSize;
            _options = options;

            IInMemoryFileSystem.IWritableFileInfo? file =
                _fileSystem.FileSystemContainer.GetFile(Name);
            if (file == null)
            {
                if (_mode.Equals(FileMode.Open) ||
                    _mode.Equals(FileMode.Truncate))
                {
                    throw ExceptionFactory.FileNotFound(_fileSystem.Path.GetFullPath(Name));
                }

                file = _fileSystem.FileSystemContainer.GetOrAddFile(Name);
                if (file == null)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw ExceptionFactory.FileAlreadyExists(_fileSystem.Path.GetFullPath(Name));
                    }

                    throw ExceptionFactory.AccessToPathDenied(_fileSystem.Path.GetFullPath(Name));
                }
            }
            else if (_mode.Equals(FileMode.CreateNew))
            {
                throw ExceptionFactory.FileAlreadyExists(_fileSystem.Path.GetFullPath(Name));
            }

            _accessLock = file.RequestAccess(access, share);

            _file = file;

            InitializeStream();
        }

        /// <inheritdoc cref="FileSystemStream.Flush()" />
        public override void Flush()
        {
            InternalFlush();
        }

        /// <inheritdoc cref="FileSystemStream.Read(byte[], int, int)" />
        public override int Read(byte[] buffer, int offset, int count)
        {
            //TimeAdjustments.LastAccessTime
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc cref="FileSystemStream.Write(byte[], int, int)" />
        public override void Write(byte[] buffer, int offset, int count)
        {
            //TimeAdjustments.LastAccessTime | TimeAdjustments.LastWriteTime
            base.Write(buffer, offset, count);
        }

        /// <inheritdoc cref="FileSystemStream.Dispose(bool)" />
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _accessLock.Dispose();
            InternalFlush();
            base.Dispose(disposing);
            OnClose();
            _isDisposed = true;
        }

        private void InitializeStream()
        {
            byte[] existingContents = _file.GetBytes();
            bool keepExistingContents =
                existingContents.Length > 0 &&
                _mode != FileMode.Truncate && _mode != FileMode.Create;
            if (keepExistingContents)
            {
                _stream.Write(existingContents, 0, existingContents.Length);
                _stream.Seek(0, _mode == FileMode.Append
                    ? SeekOrigin.End
                    : SeekOrigin.Begin);
            }
        }

        private void InternalFlush()
        {
            long position = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[Length];
            _ = _stream.Read(data, 0, (int)Length);
            _stream.Seek(position, SeekOrigin.Begin);
            _file.WriteBytes(data);
        }

        private void OnClose()
        {
            if (_options.HasFlag(FileOptions.DeleteOnClose))
            {
                _fileSystem.FileSystemContainer.Delete(Name);
            }
        }

        private static void ThrowIfInvalidModeAccess(FileMode mode, FileAccess access)
        {
            if (mode == FileMode.Append)
            {
                if (access == FileAccess.Read)
                {
                    throw ExceptionFactory.InvalidAccessCombination(mode, access);
                }

                if (access != FileAccess.Write)
                {
                    throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
                }
            }

            if (access.HasFlag(FileAccess.Read) &&
                mode == FileMode.Append)
            {
                throw ExceptionFactory.InvalidAccessCombination(mode, access);
            }

            if (!access.HasFlag(FileAccess.Write) &&
                (mode == FileMode.Truncate || mode == FileMode.CreateNew ||
                 mode == FileMode.Create || mode == FileMode.Append))
            {
                throw ExceptionFactory.InvalidAccessCombination(mode, access);
            }
        }
    }
}