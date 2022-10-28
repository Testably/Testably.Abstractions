#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemDirectoryTests<RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemDirectoryTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

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