﻿namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class FileVersionInfoFactoryMockTests
{
	[Theory]
	[InlineData(".", "foo", "*", true)]
	[InlineData(".", "foo", "f*o", true)]
	[InlineData(".", "foo", "*fo", false)]
	[InlineData("bar", "foo", "f*o", true)]
	[InlineData("bar", "foo", "baz/f*o", false)]
	[InlineData("bar", "foo", "/f*o", false)]
	[InlineData("bar", "foo", "**/f*o", true)]
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

	[Theory]
	[AutoData]
	public async Task WhenRegistered_ShouldReturnFileVersionInfoWithRegisteredValues(
		string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("abc.foo");
		fileSystem.WithFileVersionInfo("*.foo", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("abc.foo");

		await That(result.Comments).IsEqualTo(comments);
	}

	[Theory]
	[AutoData]
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
