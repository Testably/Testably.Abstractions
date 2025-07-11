namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class GetTempPathTests
{
	[Fact]
	public async Task GetTempPath_Linux_ShouldBeTmp()
	{
		Skip.IfNot(Test.RunsOnLinux);

		string result = FileSystem.Path.GetTempPath();

		await That(result).IsEqualTo("/tmp/");
	}

	[Fact]
	public async Task GetTempPath_MacOs_ShouldBeTmp()
	{
		Skip.IfNot(Test.RunsOnMac);

		string result = FileSystem.Path.GetTempPath();

		await That(result).IsEqualTo("/var/folders/??/*/T/").AsWildcard();
	}

	[Fact]
	public async Task GetTempPath_ShouldEndWithDirectorySeparator()
	{
		string directorySeparator = FileSystem.Path.DirectorySeparatorChar.ToString();

		string result = FileSystem.Path.GetTempPath();

		await That(result).EndsWith(directorySeparator);
	}

	[Fact]
	public async Task GetTempPath_ShouldRemainTheSame()
	{
		string result1 = FileSystem.Path.GetTempPath();
		string result2 = FileSystem.Path.GetTempPath();

		await That(result1).IsEqualTo(result2);
	}

	[Fact]
	public async Task GetTempPath_Windows_ShouldBeOnDriveC()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string result = FileSystem.Path.GetTempPath();

		await That(result).StartsWith(@"C:\").And.EndsWith(@"\Temp\");
	}
}
