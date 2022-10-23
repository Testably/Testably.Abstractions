#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

using Xunit.Abstractions;

namespace Testably.Abstractions.Compression.Tests;

public sealed class RealFileSystemTests :
	ZipFileTests<FileSystem>,
	IDisposable
{
	/// <inheritdoc cref="ZipFileTests{TFileSystem}.BasePath" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new FileSystem(), new TimeSystem())
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