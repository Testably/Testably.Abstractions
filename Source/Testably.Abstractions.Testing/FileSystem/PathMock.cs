using System.IO;
using Testably.Abstractions.Helpers;
#if FEATURE_FILESYSTEM_NET7
using System.Diagnostics.CodeAnalysis;
using Testably.Abstractions.Testing.Storage;
#endif

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class PathMock : PathSystemBase
{
	private readonly MockFileSystem _fileSystem;

	internal PathMock(MockFileSystem fileSystem)
		: base(fileSystem)
	{
		_fileSystem = fileSystem;
	}

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public override bool Exists([NotNullWhen(true)] string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		return _fileSystem.Storage.GetContainer(_fileSystem.Storage.GetLocation(path))
			is not NullContainer;
	}
#endif

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
