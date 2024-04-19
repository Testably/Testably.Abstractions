using System;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class WindowsPath(MockFileSystem fileSystem) : SimulatedPath(fileSystem)
	{
		/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
		public override char AltDirectorySeparatorChar => '/';

		/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
		public override char DirectorySeparatorChar => '\\';

		/// <inheritdoc cref="IPath.PathSeparator" />
		public override char PathSeparator => ';';

		/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
		public override char VolumeSeparatorChar => ':';

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public override char[] GetInvalidFileNameChars() =>
		[
			'\"', '<', '>', '|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
			(char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':', '*', '?', '\\', '/'
		];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() =>
		[
			'|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
			(char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31
		];

		/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
		public override string? GetPathRoot(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return null;
			}

			return IsPathRooted(path)
				? path.Substring(0, Math.Min(3, path.Length))
				: string.Empty;
		}

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public override string GetTempPath()
			=> @"C:\Windows\Temp\";

		/// <inheritdoc cref="IPath.IsPathRooted(string)" />
		public override bool IsPathRooted(string? path)
		{
			int? length = path?.Length;
			return (length >= 1 && IsDirectorySeparator(path![0])) ||
			       (length >= 2 && IsValidDriveChar(path![0]) && path[1] == VolumeSeparatorChar);
		}

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L280
		/// </summary>
		protected override bool IsDirectorySeparator(char c)
			=> c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;

		/// <summary>
		///     Returns true if the given character is a valid drive letter
		/// </summary>
		/// <remarks>https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L72</remarks>
		private static bool IsValidDriveChar(char value)
			=> (uint)((value | 0x20) - 'a') <= 'z' - 'a';
	}
}
