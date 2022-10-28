using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class MockFileSystem
{
	private sealed class PathMock : PathSystemBase
	{
		private readonly MockFileSystem _fileSystem;

		internal PathMock(MockFileSystem fileSystem)
			: base(fileSystem)
		{
			_fileSystem = fileSystem;
		}

		/// <inheritdoc cref="IPath.GetFullPath(string)" />
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