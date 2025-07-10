using NSubstitute.ExceptionExtensions;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task GetVersionInfo_ArbitraryFile_ShouldHaveFileNameSet(string fileName)
	{
		string filePath = FileSystem.Path.GetFullPath(fileName);
		FileSystem.File.WriteAllText(fileName, "foo");
		if (Test.IsNetFramework)
		{
			// On .NET Framework an absolute path is required
			fileName = filePath;
		}

		IFileVersionInfo result = FileSystem.FileVersionInfo.GetVersionInfo(fileName);

		await That(result.FileName).IsEqualTo(filePath);
	}

	[Theory]
	[AutoData]
	public async Task GetVersionInfo_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		if (Test.IsNetFramework)
		{
			path = FileSystem.Path.GetFullPath(path);
		}
		
		void Act() =>
			FileSystem.FileVersionInfo.GetVersionInfo(path);

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining(FileSystem.Path.GetFullPath(path)).And
			.WithHResult(-2147024894);
	}
}
