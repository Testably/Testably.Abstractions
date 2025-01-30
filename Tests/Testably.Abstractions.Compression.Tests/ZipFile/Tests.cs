namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public async Task FileSystemExtension_ShouldBeSet()
	{
		IZipFile result = FileSystem.ZipFile();

		await That(result.FileSystem).IsSameAs(FileSystem);
	}
}
