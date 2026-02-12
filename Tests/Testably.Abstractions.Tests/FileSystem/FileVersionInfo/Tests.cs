using System.IO;

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
	
	[Theory]
	[AutoData]
	public async Task Attributes_WhenDotFile_ShouldHaveHiddenFlag(bool isFile)
	{
		Skip.If(Test.RunsOnWindows);

		const string path = ".env";

		FileAttributes result;

		if (isFile)
		{
			FileSystem.File.WriteAllText(path, null);

			result = FileSystem.FileInfo.New(path).Attributes;
		}
		else
		{
			FileSystem.Directory.CreateDirectory(path);

			result = FileSystem.DirectoryInfo.New(path).Attributes;
		}

		await That(result).HasFlag(FileAttributes.Hidden);
	}
}
