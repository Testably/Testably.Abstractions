﻿using System;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed partial class Execute
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

		private readonly MockFileSystem _fileSystem = fileSystem;

		/// <inheritdoc cref="IPath.GetFullPath(string)" />
		public override string GetFullPath(string path)
		{
			path.EnsureValidArgument(_fileSystem, nameof(path));

			string? pathRoot = GetPathRoot(path);
			string? directoryRoot = GetPathRoot(_fileSystem.Storage.CurrentDirectory);
			string candidate;
			if (!string.IsNullOrEmpty(pathRoot) && !string.IsNullOrEmpty(directoryRoot))
			{
				if (pathRoot[0] == DirectorySeparatorChar && pathRoot.Length == 1)
				{
					candidate = directoryRoot + path.Substring(1);
				}
				else if (char.ToUpperInvariant(pathRoot[0]) != char.ToUpperInvariant(directoryRoot[0]))
				{
					candidate = path;
				}
				else if (pathRoot.Length < directoryRoot.Length)
				{
					candidate = Combine(_fileSystem.Storage.CurrentDirectory,
						path.Substring(pathRoot.Length));
				}
				else
				{
					candidate = Combine(_fileSystem.Storage.CurrentDirectory, path);
				}
			}
			else
			{
				candidate = Combine(_fileSystem.Storage.CurrentDirectory, path);
			}

			string fullPath = RemoveRelativeSegments(candidate, GetRootLength(candidate));
			fullPath = fullPath.TrimEnd('.');

			if (fullPath.Contains('\0', StringComparison.Ordinal))
			{
				throw ExceptionFactory.NullCharacterInPath(nameof(path));
			}

			if (fullPath.Length > 2 && fullPath[2] != DirectorySeparatorChar && fullPath[1] == ':')
			{
				return fullPath.Substring(0, 2) + DirectorySeparatorChar + fullPath.Substring(2);
			}

			return fullPath;
		}

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
		public override string GetFullPath(string path, string basePath)
		{
			basePath.EnsureValidArgument(_fileSystem, nameof(basePath));

			if (!IsPathFullyQualified(basePath))
			{
				throw ExceptionFactory.BasePathNotFullyQualified(nameof(basePath));
			}

			return GetFullPath(Combine(basePath, path));
		}
