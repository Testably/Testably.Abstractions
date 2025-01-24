using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public void GetVersionInfo_ArbitraryFile_ShouldHaveFileNameSet(string fileName)
	{
		string filePath = FileSystem.Path.GetFullPath(fileName);
		FileSystem.File.WriteAllText(fileName, "foo");
		if (Test.IsNetFramework)
		{
			// On .NET Framework an absolute path is required
			fileName = filePath;
		}

		IFileVersionInfo result = FileSystem.FileVersionInfo.GetVersionInfo(fileName);

		result.FileName.Should().Be(filePath);
	}

	[Theory]
	[AutoData]
	public void GetVersionInfo_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		if (Test.IsNetFramework)
		{
			path = FileSystem.Path.GetFullPath(path);
		}
		
		Exception? exception = Record.Exception(() =>
			FileSystem.FileVersionInfo.GetVersionInfo(path));

		exception.Should().BeException<FileNotFoundException>(
			FileSystem.Path.GetFullPath(path),
			hResult: -2147024894);
	}
}
