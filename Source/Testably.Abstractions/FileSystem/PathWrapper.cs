using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.FileSystem;

internal sealed class PathWrapper : PathSystemBase
{
	public PathWrapper(RealFileSystem fileSystem)
		: base(fileSystem)
	{
	}
}