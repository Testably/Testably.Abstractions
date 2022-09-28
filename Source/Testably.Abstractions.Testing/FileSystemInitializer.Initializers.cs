﻿using System;
using System.Collections.Generic;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
    private sealed class DirectoryInitializer<TFileSystem>
        : Initializer<TFileSystem>,
            IFileSystemDirectoryInitializer<TFileSystem>
        where TFileSystem : IFileSystem
    {
        public DirectoryInitializer(Initializer<TFileSystem> initializer,
                                    IFileSystem.IDirectoryInfo directory)
            : base(initializer)
        {
            Directory = directory;
        }

        #region IFileSystemDirectoryInitializer<TFileSystem> Members

        /// <inheritdoc cref="IFileSystemDirectoryInitializer{TFileSystem}.Directory" />
        public IFileSystem.IDirectoryInfo Directory { get; }

        /// <inheritdoc
        ///     cref="IFileSystemDirectoryInitializer{TFileSystem}.Initialized(Action{IFileSystemInitializer{TFileSystem}})" />
        public IFileSystemDirectoryInitializer<TFileSystem> Initialized(
            Action<IFileSystemInitializer<TFileSystem>> subdirectoryInitializer)
        {
            Initializer<TFileSystem> initializer = new(this, Directory);
            subdirectoryInitializer.Invoke(initializer);
            return this;
        }

        #endregion
    }

    private sealed class FileInitializer<TFileSystem>
        : Initializer<TFileSystem>,
            IFileSystemFileInitializer<TFileSystem>
        where TFileSystem : IFileSystem
    {
        public FileInitializer(Initializer<TFileSystem> initializer,
                               IFileSystem.IFileInfo file)
            : base(initializer)
        {
            File = file;
        }

        #region IFileSystemFileInitializer<TFileSystem> Members

        /// <inheritdoc cref="IFileSystemFileInitializer{TFileSystem}.File" />
        public IFileSystem.IFileInfo File { get; }

        /// <inheritdoc cref="IFileSystemFileInitializer{TFileSystem}.Which(Action{IFileManipulator})" />
        public IFileSystemFileInitializer<TFileSystem> Which(
            Action<IFileManipulator> fileManipulation)
        {
            FileManipulator fileManipulator = new(FileSystem, File);
            fileManipulation.Invoke(fileManipulator);
            return this;
        }

        #endregion

        private sealed class FileManipulator : IFileManipulator
        {
            internal FileManipulator(IFileSystem fileSystem, IFileSystem.IFileInfo file)
            {
                FileSystem = fileSystem;
                File = file;
            }

            #region IFileManipulator Members

            /// <inheritdoc cref="IFileManipulator.File" />
            public IFileSystem.IFileInfo File { get; }

            /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
            public IFileSystem FileSystem { get; }

            /// <inheritdoc cref="IFileManipulator.HasBytesContent" />
            public IFileManipulator HasBytesContent(byte[] bytes)
            {
                FileSystem.File.WriteAllBytes(File.FullName, bytes);
                return this;
            }

            /// <inheritdoc cref="IFileManipulator.HasStringContent" />
            public IFileManipulator HasStringContent(string contents)
            {
                FileSystem.File.WriteAllText(File.FullName, contents);
                return this;
            }

            #endregion
        }
    }

    private class Initializer<TFileSystem>
        : IFileSystemInitializer<TFileSystem>
        where TFileSystem : IFileSystem
    {
        private readonly string _basePath;

        private readonly Dictionary<int, IFileSystem.IFileSystemInfo>
            _initializedFileSystemInfos = new();

        public Initializer(TFileSystem fileSystem, string basePath)
        {
            _basePath = basePath;
            FileSystem = fileSystem;
        }

        protected Initializer(Initializer<TFileSystem> parent)
        {
            FileSystem = parent.FileSystem;
            _initializedFileSystemInfos = parent._initializedFileSystemInfos;
            _basePath = parent._basePath;
        }

        internal Initializer(Initializer<TFileSystem> parent,
                             IFileSystem.IDirectoryInfo subdirectory)
        {
            FileSystem = parent.FileSystem;
            _initializedFileSystemInfos = parent._initializedFileSystemInfos;
            _basePath = FileSystem.Path.Combine(parent._basePath, subdirectory.Name);
        }

        #region IFileSystemInitializer<TFileSystem> Members

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.FileSystem" />
        public TFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.this[int]" />
        public IFileSystem.IFileSystemInfo this[int index]
            => _initializedFileSystemInfos[index];

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithAFile(string?)" />
        public IFileSystemFileInitializer<TFileSystem> WithAFile(string? extension = null)
        {
            IRandomSystem randomSystem = (FileSystem as FileSystemMock)?.RandomSystem ??
                                         new RandomSystem();
            string fileName;
            do
            {
                fileName =
                    $"{randomSystem.GenerateFileName()}-{randomSystem.Random.Shared.Next(10000)}.{randomSystem.GenerateFileExtension(extension)}";
            } while (FileSystem.File.Exists(
                FileSystem.Path.Combine(_basePath, fileName)));

            return WithFile(fileName);
        }

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithASubdirectory()" />
        public IFileSystemDirectoryInitializer<TFileSystem> WithASubdirectory()
        {
            IRandomSystem randomSystem = (FileSystem as FileSystemMock)?.RandomSystem ??
                                         new RandomSystem();
            string directoryName;
            do
            {
                directoryName =
                    $"{randomSystem.GenerateFileName()}-{randomSystem.Random.Shared.Next(10000)}";
            } while (FileSystem.Directory.Exists(
                FileSystem.Path.Combine(_basePath, directoryName)));

            return WithSubdirectory(directoryName);
        }

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithFile(string)" />
        public IFileSystemFileInitializer<TFileSystem> WithFile(string fileName)
        {
            IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(
                FileSystem.Path.Combine(_basePath, fileName));
            if (fileInfo.Exists)
            {
                throw new TestingException(
                    $"The file '{fileInfo.FullName}' already exists!");
            }

            FileSystem.File.WriteAllText(fileInfo.FullName, null);
            _initializedFileSystemInfos.Add(
                _initializedFileSystemInfos.Count,
                fileInfo);

            return new FileInitializer<TFileSystem>(this, fileInfo);
        }

        /// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithSubdirectory(string)" />
        public IFileSystemDirectoryInitializer<TFileSystem> WithSubdirectory(
            string directoryName)
        {
            IFileSystem.IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(
                FileSystem.Path.Combine(_basePath, directoryName));
            if (directoryInfo.Exists)
            {
                throw new TestingException(
                    $"The directory '{directoryInfo.FullName}' already exists!");
            }

            FileSystem.Directory.CreateDirectory(directoryInfo.FullName);
            _initializedFileSystemInfos.Add(
                _initializedFileSystemInfos.Count,
                directoryInfo);

            return new DirectoryInitializer<TFileSystem>(this, directoryInfo);
        }

        #endregion
    }
}