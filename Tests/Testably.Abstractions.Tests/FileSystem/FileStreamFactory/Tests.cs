using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task New_AppendAccessWithReadWriteMode_ShouldThrowArgumentException(
		string path)
	{
		void Act()
		{
			FileSystem.FileStream.New(path, FileMode.Append, FileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessageContaining(nameof(FileMode.Append)).And
			.WithHResult(-2147024809).And
			.WithParamName(Test.IsNetFramework ? null : "access");
	}

	[Test]
	[AutoArguments]
	public async Task New_ExistingFileWithCreateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);
		stream.Dispose();

		await That(FileSystem.File.ReadAllText(path)).IsEmpty();
	}

	[Test]
	[AutoArguments]
	public async Task New_ExistingFileWithCreateNewMode_ShouldThrowIOException(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		}

		await That(Act).Throws<IOException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024816 : 17);
	}

	[Test]
	[AutoArguments]
	public async Task New_ExistingFileWithTruncateMode_ShouldIgnoreContent(
		string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Truncate);
		stream.Dispose();

		await That(FileSystem.File.ReadAllText(path)).IsEmpty();
	}

	[Test]
	[AutoArguments(FileMode.Append)]
	[AutoArguments(FileMode.Truncate)]
	[AutoArguments(FileMode.Create)]
	[AutoArguments(FileMode.CreateNew)]
	[AutoArguments(FileMode.Append)]
	public async Task New_InvalidModeForReadAccess_ShouldThrowArgumentException(
		FileMode mode, string path)
	{
		FileAccess access = FileAccess.Read;

		void Act()
		{
			FileSystem.FileStream.New(path, mode, access);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(Test.IsNetFramework ? null : "access").And
			.Which.For(x => x.Message,
				it => it.Contains(mode.ToString()).And.Contains(access.ToString()));
	}

	[Test]
	[AutoArguments(FileMode.Open)]
	[AutoArguments(FileMode.Truncate)]
	public async Task New_MissingFileWithIncorrectMode_ShouldThrowFileNotFoundException(
		FileMode mode, string path)
	{
		void Act()
		{
			FileSystem.FileStream.New(path, mode);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments]
	public async Task New_MissingFileWithTruncateMode_ShouldThrowFileNotFoundException(
		string path)
	{
		void Act()
		{
			FileSystem.FileStream.New(path, FileMode.Truncate);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments(FileAccess.Read)]
	[AutoArguments(FileAccess.ReadWrite)]
	[AutoArguments(FileAccess.Write)]
	public async Task
		New_ReadOnlyFlag_ShouldThrowUnauthorizedAccessException_WhenAccessContainsWrite(
			FileAccess access,
			string path)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

		void Act()
		{
			FileSystem.FileStream.New(path, FileMode.Open, access);
		}

		if (access.HasFlag(FileAccess.Write))
		{
			await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Test]
	[AutoArguments]
	public async Task New_SamePathAsExistingDirectory_ShouldThrowCorrectException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.FileStream.New(path, FileMode.CreateNew);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<UnauthorizedAccessException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024891);
		}
		else
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(17);
		}
	}

	[Test]
	[AutoArguments(false)]
	[AutoArguments(true)]
	public async Task New_WithUseAsyncSet_ShouldSetProperty(bool useAsync, string path)
	{
		using FileSystemStream stream = FileSystem.FileStream.New(
			path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1,
			useAsync);

		await That(stream.IsAsync).IsEqualTo(useAsync);
	}
}
