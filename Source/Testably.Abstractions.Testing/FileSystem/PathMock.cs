using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
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
		path.EnsureValidArgument(FileSystem, nameof(path));

		return Path.GetFullPath(Path.Combine(
			_fileSystem.Storage.CurrentDirectory,
			path));
	}

#if FEATURE_PATH_RELATIVE
	/// <inheritdoc cref="IPath.GetRelativePath(string, string)" />
	public override string GetRelativePath(string relativeTo, string path)
	{
		relativeTo.EnsureValidArgument(FileSystem, nameof(relativeTo));
		path.EnsureValidArgument(FileSystem, nameof(path));

		relativeTo = FileSystem.Path.GetFullPath(relativeTo);
		path = FileSystem.Path.GetFullPath(path);

		return Path.GetRelativePath(relativeTo, path);
	}
#endif
}
