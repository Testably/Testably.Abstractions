using System;
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file stream in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileStreamMock : FileSystemStream
    {
        private readonly FileSystemMock _fileSystem;
        private readonly FileMode _mode;
        private readonly FileAccess _access;
        private readonly FileShare _share;
        private readonly int _bufferSize;
        private readonly FileOptions _options;
        private readonly MemoryStream _stream;
        private bool _isDisposed;
        private readonly IDisposable _accessLock;
        private readonly IInMemoryFileSystem.IWritableFileInfo _file;

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
            if (mode == FileMode.Append)
            {
                if (access == FileAccess.Read)
                {
                    throw new ArgumentException(
                        $"Combining FileMode: {mode} with FileAccess: {access} is invalid.",
                        nameof(access));
                }

                if (access != FileAccess.Write)
                {
                    throw new ArgumentException(
                        $"{mode} access can be requested only in write-only mode.",
                        nameof(access));
                }
            }

            if (access.HasFlag(FileAccess.Read) &&
                mode == FileMode.Append)
            {
                throw new ArgumentException(
                    $"Combining FileMode: {mode} with FileAccess: {access} is invalid.",
                    nameof(access));
            }

            if (!access.HasFlag(FileAccess.Write))
            {
                if (mode == FileMode.Truncate || mode == FileMode.CreateNew ||
                    mode == FileMode.Create || mode == FileMode.Append)
                {
                    throw new ArgumentException(
                        $"Combining FileMode: {mode} with FileAccess: {access} is invalid.",
                        nameof(access));
                }
            }

            _stream = stream;
            _fileSystem = fileSystem;
            _mode = mode;
            _access = access;
            _share = share;
            _bufferSize = bufferSize;
            _options = options;

            IInMemoryFileSystem.IWritableFileInfo? file =
                _fileSystem.FileSystemContainer.GetFile(Name);
            if (file == null)
            {
                if (_mode.Equals(FileMode.Open) ||
                    _mode.Equals(FileMode.Truncate))
                {
                    throw new FileNotFoundException(
                        $"Could not find file '{_fileSystem.Path.GetFullPath(Name)}'.");
                }

                file = _fileSystem.FileSystemContainer.GetOrAddFile(Name);
                if (file == null)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        throw new IOException($"The file '{_fileSystem.Path.GetFullPath(Name)}' already exists.");
                    }
                    throw new UnauthorizedAccessException(
                        $"Access to the path '{_fileSystem.Path.GetFullPath(Name)}' is denied.");
                }
            }
            else if (_mode.Equals(FileMode.CreateNew))
            {
                throw new IOException(
                    $"The file '{_fileSystem.Path.GetFullPath(Name)}' already exists.");
            }

            _accessLock = file.RequestAccess(access, share);

            _file = file;

            InitializeStream();
        }

        private void InitializeStream()
        {
            //file.CheckFileAccess(path, access);

            //var timeAdjustments = GetTimeAdjustmentsForFileStreamWhenFileExists(mode, access);
            //mockFileDataAccessor.AdjustTimes(fileData, timeAdjustments);
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
        //private TimeAdjustments GetTimeAdjustmentsForFileStreamWhenFileExists(FileMode mode, FileAccess access)
        //{
        //    switch (mode)
        //    {
        //        case FileMode.Append:
        //        case FileMode.CreateNew:
        //            if (access.HasFlag(FileAccess.Read))
        //            {
        //                return TimeAdjustments.LastAccessTime;
        //            }
        //            return TimeAdjustments.None;
        //        case FileMode.Create:
        //        case FileMode.Truncate:
        //            if (access.HasFlag(FileAccess.Write))
        //            {
        //                return TimeAdjustments.LastAccessTime | TimeAdjustments.LastWriteTime;
        //            }
        //            return TimeAdjustments.LastAccessTime;
        //        case FileMode.Open:
        //        case FileMode.OpenOrCreate:
        //        default:
        //            return TimeAdjustments.None;
        //    }
        //}

        /// <inheritdoc />
        public override bool CanRead => _access.HasFlag(FileAccess.Read);

        /// <inheritdoc />
        public override bool CanWrite => _access.HasFlag(FileAccess.Write);

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            //mockFileDataAccessor.AdjustTimes(fileData,
            //    TimeAdjustments.LastAccessTime);
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            //mockFileDataAccessor.AdjustTimes(fileData,
            //    TimeAdjustments.LastAccessTime | TimeAdjustments.LastWriteTime);
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

        /// <inheritdoc />
        public override void Flush()
        {
            InternalFlush();
        }

        private void InternalFlush()
        {
            /* reset back to the beginning .. */
            long position = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);
            /* .. read everything out */
            byte[] data = new byte[Length];
            _stream.Read(data, 0, (int)Length);
            /* restore to original position */
            _stream.Seek(position, SeekOrigin.Begin);
            /* .. put it in the mock system */
            _file.WriteBytes(data);
        }

        private void OnClose()
        {
            if (_options.HasFlag(FileOptions.DeleteOnClose))
            {
                _fileSystem.FileSystemContainer.Delete(Name);
            }

            //if (_options.HasFlag(FileOptions.Encrypted) && mockFileDataAccessor.FileExists(path))
            //{
            //    mockFileDataAccessor.FileInfo.FromFileName(path).Encrypt();
            //}
        }
    }
}