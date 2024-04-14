﻿using System.Text;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private class LinuxPath(MockFileSystem fileSystem) : SimulatedPath(fileSystem)
	{
		/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
		public override char AltDirectorySeparatorChar => '/';

		/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
		public override char DirectorySeparatorChar => '/';

		/// <inheritdoc cref="IPath.PathSeparator" />
		public override char PathSeparator => ':';

		/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
		public override char VolumeSeparatorChar => '/';

		private readonly MockFileSystem _fileSystem = fileSystem;

		/// <inheritdoc cref="IPath.GetFullPath(string)" />
		public override string GetFullPath(string path)
		{
			path.EnsureValidArgument(_fileSystem, nameof(path));

			if (!IsPathRooted(path))
			{
				path = Combine(_fileSystem.Storage.CurrentDirectory, path);
			}

			// We would ideally use realpath to do this, but it resolves symlinks and requires that the file actually exist.
			string collapsedString = RemoveRelativeSegments(path, GetRootLength(path));

			string result = collapsedString.Length == 0
				? $"{DirectorySeparatorChar}"
				: collapsedString;

			return result;
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
				return GetFullPath(path);

			return GetFullPath(Combine(basePath, path));
		}
#endif

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public override char[] GetInvalidFileNameChars() => ['\0', '/'];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() => ['\0'];

		/// <inheritdoc cref="IPath.GetPathRoot(string?)" />
		public override string? GetPathRoot(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			return IsPathRooted(path)
				? $"{DirectorySeparatorChar}"
				: string.Empty;
		}

		/// <inheritdoc />
		public override string GetTempPath()
			=> "/tmp/";

		/// <inheritdoc cref="IPath.IsPathRooted(string)" />
		public override bool IsPathRooted(string? path)
			=> path?.Length > 0 && path[0] == '/';

		protected override int GetRootLength(string path)
		{
			return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
		}

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Unix.cs#L27
		/// </summary>
		protected override bool IsDirectorySeparator(char c)
		{
			return c == '/';
		}

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Unix.cs#L89
		/// </summary>
		protected override bool IsEffectivelyEmpty(string path)
			=> string.IsNullOrEmpty(path);

		/// <summary>
		///     https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Unix.cs#L39
		/// </summary>
		protected override string? NormalizeDirectorySeparators(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}

			// Make a pass to see if we need to normalize so we can potentially skip allocating
			bool normalized = true;

			for (int i = 0; i < path.Length; i++)
			{
				if (IsDirectorySeparator(path[i])
				    && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
				{
					normalized = false;
					break;
				}
			}

			if (normalized)
			{
				return path;
			}

			StringBuilder builder = new(path.Length);

			for (int i = 0; i < path.Length; i++)
			{
				char current = path[i];

				// Skip if we have another separator following
				if (IsDirectorySeparator(current)
				    && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
				{
					continue;
				}

				builder.Append(current);
			}

			return builder.ToString();
		}

		/// <summary>
		/// https://github.com/dotnet/runtime/blob/v8.0.4/src/libraries/Common/src/System/IO/PathInternal.Unix.cs#L77
		/// </summary>
		protected override bool IsPartiallyQualified(string path)
			=> !IsPathRooted(path);
	}
}
