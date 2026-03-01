namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetFileNameWithoutExtensionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task
		GetFileNameWithoutExtension_MultipleDots_ShouldReturnOnlyRemoveTheLastExtension(
			string directory, string filename1, string filename2, string extension)
	{
		string filename = filename1 + "." + filename2;
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}

	[Test]
	public async Task GetFileNameWithoutExtension_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

		await That(result).IsNull();
	}

	[Test]
	[AutoArguments]
	public async Task GetFileNameWithoutExtension_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}

#if FEATURE_SPAN
	[Test]
	[AutoArguments]
	public async Task GetFileNameWithoutExtension_Span_ShouldReturnFileNameWithoutExtension(
		string directory, string filename, string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		ReadOnlySpan<char> result =
			FileSystem.Path.GetFileNameWithoutExtension(path.AsSpan());

		await That(result.ToString()).IsEqualTo(filename);
	}
#endif

	[Test]
	[AutoArguments]
	public async Task GetFileNameWithoutExtension_StartingDot_ShouldReturnEmptyString(
		string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + "." + filename;

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo("");
	}

	[Test]
	[AutoArguments]
	public async Task
		GetFileNameWithoutExtension_TrailingDot_ShouldReturnFilenameWithoutTrailingDot(
			string directory, string filename)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename + ".";

		string result = FileSystem.Path.GetFileNameWithoutExtension(path);

		await That(result).IsEqualTo(filename);
	}
}
