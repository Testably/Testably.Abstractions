#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

using Testably.Abstractions.Testing.FileSystemInitializer;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemFileSystemInfoTests<RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemFileSystemInfoTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new RealFileSystem(), new RealTimeSystem())
	{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory(testOutputHelper.WriteLine);
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion
}
#endif