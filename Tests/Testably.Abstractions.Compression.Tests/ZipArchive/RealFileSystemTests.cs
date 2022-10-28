#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

using Xunit.Abstractions;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

[Collection(nameof(RealFileSystemTests))]
public sealed class RealFileSystemTests :
	ZipArchiveTests<RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="ZipArchiveTests{TFileSystem}.BasePath" />
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