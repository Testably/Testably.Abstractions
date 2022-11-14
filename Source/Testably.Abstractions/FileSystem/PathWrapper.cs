using System.IO;
using Testably.Abstractions.Helpers;
#if FEATURE_FILESYSTEM_NET7
using System.Diagnostics.CodeAnalysis;
#endif

namespace Testably.Abstractions.FileSystem;

internal sealed class PathWrapper : PathSystemBase
{
	public PathWrapper(RealFileSystem fileSystem)
		: base(fileSystem)
	{
	}

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IPath.Exists(string)" />
	public override bool Exists([NotNullWhen(true)] string? path)
		=> Path.Exists(path);
#endif
}
