namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

[FileSystemTests]
public class PathTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task Path_Empty_ShouldNotThrowException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = "";
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task Path_Null_ShouldNotThrowException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = null!;
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[AutoArguments]
	public async Task Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = path;
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithMessageContaining(path);
	}

	[Test]
	public async Task Path_Whitespace_ShouldThrowArgumentException()
	{
		using IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			fileSystemWatcher.Path = "  ";
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(Test.IsNetFramework ? null : "Path");
	}
}
