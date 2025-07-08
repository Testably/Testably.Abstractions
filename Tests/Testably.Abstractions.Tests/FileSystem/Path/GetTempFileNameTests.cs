namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetTempFileNameTests
{
	[Fact]
	public async Task GetTempFileName_ShouldBeInTempPath()
	{
		string tempPath = FileSystem.Path.GetTempPath();

#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
#pragma warning restore CS0618 // Type or member is obsolete

		await That(result).StartsWith(tempPath);
	}

	[Fact]
	public async Task GetTempFileName_ShouldExist()
	{
		#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
		#pragma warning restore CS0618 // Type or member is obsolete

		await That(FileSystem.File.Exists(result)).IsTrue();
	}
}
