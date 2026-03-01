namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task FileSystemExtension_ShouldBeSet()
	{
		IZipFile result = FileSystem.ZipFile();

		await That(result.FileSystem).IsSameAs(FileSystem);
	}
}
