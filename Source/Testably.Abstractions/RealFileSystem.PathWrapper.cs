using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class RealFileSystem
{
	private sealed class PathWrapper : PathSystemBase
	{
		public PathWrapper(RealFileSystem fileSystem)
			: base(fileSystem)
		{
		}
	}
}