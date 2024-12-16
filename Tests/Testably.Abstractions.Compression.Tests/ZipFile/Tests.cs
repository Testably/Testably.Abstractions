namespace Testably.Abstractions.Compression.Tests.ZipFile;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public async Task FileSystemExtension_ShouldBeSet()
	{
		IZipFile result = FileSystem.ZipFile();

		result.FileSystem.Should().Be(FileSystem);
	}
}
