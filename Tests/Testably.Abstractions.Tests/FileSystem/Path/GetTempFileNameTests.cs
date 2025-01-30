namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetTempFileNameTests
{
	[Fact]
	public void GetTempFileName_ShouldBeInTempPath()
	{
		string tempPath = FileSystem.Path.GetTempPath();

		#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
		#pragma warning restore CS0618 // Type or member is obsolete

		result.Should().StartWith(tempPath);
	}

	[Fact]
	public void GetTempFileName_ShouldExist()
	{
		#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
		#pragma warning restore CS0618 // Type or member is obsolete

		FileSystem.File.Exists(result).Should().BeTrue();
	}
}
