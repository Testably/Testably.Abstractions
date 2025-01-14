namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetTempFileNameTests
{
	[SkippableFact]
	public void GetTempFileName_ShouldBeInTempPath()
	{
		string tempPath = FileSystem.Path.GetTempPath();

		string result = FileSystem.Path.GetTempFileName();

		result.Should().StartWith(tempPath);
	}

	[SkippableFact]
	public void GetTempFileName_ShouldExist()
	{
		string result = FileSystem.Path.GetTempFileName();

		FileSystem.File.Exists(result).Should().BeTrue();
	}
}
