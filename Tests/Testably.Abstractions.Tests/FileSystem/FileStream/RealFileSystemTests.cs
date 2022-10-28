#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests :
	FileSystemFileStreamTests<Abstractions.RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="FileSystemFileStreamTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new Abstractions.RealFileSystem(), new Abstractions.RealTimeSystem())
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