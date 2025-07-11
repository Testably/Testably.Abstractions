namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task ToString_ShouldReturnProvidedPath(string fileName)
	{
		FileSystem.File.WriteAllText(fileName, "");
		string fullPath = FileSystem.Path.GetFullPath(fileName);
		IFileVersionInfo fileInfo = FileSystem.FileVersionInfo.GetVersionInfo(fullPath);

		string? result = fileInfo.ToString();

		await That(result).Contains(fullPath);
	}
}