#endif

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public override char[] GetInvalidFileNameChars() =>
		[
			'\"', '<', '>', '|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
			(char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':', '*', '?', '\\', '/',
		];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() =>
		[
			'|', '\0',
			(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
			(char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
			(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31,
		];

		/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
		public override string? GetPathRoot(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return null;
			}

			return path.Substring(0, GetRootLength(path))
				.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
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
		///     https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L181
		/// </summary>
		protected override int GetRootLength(string path)
		{
			bool IsDeviceUNC(string p)
				=> p.Length > 7
				   && IsDevice(p)
				   && IsDirectorySeparator(p[7])
				   && p[4] == 'U'
				   && p[5] == 'N'
				   && p[6] == 'C';

			bool IsDevice(string p)
				=> IsExtended(p)
				   ||
				   (
					   p.Length > 3
					   && IsDirectorySeparator(p[0])
					   && IsDirectorySeparator(p[1])
					   && (p[2] == '.' || p[2] == '?')
					   && IsDirectorySeparator(p[3])
				   );

			bool IsExtended(string p)
				=> p.Length > 3
				   && p[0] == '\\'
				   && (p[1] == '\\' || p[1] == '?')
				   && p[2] == '?'
				   && p[3] == '\\';

			int pathLength = path.Length;

			if (pathLength > 0 && IsDirectorySeparator(path[0]))
			{
				bool deviceSyntax = IsDevice(path);
				bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

				if (deviceSyntax && !deviceUnc)
				{
					return GetRootLengthWithDeviceSyntax(path);
				}

				// UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
				if (deviceUnc || (path.Length > 1 && IsDirectorySeparator(path[1])))
				{
					return GetRootLengthWithDeviceUncSyntax(path, deviceUnc);
				}

				// Current drive rooted (e.g. "\foo")
				return 1;
			}

			if (pathLength >= 2
			    && path[1] == ':'
			    && IsValidDriveChar(path[0]))
			{
				// If the colon is followed by a directory separator, move past it (e.g "C:\")
				if (pathLength > 2 && IsDirectorySeparator(path[2]))
				{
					return 3;
				}

				// Valid drive specified path ("C:", "D:", etc.)
				return 2;
			}

			return 0;
		}

		private int GetRootLengthWithDeviceSyntax(string path)
		{
			// Device path (e.g. "\\?\.", "\\.\")
			// Skip any characters following the prefix that aren't a separator
			int i = 4;
			while (i < path.Length && !IsDirectorySeparator(path[i]))
			{
				i++;
			}

			// If there is another separator take it, as long as we have had at least one
			// non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
			if (i < path.Length && i > 4 && IsDirectorySeparator(path[i]))
			{
				i++;
			}

			return i;
		}

		private int GetRootLengthWithDeviceUncSyntax(string path,
			bool deviceUnc)
		{
			// Start past the prefix ("\\" or "\\?\UNC\")
			int i = deviceUnc ? 8 : 2;

			// Skip two separators at most
			int n = 2;
			while (i < path.Length && (!IsDirectorySeparator(path[i]) || --n > 0))
			{
				i++;
			}

			return i;
		}

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L280
		/// </summary>
		protected override bool IsDirectorySeparator(char c)
			=> c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L381
		/// </summary>
		protected override bool IsEffectivelyEmpty(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return true;
			}

			return path.All(c => c == ' ');
		}

#if FEATURE_PATH_RELATIVE
		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L250
		/// </summary>
		protected override bool IsPartiallyQualified(string path)
		{
			if (path.Length < 2)
			{
				// It isn't fixed, it must be relative.  There is no way to specify a fixed
				// path with one character (or less).
				return true;
			}

			if (IsDirectorySeparator(path[0]))
			{
				// There is no valid way to specify a relative path with two initial slashes or
				// \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
				return !(path[1] == '?' || IsDirectorySeparator(path[1]));
			}

			// The only way to specify a fixed path that doesn't begin with two slashes
			// is the drive, colon, slash format- i.e. C:\
			return !(path.Length >= 3
			         && path[1] == VolumeSeparatorChar
			         && IsDirectorySeparator(path[2])
			         // To match old behavior we'll check the drive character for validity as the path is technically
			         // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
			         && IsValidDriveChar(path[0]));
		}
#endif

		/// <summary>
		///     Returns true if the given character is a valid drive letter
		/// </summary>
		/// <remarks>https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L72</remarks>
		private static bool IsValidDriveChar(char value)
			=> (uint)((value | 0x20) - 'a') <= 'z' - 'a';

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L318
		/// </summary>
		protected override string NormalizeDirectorySeparators(string path)
		{
			bool IsAlreadyNormalized()
			{
				for (int i = 1; i < path.Length; i++)
				{
					char current = path[i];
					if (IsDirectorySeparator(current)
					    && (current != DirectorySeparatorChar
					        || (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))))
					{
						return false;
					}
				}

				return true;
			}

			if (IsAlreadyNormalized())
			{
				return path;
			}

			StringBuilder builder = new();

			int start = 0;
			if (IsDirectorySeparator(path[start]))
			{
				start++;
				builder.Append(DirectorySeparatorChar);
			}

			for (int i = start; i < path.Length; i++)
			{
				char current = path[i];

				if (IsDirectorySeparator(current))
				{
					if (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
					{
						continue;
					}

					current = DirectorySeparatorChar;
				}

				builder.Append(current);
			}

			return builder.ToString();
		}
	}
}
