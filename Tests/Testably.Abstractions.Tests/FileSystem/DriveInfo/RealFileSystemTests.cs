#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM
using Testably.Abstractions.Tests.TestHelpers.Traits;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

[Collection(nameof(RealFileSystemTests))]
[SystemTest(nameof(RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemDriveInfoTests<Abstractions.FileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemDriveInfoTests{TFileSystem}.BasePath" />
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