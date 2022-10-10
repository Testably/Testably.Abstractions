using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	private sealed class PathMock : PathSystemBase
	{
		private readonly FileSystemMock _fileSystem;

		internal PathMock(FileSystemMock fileSystem)
			: base(fileSystem)
		{
			_fileSystem = fileSystem;
		}

		/// <inheritdoc cref="IFileSystem.IPath.GetFullPath(string)" />
		public override string GetFullPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return string.Empty;
			}

			return System.IO.Path.GetFullPath(System.IO.Path.Combine(
				_fileSystem.Storage.CurrentDirectory,
				path));
		}
	}
}