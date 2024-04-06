using System.IO;
#if FEATURE_SPAN
using System;
#endif

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private class LinuxPath(MockFileSystem fileSystem) : NativePath(fileSystem)
	{
#if FEATURE_SPAN
		/// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
		public override bool IsPathRooted(ReadOnlySpan<char> path)
			=> IsPathRooted(path.ToString());
#endif

		/// <inheritdoc cref="Path.IsPathRooted(string)" />
		public override bool IsPathRooted(string? path)
			=> path?.Length > 0 && path[0] == '/';
	}
}
