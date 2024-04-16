using System.Linq;

namespace Testably.Abstractions.Testing.Helpers;

internal partial class Execute
{
	private sealed class MacPath(MockFileSystem fileSystem) : LinuxPath(fileSystem)
	{
		private readonly MockFileSystem _fileSystem = fileSystem;

		private string? _tempPath;

		/// <inheritdoc cref="IPath.GetTempPath()" />
		public override string GetTempPath()
		{
			_tempPath ??= $"/var/folders/{RandomString(2)}/{RandomString(2)}_{RandomString(27)}/T/";
			return _tempPath;
		}

		private string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[_fileSystem.RandomSystem.Random.Shared.Next(s.Length)]).ToArray());
		}
	}
}
