namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class ChangeExtensionTests
{
	[Theory]
	[AutoData]
	public async Task ChangeExtension_EmptyPath_ShouldReturnEmptyString(string extension)
	{
		string result = FileSystem.Path.ChangeExtension(string.Empty, extension);

		await That(result).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_NullPath_ShouldReturnNull(string extension)
	{
		string? result = FileSystem.Path.ChangeExtension(null, extension);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_WhenExtensionIsNull_ShouldRemovePreviousExtension(
		string fileName)
	{
		string path = fileName + "..foo";
		string expectedResult = fileName + ".";

		string result = FileSystem.Path.ChangeExtension(path, null);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_WithDirectory_ShouldIncludeDirectory(
		string directory, string fileName, string extension)
	{
		string path = FileSystem.Path.Combine(directory, fileName + ".foo");
		string expectedResult =
			FileSystem.Path.Combine(directory, fileName + "." + extension);

		string result = FileSystem.Path.ChangeExtension(path, extension);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_WithDotOnlyInDirectory_ShouldAppendExtensionToPath(
		string directory, string fileName, string extension)
	{
		directory = directory + "." + "with-dot";
		string path = $"{directory}{FileSystem.Path.DirectorySeparatorChar}{fileName}";
		string expectedResult = path + "." + extension;

		string result = FileSystem.Path.ChangeExtension(path, extension);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_WithFileStartingWithDot_ShouldAppendExtensionToPath(
		string fileName, string extension)
	{
		string path = $".{fileName}";
		string expectedResult = $".{extension}";

		string result = FileSystem.Path.ChangeExtension(path, extension);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task ChangeExtension_WithLeadingDotInExtension_ShouldNotIncludeTwoDots(
		string fileName, string extension)
	{
		string path = fileName + ".foo";
		string expectedResult = fileName + "." + extension;

		string result = FileSystem.Path.ChangeExtension(path, "." + extension);

		await That(result).IsEqualTo(expectedResult);
	}
}
