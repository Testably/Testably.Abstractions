#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using Testably.Abstractions.Tests.TestHelpers.Traits;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
[SystemTest(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemDirectoryInfoTests<Abstractions.FileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemDirectoryInfoTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new Abstractions.FileSystem(), new Abstractions.TimeSystem())
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