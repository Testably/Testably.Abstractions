namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class FileVersionInfoFactoryMockTests
{
	[Test]
	[Arguments(".", "foo", "*", true)]
	[Arguments(".", "foo", "f*o", true)]
	[Arguments(".", "foo", "*fo", false)]
	[Arguments("bar", "foo", "f*o", true)]
	[Arguments("bar", "foo", "baz/f*o", false)]
	[Arguments("bar", "foo", "/f*o", false)]
	[Arguments("bar", "foo", "**/f*o", true)]
	public async Task ShouldSupportGlobPattern(
		string baseDirectory, string fileName, string globPattern, bool expectMatch)
	{
		MockFileSystem fileSystem = new();
		string filePath = fileSystem.Path.Combine(baseDirectory, fileName);
		fileSystem.Directory.CreateDirectory(baseDirectory);
		fileSystem.File.WriteAllText(filePath, "");
		fileSystem.WithFileVersionInfo(globPattern, b => b.SetComments("isMatch"));
		string? expected = expectMatch ? "isMatch" : null;

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo(filePath);

		await That(result.Comments).IsEqualTo(expected);
	}

	[Test]
	[AutoArguments]
	public async Task WhenRegistered_ShouldReturnFileVersionInfoWithRegisteredValues(
		string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("abc.foo");
		fileSystem.WithFileVersionInfo("*.foo", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("abc.foo");

		await That(result.Comments).IsEqualTo(comments);
	}

	[Test]
	[AutoArguments]
	public async Task WhenRegisteredUnderDifferentName_ShouldReturnDefaultValues(
		string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("abc.bar");
		fileSystem.WithFileVersionInfo("*.foo", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("abc.bar");

		await That(result.Comments).IsNull();
	}
}
