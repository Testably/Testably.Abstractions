using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class WindowsPath(MockFileSystem fileSystem, bool isNetFramework)
		: SimulatedPath(fileSystem)
	{
		private const int DevicePrefixLength = 4;
		private const int UncExtendedPrefixLength = 8;
		private const int UncPrefixLength = 2;

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

			if (IsExtended(path))
			{
				return path;
			}

			string? pathRoot = GetPathRoot(path);
			string? directoryRoot = GetPathRoot(_fileSystem.Storage.CurrentDirectory);
			string candidate;
			if (!string.IsNullOrEmpty(pathRoot) && !string.IsNullOrEmpty(directoryRoot))
			{
				if (char.ToUpperInvariant(pathRoot[0]) != char.ToUpperInvariant(directoryRoot[0]))
				{
					candidate = path;
				}
				else if (pathRoot.Length < directoryRoot.Length)
				{
					candidate = Combine(_fileSystem.Storage.CurrentDirectory, path.Substring(pathRoot.Length));
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

			string fullPath =
				NormalizeDirectorySeparators(RemoveRelativeSegments(candidate,
					GetRootLength(candidate)));
			fullPath = fullPath.TrimEnd('.');

			if (fullPath.Contains('\0', StringComparison.Ordinal))
			{
				throw ExceptionFactory.NullCharacterInPath(nameof(path));
			}

			if (fullPath.Length > 2 && fullPath[1] == ':' && fullPath[2] != DirectorySeparatorChar)
			{
				return fullPath.Substring(0, 2) + DirectorySeparatorChar + fullPath.Substring(2);
			}

			return fullPath;
		}

#if FEATURE_PATH_RELATIVE
		/// <inheritdoc cref="IPath.GetFullPath(string, string)" />
		public override string GetFullPath(string path, string basePath)
		{
			path.EnsureValidArgument(_fileSystem, nameof(path));
			basePath.EnsureValidArgument(_fileSystem, nameof(basePath));

			if (!IsPathFullyQualified(basePath))
			{
				throw ExceptionFactory.BasePathNotFullyQualified(nameof(basePath));
			}

			if (IsPathFullyQualified(path))
			{
				return GetFullPath(path);
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
			(char)26, (char)27, (char)28, (char)29, (char)30, (char)31, ':', '*', '?', '\\', '/'
		];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() =>
			isNetFramework
				?
				[
					'"', '<', '>', '|', '\0',
					(char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9,
					(char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17,
					(char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25,
					(char)26, (char)27, (char)28, (char)29, (char)30, (char)31
				]
				:
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
				? path.Substring(0,3)
				: string.Empty;
		}

		/// <inheritdoc />
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
		///     Gets the length of the root of the path (drive, share, etc.).
		/// </summary>
		protected override int GetRootLength(string path)
		{
			int pathLength = path.Length;
			int i = 0;

			bool deviceSyntax = IsDevice(path);
			bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

			if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
			{
				// UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
				if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
				{
					// UNC (\\?\UNC\ or \\), scan past server\share

					// Start past the prefix ("\\" or "\\?\UNC\")
					i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

					// Skip two separators at most
					int n = 2;
					while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
					{
						i++;
					}
				}
				else
				{
					// Current drive rooted (e.g. "\foo")
					i = 1;
				}
			}
			else if (deviceSyntax)
			{
				// Device path (e.g. "\\?\.", "\\.\")
				// Skip any characters following the prefix that aren't a separator
				i = DevicePrefixLength;
				while (i < pathLength && !IsDirectorySeparator(path[i]))
				{
					i++;
				}

				// If there is another separator take it, as long as we have had at least one
				// non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
				if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
				{
					i++;
				}
			}
			else if (pathLength >= 2
			         && path[1] == ':'
			         && IsValidDriveChar(path[0]))
			{
				// Valid drive specified path ("C:", "D:", etc.)
				i = 2;

				// If the colon is followed by a directory separator, move past it (e.g "C:\")
				if (pathLength > 2 && IsDirectorySeparator(path[2]))
				{
					i++;
				}
			}

			return i;
		}

		/// <summary>
		///     Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
		/// </summary>
		private bool IsDevice(string path)
		{
			// If the path begins with any two separators is will be recognized and normalized and prepped with
			// "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
			return IsExtended(path)
			       ||
			       (
				       path.Length >= DevicePrefixLength
				       && IsDirectorySeparator(path[0])
				       && IsDirectorySeparator(path[1])
				       && (path[2] == '.' || path[2] == '?')
				       && IsDirectorySeparator(path[3])
			       );
		}

		/// <summary>
		///     Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
		/// </summary>
		private bool IsDeviceUNC(string path)
		{
			return path.Length >= UncExtendedPrefixLength
			       && IsDevice(path)
			       && IsDirectorySeparator(path[7])
			       && path[4] == 'U'
			       && path[5] == 'N'
			       && path[6] == 'C';
		}

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L280
		/// </summary>
		protected override bool IsDirectorySeparator(char c)
			=> c == '\\' || c == '/';

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

		/// <summary>
		///     Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
		///     path matches exactly (cannot use alternate directory separators) Windows will skip normalization
		///     and path length checks.
		/// </summary>
		private static bool IsExtended(string path)
		{
			// While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
			// Skipping of normalization will *only* occur if back slashes ('\') are used.
			return path.Length >= DevicePrefixLength
			       && path[0] == '\\'
			       && (path[1] == '\\' || path[1] == '?')
			       && path[2] == '?'
			       && path[3] == '\\';
		}

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

		/// <summary>
		///     Returns true if the given character is a valid drive letter
		/// </summary>
		/// <remarks>https://github.com/dotnet/runtime/blob/v8.0.3/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L72</remarks>
		private static bool IsValidDriveChar(char value)
			=> (uint)((value | 0x20) - 'a') <= 'z' - 'a';

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Windows.cs#L318
		/// </summary>
		[return: NotNullIfNotNull(nameof(path))]
		protected override string? NormalizeDirectorySeparators(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			char current;

			// Make a pass to see if we need to normalize so we can potentially skip allocating
			bool normalized = true;

			for (int i = 0; i < path.Length; i++)
			{
				current = path[i];
				if (IsDirectorySeparator(current)
				    && (current != '\\'
				        // Check for sequential separators past the first position (we need to keep initial two for UNC/extended)
				        || (i > 0 && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))))
				{
					normalized = false;
					break;
				}
			}

			if (normalized)
			{
				return path;
			}

			StringBuilder builder = new();

			int start = 0;
			if (IsDirectorySeparator(path[start]))
			{
				start++;
				builder.Append('\\');
			}

			for (int i = start; i < path.Length; i++)
			{
				current = path[i];

				// If we have a separator
				if (IsDirectorySeparator(current))
				{
					// If the next is a separator, skip adding this
					if (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
					{
						continue;
					}

					// Ensure it is the primary separator
					current = '\\';
				}

				builder.Append(current);
			}

			return builder.ToString();
		}
	}
}
