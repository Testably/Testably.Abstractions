namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class Tests
{
	[SkippableFact]
	public async Task FileSystemExtension_ShouldBeSet()
	{
		IZipFile result = FileSystem.ZipFile();

		await That(result.FileSystem).Is(FileSystem);
	}
}
