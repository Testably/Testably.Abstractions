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

		/// <inheritdoc cref="IPath.GetInvalidFileNameChars()" />
		public override char[] GetInvalidFileNameChars() => ['\0', '/'];

		/// <inheritdoc cref="IPath.GetInvalidPathChars()" />
		public override char[] GetInvalidPathChars() => ['\0'];

		/// <inheritdoc cref="IPath.IsPathRooted(string)" />
		public override bool IsPathRooted(string? path)
			=> path?.Length > 0 && path[0] == '/';
	}
}
