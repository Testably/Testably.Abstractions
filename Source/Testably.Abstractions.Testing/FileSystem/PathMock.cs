using System.IO;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class PathMock : PathSystemBase
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

		return Path.GetFullPath(Path.Combine(
			_fileSystem.Storage.CurrentDirectory,
			path));
	}
}