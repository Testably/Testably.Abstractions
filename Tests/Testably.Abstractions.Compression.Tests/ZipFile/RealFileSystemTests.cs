#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

using Testably.Abstractions.Testing.FileSystemInitializer;
using Xunit.Abstractions;

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[Collection(nameof(RealFileSystemTests))]
public sealed class RealFileSystemTests :
	ZipFileTests<RealFileSystem>,
	IDisposable
{
	/// <inheritdoc cref="ZipFileTests{TFileSystem}.BasePath" />
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