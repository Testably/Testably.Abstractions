using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing;

public sealed partial class MockFileSystem
{
	private sealed class FileInfoFactoryMock : IFileInfoFactory
	{
		private readonly MockFileSystem _fileSystem;

		internal FileInfoFactoryMock(MockFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IFileInfoFactory Members

		/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IFileInfoFactory.New(string)" />
		public IFileInfo New(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			return FileInfoMock.New(
				_fileSystem.Storage.GetLocation(fileName),
				_fileSystem);
		}

		/// <inheritdoc cref="IFileInfoFactory.Wrap(FileInfo)" />
		[return: NotNullIfNotNull("fileInfo")]
		public IFileInfo? Wrap(FileInfo? fileInfo)
			=> FileInfoMock.New(
				_fileSystem.Storage.GetLocation(
					fileInfo?.FullName,
					fileInfo?.ToString()),
				_fileSystem);

		#endregion
	}
}