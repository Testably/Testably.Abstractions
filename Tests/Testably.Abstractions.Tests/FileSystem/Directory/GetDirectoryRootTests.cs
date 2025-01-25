namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetDirectoryRootTests
{
	[Fact]
	public void GetDirectoryRoot_Empty_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.GetDirectoryRoot("");
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[Fact]
	public void GetDirectoryRoot_ShouldReturnDefaultRoot()
	{
		string expectedRoot = FileTestHelper.RootDrive(Test);

		string result = FileSystem.Directory.GetDirectoryRoot("foo");

		result.Should().Be(expectedRoot);
	}

	[Theory]
	[InlineData('A')]
	[InlineData('C')]
	[InlineData('X')]
	public void GetDirectoryRoot_SpecificDrive_ShouldReturnRootWithCorrectDrive(
		char drive)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
		string expectedRoot = FileTestHelper.RootDrive(Test, "", drive);
		string path = System.IO.Path.Combine($"{drive}:\\foo", "bar");

		string result = FileSystem.Directory.GetDirectoryRoot(path);

		result.Should().Be(expectedRoot);
	}
}
