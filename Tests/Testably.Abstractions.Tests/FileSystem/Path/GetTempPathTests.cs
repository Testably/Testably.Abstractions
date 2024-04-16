namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetTempPathTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetTempPath_Linux_ShouldBeTmp()
	{
		Skip.IfNot(Test.RunsOnLinux);

		string result = FileSystem.Path.GetTempPath();

		result.Should().Be("/tmp/");
	}

	[SkippableFact]
	public void GetTempPath_MacOs_ShouldBeTmp()
	{
		Skip.IfNot(Test.RunsOnMac);

		string result = FileSystem.Path.GetTempPath();

		result.Should().Match("/var/folders/??/??_*/T/");
	}

	[SkippableFact]
	public void GetTempPath_ShouldRemainTheSame()
	{
		string result1 = FileSystem.Path.GetTempPath();
		string result2 = FileSystem.Path.GetTempPath();

		result1.Should().Be(result2);
	}

	[SkippableFact]
	public void GetTempPath_Windows_ShouldBeOnDriveC()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string result = FileSystem.Path.GetTempPath();

		result.Should().StartWith(@"C:\").And.EndWith(@"\Temp\");
	}
}
