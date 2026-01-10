using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

[Collection(nameof(IDirectoryCleaner))]
public sealed class FileStreamFactoryMockTests : IDisposable
{
	#region Test Setup

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

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _directoryCleaner.Dispose();

	#endregion

	[Theory]
	[AutoData]
	public async Task Wrap_ShouldThrowNotSupportedException(
		string path, string contents)
	{
		path = RealFileSystem.Path.GetFullPath(path);
		RealFileSystem.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			// ReSharper disable once AccessToDisposedClosure
			MockFileSystem.FileStream.Wrap(new FileStream(path, FileMode.Open));
		});

		await That(exception).IsExactly<NotSupportedException>();
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Theory]
	[InlineAutoData(FileMode.CreateNew)]
	[InlineAutoData(FileMode.Create)]
	[InlineAutoData(FileMode.OpenOrCreate)]
	[InlineAutoData(FileMode.Append)]
	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
		Justification = "Skip.If(Test.RunsOnWindows) handles platform check.")]
	public async Task New_ShouldSetUnixFileMode(FileMode mode, string path)
	{
		Skip.If(Test.RunsOnWindows);

		var options = new FileStreamOptions
		{
			Access = FileAccess.Write,
			Mode = mode,
			UnixCreateMode = UnixFileMode.UserRead,
		};

		await using (MockFileSystem.FileStream.New(path, options)) { }
		var result = MockFileSystem.File.GetUnixFileMode(path);
		
		await That(result).IsEqualTo(options.UnixCreateMode);
	}

	[Theory]
	[InlineAutoData(FileMode.Open)]
	[InlineAutoData(FileMode.Truncate)]
	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
		Justification = "Skip.If(Test.RunsOnWindows) handles platform check.")]
	public async Task New_ShouldThrowArgumentException_When_InvalidFileMode(FileMode mode,
		string path)
	{
		Skip.If(Test.RunsOnWindows);

		MockFileSystem.File.Create(path);
		var options = new FileStreamOptions
		{
			Access = FileAccess.Write,
			Mode = mode,
			UnixCreateMode = UnixFileMode.UserRead,
		};

		Exception? exception = Record.Exception(() =>
		{
			using (MockFileSystem.FileStream.New(path, options)) { }
		});

		await That(exception).IsExactly<ArgumentException>();
	}
#endif
}
