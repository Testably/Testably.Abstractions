using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllLinesTests
{
	[Theory]
	[AutoData]
	public async Task WriteAllLines_Enumerable_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllLines(path, contents.AsEnumerable());

		string[] result = FileSystem.File.ReadAllLines(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_Enumerable_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents.AsEnumerable());

		string[] result = FileSystem.File.ReadAllLines(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_Enumerable_WithEncoding_ShouldCreateFileWithText(
		Encoding encoding, string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents.AsEnumerable(), encoding);

		string[] result = FileSystem.File.ReadAllLines(path, encoding);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_Enumerable_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string[] contents = ["breuß"];

		FileSystem.File.WriteAllLines(path, contents.AsEnumerable());

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6 + Environment.NewLine.Length);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllLines(path, contents);

		string[] result = FileSystem.File.ReadAllLines(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_ShouldCreateFileWithText(string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents);

		string[] result = FileSystem.File.ReadAllLines(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLines_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string[] contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.WriteAllLines(path, contents);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLines_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string[] contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		void Act()
		{
			FileSystem.File.WriteAllLines(path, contents);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_WithEncoding_ShouldCreateFileWithText(
		Encoding encoding, string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents, encoding);

		string[] result = FileSystem.File.ReadAllLines(path, encoding);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLines_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string[] contents = ["breuß"];

		FileSystem.File.WriteAllLines(path, contents);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6 + Environment.NewLine.Length);
	}
}
