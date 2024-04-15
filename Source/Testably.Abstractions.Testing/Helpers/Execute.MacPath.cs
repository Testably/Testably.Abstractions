namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class MacPath(MockFileSystem fileSystem) : LinuxPath(fileSystem);
}
