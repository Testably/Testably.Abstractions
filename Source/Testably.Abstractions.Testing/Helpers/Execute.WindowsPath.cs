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

		/// <summary>
		///     True if the given character is a directory separator.
		/// </summary>
		/// <remarks>https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L280</remarks>
		private static bool IsDirectorySeparator(char c)
			=> c == '\\' || c == '/';

		/// <summary>
		///     Returns true if the given character is a valid drive letter
		/// </summary>
		/// <remarks>https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L72</remarks>
		private static bool IsValidDriveChar(char value)
			=> (uint)((value | 0x20) - 'a') <= 'z' - 'a';
	}
}
