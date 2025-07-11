namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class GetDirectoryRootTests
{
	[Fact]
	public async Task GetDirectoryRoot_Empty_ShouldThrowArgumentException()
	{
		void Act()
		{
			FileSystem.Directory.GetDirectoryRoot("");
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Fact]
	public async Task GetDirectoryRoot_ShouldReturnDefaultRoot()
	{
		string expectedRoot = FileTestHelper.RootDrive(Test);

		string result = FileSystem.Directory.GetDirectoryRoot("foo");

		await That(result).IsEqualTo(expectedRoot);
	}

	[Theory]
	[InlineData('A')]
	[InlineData('C')]
	[InlineData('X')]
	public async Task GetDirectoryRoot_SpecificDrive_ShouldReturnRootWithCorrectDrive(
		char drive)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
		string expectedRoot = FileTestHelper.RootDrive(Test, "", drive);
		string path = System.IO.Path.Combine($"{drive}:\\foo", "bar");

		string result = FileSystem.Directory.GetDirectoryRoot(path);

		await That(result).IsEqualTo(expectedRoot);
	}
}
