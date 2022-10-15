namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetDirectoryRoot))]
	public void GetDirectoryRoot_Empty_ShouldThrowArgumentException()
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.GetDirectoryRoot("");
		});

		exception.Should().BeOfType<ArgumentException>();
	}

	[SkippableFact]
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetDirectoryRoot))]
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
	[FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetDirectoryRoot))]
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