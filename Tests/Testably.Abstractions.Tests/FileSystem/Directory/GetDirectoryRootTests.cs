namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetDirectoryRootTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetDirectoryRoot_Empty_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.GetDirectoryRoot("");
		});

		exception.Should().BeException<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[SkippableFact]
	public void GetDirectoryRoot_ShouldReturnDefaultRoot()
	{
		string expectedRoot = FileTestHelper.RootDrive();

		string result = FileSystem.Directory.GetDirectoryRoot("foo");

		result.Should().Be(expectedRoot);
	}

	[SkippableTheory]
	[InlineData('A')]
	[InlineData('C')]
	[InlineData('X')]
	public void GetDirectoryRoot_SpecificDrive_ShouldReturnRootWithCorrectDrive(
		char drive)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
		string expectedRoot = FileTestHelper.RootDrive("", drive);
		string path = System.IO.Path.Combine($"{drive}:\\foo", "bar");

		string result = FileSystem.Directory.GetDirectoryRoot(path);

		result.Should().Be(expectedRoot);
	}
}