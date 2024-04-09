#if FEATURE_SPAN
#endif
#if FEATURE_FILESYSTEM_NET7
using Testably.Abstractions.Testing.Storage;
#endif

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class MacPath(MockFileSystem fileSystem) : LinuxPath(fileSystem);
}
