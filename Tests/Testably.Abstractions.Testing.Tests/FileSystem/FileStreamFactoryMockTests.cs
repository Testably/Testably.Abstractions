using System.IO;
using Testably.Abstractions.Testing.FileSystemInitializer;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

[Collection(nameof(IDirectoryCleaner))]
public sealed class FileStreamFactoryMockTests : IDisposable
{
	public string BasePath => _directoryCleaner.BasePath;
	public MockFileSystem MockFileSystem { get; }
	public RealFileSystem RealFileSystem { get; }
	private readonly IDirectoryCleaner _directoryCleaner;

	public FileStreamFactoryMockTests()
	{
		RealFileSystem = new RealFileSystem();
		_directoryCleaner = RealFileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
		MockFileSystem = new MockFileSystem();
		MockFileSystem.InitializeIn(_directoryCleaner.BasePath);
	}

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[SkippableTheory]
	[AutoData]
	public void
		Wrap_ShouldThrowNotSupportedException(
			string path, string contents)
	{
		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.Wrap(new FileStream(path, FileMode.Open));
		});

		exception.Should().BeOfType<NotSupportedException>();
	}
}
