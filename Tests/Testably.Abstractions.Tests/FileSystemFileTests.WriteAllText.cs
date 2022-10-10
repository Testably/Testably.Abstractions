namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllText))]
	public void WriteAllText_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllText))]
	public void WriteAllText_ShouldCreateFileWithText(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllText))]
	public void WriteAllText_SpecialCharacters_ShouldReturnSameText(string path)
	{
		char[] specialCharacters = { 'Ä', 'Ö', 'Ü', 'ä', 'ö', 'ü', 'ß' };
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			FileSystem.File.WriteAllText(path, contents);

			string result = FileSystem.File.ReadAllText(path);

			result.Should().Be(contents,
				$"{contents} should be encoded and decoded identical.");
		}
	}
}