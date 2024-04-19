namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class MacPath(MockFileSystem fileSystem) : LinuxPath(fileSystem)
	{
		private string? _tempPath;

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public override string GetTempPath()
		{
			_tempPath ??= $"/var/folders/{RandomString(2)}/{RandomString(2)}_{RandomString(27)}/T/";
			return _tempPath;
		}
	}
}
