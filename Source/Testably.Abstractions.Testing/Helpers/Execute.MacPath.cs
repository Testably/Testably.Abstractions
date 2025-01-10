namespace Testably.Abstractions.Testing.Helpers;

internal sealed partial class Execute
{
	private sealed class MacPath(MockFileSystem fileSystem) : LinuxPath(fileSystem)
	{
		private readonly MockFileSystem _fileSystem = fileSystem;
		private string? _tempPath;

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public override string GetTempPath()
		{
			_tempPath ??=
				$"/var/folders/{RandomString(_fileSystem, 2)}/{RandomString(_fileSystem, 2)}_{RandomString(_fileSystem, 27)}/T/";
			return _tempPath;
		}
	}
}
