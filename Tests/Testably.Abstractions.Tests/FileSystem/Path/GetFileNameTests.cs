namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetFileNameTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task GetFileName_EmptyString_ShouldReturnEmptyString()
	{
		string result = FileSystem.Path.GetFileName(string.Empty);

		await That(result).IsEqualTo(string.Empty);
	}

	[Test]
	public async Task GetFileName_Null_ShouldReturnNull()
	{
		string? result = FileSystem.Path.GetFileName(null);

		await That(result).IsNull();
	}

	[Test]
	[AutoArguments]
	public async Task GetFileName_ShouldReturnFilename(string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
		              "." + extension;

		string result = FileSystem.Path.GetFileName(path);

		await That(result).IsEqualTo(filename + "." + extension);
	}

#if FEATURE_SPAN
	[Test]
	[AutoArguments]
	public async Task GetFileName_Span_ShouldReturnDirectory(
		string directory, string filename,
		string extension)
	{
		string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
					  "." + extension;

		ReadOnlySpan<char> result = FileSystem.Path.GetFileName(path.AsSpan());

		await That(result.ToString()).IsEqualTo(filename + "." + extension);
	}
#endif

	[Test]
	[Arguments("foo/", "", TestOS.All)]
	[Arguments("bar\\", "", TestOS.Windows)]
	[Arguments("/foo", "foo", TestOS.All)]
	[Arguments("\\bar", "bar", TestOS.Windows)]
	public async Task GetFileName_SpecialCases_ShouldReturnExpectedResult(
		string? path, string? expected, TestOS operatingSystem)
	{
		Skip.IfNot(Test.RunsOn(operatingSystem));

		string? result = FileSystem.Path.GetFileName(path);

		await That(result).IsEqualTo(expected);
	}

	[Test]
	[AutoArguments]
	public async Task GetFileName_WithoutDirectory_ShouldReturnFilename(string filename)
	{
		string result = FileSystem.Path.GetFileName(filename);

		await That(result).IsEqualTo(filename);
	}
}
