namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private class LinuxPath(MockFileSystem fileSystem) : SimulatedPath(fileSystem)
	{
		private readonly MockFileSystem _fileSystem = fileSystem;

		/// <inheritdoc cref="IPath.AltDirectorySeparatorChar" />
		public override char AltDirectorySeparatorChar => '/';

		/// <inheritdoc cref="IPath.DirectorySeparatorChar" />
		public override char DirectorySeparatorChar => '/';

		/// <inheritdoc cref="IPath.PathSeparator" />
		public override char PathSeparator => ':';

		/// <inheritdoc cref="IPath.VolumeSeparatorChar" />
		public override char VolumeSeparatorChar => '/';

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public override char[] GetInvalidFileNameChars() => ['\0', '/'];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() => ['\0'];

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

			string result = collapsedString.Length == 0 ? $"{DirectorySeparatorChar}" : collapsedString;

			return result;
		}

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

		private int GetRootLength(string path)
		{
			return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
		}

		protected override bool IsDirectorySeparator(char c)
		{
			return c == '/';
		}

	}
}
