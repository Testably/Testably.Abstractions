namespace Testably.Abstractions.Tests.FileSystem.Path;

public abstract partial class FileSystemPathTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.ChangeExtension))]
	public void ChangeExtension_EmptyPath_ShouldReturnEmptyString(string extension)
	{
		string result = FileSystem.Path.ChangeExtension(string.Empty, extension);

		result.Should().BeEmpty();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.ChangeExtension))]
	public void ChangeExtension_NullPath_ShouldReturnNull(string extension)
	{
		string? result = FileSystem.Path.ChangeExtension(null, extension);

		result.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.ChangeExtension))]
	public void ChangeExtension_WithDirectory_ShouldIncludeDirectory(
		string directory, string fileName, string extension)
	{
		string path = FileSystem.Path.Combine(directory, fileName + ".foo");
		string expectedResult =
			FileSystem.Path.Combine(directory, fileName + "." + extension);

		string result = FileSystem.Path.ChangeExtension(path, extension);

		result.Should().Be(expectedResult);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Path(nameof(IFileSystem.IPath.ChangeExtension))]
	public void ChangeExtension_WithLeadingDotInExtension_ShouldNotIncludeTwoDots(
		string fileName, string extension)
	{
		string path = fileName + ".foo";
		string expectedResult = fileName + "." + extension;

		string result = FileSystem.Path.ChangeExtension(path, "." + extension);

		result.Should().Be(expectedResult);
	}
}