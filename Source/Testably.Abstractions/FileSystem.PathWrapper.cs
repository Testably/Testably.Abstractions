using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class PathWrapper : PathSystemBase
	{
		public PathWrapper(FileSystem fileSystem)
			: base(fileSystem)
		{
		}
	}
}