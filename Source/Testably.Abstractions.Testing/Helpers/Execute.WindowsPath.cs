using System.IO;
#if FEATURE_SPAN
using System;
#endif

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class WindowsPath(MockFileSystem fileSystem) : NativePath(fileSystem)
	{
#if FEATURE_SPAN
		/// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
		public override bool IsPathRooted(ReadOnlySpan<char> path)
			=> IsPathRooted(path.ToString());
#endif

		/// <inheritdoc cref="Path.IsPathRooted(string)" />
		public override bool IsPathRooted(string? path)
		{
			int? length = path?.Length;
			return (length >= 1 && IsDirectorySeparator(path![0])) ||
			       (length >= 2 && IsValidDriveChar(path![0]) && path[1] == VolumeSeparatorChar);
		}

		private static bool IsDirectorySeparator(char c)
			=> c == '\\' || c == '/';

		private static bool IsValidDriveChar(char value)
			=> (uint)((value | 0x20) - 'a') <= 'z' - 'a';
	}
}
