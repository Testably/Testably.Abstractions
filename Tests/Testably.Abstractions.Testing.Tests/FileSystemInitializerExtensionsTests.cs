using aweXpect.Testably;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;
using Assembly = System.Reflection.Assembly;

namespace Testably.Abstractions.Testing.Tests;

[NotInParallel(nameof(RealFileSystem))]
public class FileSystemInitializerExtensionsTests
{
	[Test]
	public async Task Initialize_WithAFile_ShouldCreateFile()
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile();

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".")).HasSingle();
	}

	[Test]
	[Arguments("txt")]
	public async Task Initialize_WithAFile_WithExtension_ShouldCreateFileWithExtension(
		string extension)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithAFile(extension);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".", $"*.{extension}")).HasSingle();
	}

	[Test]
	public async Task Initialize_WithASubdirectory_ShouldCreateDirectory()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory").WithASubdirectory();

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateDirectories(".")).HasSingle();
	}

	[Test]
	[Arguments("foo.txt")]
	public async Task Initialize_WithFile_Existing_ShouldThrowTestingException(string fileName)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(fileName, null);

		void Act()
		{
			sut.Initialize().WithFile(fileName);
		}

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{fileName}*").AsWildcard();
	}

	[Test]
	[Arguments("foo.txt", new byte[]{ 0x1, 0x2, 0x3, })]
	public async Task Initialize_WithFile_HasBytesContent_ShouldCreateFileWithGivenFileContent(
		string fileName, byte[] fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasBytesContent(fileContent));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		byte[] result = sut.File.ReadAllBytes(fileName);

		await That(result).IsEqualTo(fileContent).InAnyOrder();
	}

	[Test]
	[Arguments("foo.txt", "file-content")]
	public async Task Initialize_WithFile_HasStringContent_ShouldCreateFileWithGivenFileContent(
		string fileName, string fileContent)
	{
		MockFileSystem sut = new();
		sut.Initialize()
			.WithFile(fileName).Which(f => f
				.HasStringContent(fileContent));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		string result = sut.File.ReadAllText(fileName);

		await That(result).IsEqualTo(fileContent);
	}

	[Test]
	[Arguments("foo.txt")]
	public async Task Initialize_WithFile_ShouldCreateFileWithGivenFileName(string fileName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithFile(fileName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateFiles(".", fileName)).HasSingle();
	}

	[Test]
	public async Task Initialize_WithNestedSubdirectories_ShouldCreateAllNestedDirectories()
	{
		MockFileSystem sut = new();
		sut.InitializeIn("base-directory")
			.WithSubdirectory("foo").Initialized(d => d
				.WithSubdirectory("bar").Initialized(s => s
					.WithSubdirectory("xyz")));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		List<string> result = sut.Directory
			.EnumerateDirectories(".", "*", SearchOption.AllDirectories).ToList();

		await That(result.Count).IsEqualTo(3);
		await That(result).Contains(sut.Path.Combine(".", "foo"));
		await That(result).Contains(sut.Path.Combine(".", "foo", "bar"));
		await That(result).Contains(sut.Path.Combine(".", "foo", "bar", "xyz"));
	}

	[Test]
	[Arguments(false)]
	[Arguments(true)]
	public async Task Initialize_WithOptions_ShouldConsiderValueOfInitializeTempDirectory(
		bool initializeTempDirectory)
	{
		MockFileSystem sut = new();

		sut.Initialize(options => options.InitializeTempDirectory = initializeTempDirectory);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.Exists(sut.Path.GetTempPath())).IsEqualTo(initializeTempDirectory);
	}

	[Test]
	[Arguments("my-directory")]
	public async Task Initialize_WithSubdirectory_Existing_ShouldThrowTestingException(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Directory.CreateDirectory(directoryName);

		void Act()
			=> sut.Initialize().WithSubdirectory(directoryName);

		await That(Act).ThrowsExactly<TestingException>().WithMessage($"*{directoryName}*")
			.AsWildcard();
	}

	[Test]
	[Arguments("my-directory")]
	public async Task Initialize_WithSubdirectory_ShouldCreateDirectoryWithGivenDirectoryName(
		string directoryName)
	{
		MockFileSystem sut = new();
		sut.Initialize().WithSubdirectory(directoryName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.EnumerateDirectories(".", directoryName)).HasSingle();
	}

	[Test]
	[Arguments("my-directory")]
	public async Task Initialize_WithSubdirectory_ShouldExist(string directoryName)
	{
		MockFileSystem sut = new();
		IFileSystemDirectoryInitializer<
			MockFileSystem> result =
			sut.Initialize().WithSubdirectory(directoryName);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(result.Directory.Exists).IsTrue();
	}

	[Test]
	[Arguments("my-path")]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_ShouldCopyAllMatchingResourceFilesInDirectory(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			searchPattern: "*.txt");

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources"));
		string[] result2 =
			fileSystem.Directory.GetFiles(Path.Combine(path, "TestResources", "SubResource"));
		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(x => x.EndsWith("TestFile1.txt", StringComparison.Ordinal));
		await That(result).Contains(x => x.EndsWith("TestFile2.txt", StringComparison.Ordinal));
		await That(result2.Length).IsEqualTo(1);
		await That(result2)
			.Contains(x => x.EndsWith("SubResourceFile1.txt", StringComparison.Ordinal));
	}

	[Test]
	[Arguments("my-path")]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_WithoutRecurseSubdirectories_ShouldOnlyCopyTopmostFilesInRelativePath(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			"TestResources",
			searchPattern: "*.txt",
			SearchOption.TopDirectoryOnly);

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(path);
		await That(result.Length).IsEqualTo(2);
		await That(result).Contains(x => x.EndsWith("TestFile1.txt", StringComparison.Ordinal));
		await That(result).Contains(x => x.EndsWith("TestFile2.txt", StringComparison.Ordinal));
		await That(fileSystem.Directory.Exists(Path.Combine(path, "SubResource"))).IsFalse();
	}

	[Test]
	[Arguments("my-path")]
	public async Task
		InitializeEmbeddedResourcesFromAssembly_WithRelativePath_ShouldCopyAllResourceInMatchingPathInDirectory(
			string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("foo");

		fileSystem.InitializeEmbeddedResourcesFromAssembly(
			path,
			Assembly.GetExecutingAssembly(),
			"TestResources/SubResource",
			searchPattern: "*.txt");

		await That(fileSystem.Statistics.TotalCount).IsEqualTo(0);
		string[] result = fileSystem.Directory.GetFiles(path);
		await That(result.Length).IsEqualTo(1);
		await That(result)
			.Contains(x => x.EndsWith("SubResourceFile1.txt", StringComparison.Ordinal));
	}

	[Test]
	[Arguments("my-directory")]
	public async Task InitializeFromRealDirectory_MissingDrive_ShouldCreateDrive(
		string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);
		SkipIfRealFileSystemShouldBeSkipped();

		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			MockFileSystem sut = new();
			IDriveInfo[] drives = sut.DriveInfo.GetDrives();
			for (char c = 'D'; c <= 'Z'; c++)
			{
				if (drives.Any(d => d.Name.StartsWith($"{c}", StringComparison.Ordinal)))
				{
					continue;
				}

				directoryName = Path.Combine($"{c}:\\", directoryName);
				break;
			}

			sut.InitializeFromRealDirectory(tempPath, directoryName);

			await That(sut.Directory.Exists(directoryName)).IsTrue();
			await That(sut.DriveInfo.GetDrives()).HasCount(drives.Length + 1);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyFileToTargetDirectory()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "Hello, World!");

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem).HasDirectory("foo");
			await That(fileSystem).HasFile("foo/test.txt").WithContent("Hello, World!");
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task
		InitializeFromRealDirectory_ShouldRecursivelyCopyDirectoriesToTargetDirectory()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			Directory.CreateDirectory(Path.Combine(tempPath, "subdir"));
			File.WriteAllText(Path.Combine(tempPath, "subdir", "test1.txt"), "foo");
			File.WriteAllText(Path.Combine(tempPath, "subdir", "test2.txt"), "bar");

			fileSystem.InitializeFromRealDirectory(tempPath);

			await That(fileSystem).HasDirectory(fileSystem.Path.Combine(tempPath, "subdir"));
			await That(fileSystem).HasFile(fileSystem.Path.Combine(tempPath, "subdir", "test1.txt"))
				.WithContent("foo");
			await That(fileSystem).HasFile(fileSystem.Path.Combine(tempPath, "subdir", "test2.txt"))
				.WithContent("bar");
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task
		InitializeFromRealDirectory_WhenDirectoryDoesNotExist_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		while (Directory.Exists(tempPath))
		{
			tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		void Act()
			=> fileSystem.InitializeFromRealDirectory(tempPath, "foo");

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessage($"The directory '{tempPath}' does not exist.");
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyFileAttributes()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "content");
			File.SetAttributes(filePath, FileAttributes.ReadOnly);
			FileAttributes expectedAttributes = File.GetAttributes(filePath);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.File.GetAttributes(
				fileSystem.Path.Combine("foo", "test.txt"))).IsEqualTo(expectedAttributes);
		}
		finally
		{
			foreach (string file in Directory.EnumerateFiles(tempPath, "*",
				         SearchOption.AllDirectories))
			{
				File.SetAttributes(file, FileAttributes.Normal);
			}

			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyFileCreationTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "content");
			DateTime expectedCreationTime =
				new DateTime(2020, 1, 15, 10, 30, 0, DateTimeKind.Utc);
			File.SetCreationTimeUtc(filePath, expectedCreationTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.File.GetCreationTimeUtc(
				fileSystem.Path.Combine("foo", "test.txt"))).IsEqualTo(expectedCreationTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyFileLastAccessTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "content");
			DateTime expectedLastAccessTime =
				new DateTime(2021, 6, 10, 8, 0, 0, DateTimeKind.Utc);
			File.SetLastAccessTimeUtc(filePath, expectedLastAccessTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.File.GetLastAccessTimeUtc(
				fileSystem.Path.Combine("foo", "test.txt"))).IsEqualTo(expectedLastAccessTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyFileLastWriteTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string filePath = Path.Combine(tempPath, "test.txt");
			File.WriteAllText(filePath, "content");
			DateTime expectedLastWriteTime =
				new DateTime(2022, 3, 25, 12, 0, 0, DateTimeKind.Utc);
			File.SetLastWriteTimeUtc(filePath, expectedLastWriteTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.File.GetLastWriteTimeUtc(
				fileSystem.Path.Combine("foo", "test.txt"))).IsEqualTo(expectedLastWriteTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyDirectoryCreationTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string subDir = Path.Combine(tempPath, "subdir");
			Directory.CreateDirectory(subDir);
			DateTime expectedCreationTime =
				new DateTime(2020, 1, 15, 10, 30, 0, DateTimeKind.Utc);
			Directory.SetCreationTimeUtc(subDir, expectedCreationTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.Directory.GetCreationTimeUtc(
				fileSystem.Path.Combine("foo", "subdir"))).IsEqualTo(expectedCreationTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyDirectoryLastWriteTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string subDir = Path.Combine(tempPath, "subdir");
			Directory.CreateDirectory(subDir);
			DateTime expectedLastWriteTime =
				new DateTime(2022, 3, 25, 12, 0, 0, DateTimeKind.Utc);
			Directory.SetLastWriteTimeUtc(subDir, expectedLastWriteTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.Directory.GetLastWriteTimeUtc(
				fileSystem.Path.Combine("foo", "subdir"))).IsEqualTo(expectedLastWriteTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	public async Task InitializeFromRealDirectory_ShouldCopyDirectoryLastAccessTime()
	{
		SkipIfRealFileSystemShouldBeSkipped();

		MockFileSystem fileSystem = new();
		string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		try
		{
			Directory.CreateDirectory(tempPath);
			string subDir = Path.Combine(tempPath, "subdir");
			Directory.CreateDirectory(subDir);
			DateTime expectedLastAccessTime =
				new DateTime(2021, 6, 10, 8, 0, 0, DateTimeKind.Utc);
			Directory.SetLastAccessTimeUtc(subDir, expectedLastAccessTime);

			fileSystem.InitializeFromRealDirectory(tempPath, "foo");

			await That(fileSystem.Directory.GetLastAccessTimeUtc(
				fileSystem.Path.Combine("foo", "subdir"))).IsEqualTo(expectedLastAccessTime);
		}
		finally
		{
			Directory.Delete(tempPath, true);
		}
	}

	[Test]
	[Arguments("my-directory")]
	public async Task InitializeIn_MissingDrive_ShouldCreateDrive(string directoryName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		for (char c = 'D'; c <= 'Z'; c++)
		{
			if (drives.Any(d => d.Name.StartsWith($"{c}", StringComparison.Ordinal)))
			{
				continue;
			}

			directoryName = Path.Combine($"{c}:\\", directoryName);
			break;
		}

		sut.InitializeIn(directoryName);

		await That(sut.Directory.Exists(directoryName)).IsTrue();
		await That(sut.DriveInfo.GetDrives()).HasCount(drives.Length + 1);
	}

	[Test]
	[Arguments("my-path")]
	public async Task InitializeIn_ShouldSetCurrentDirectory(string path)
	{
		MockFileSystem sut = new();
		string expectedPath = sut.Execute.Path.GetFullPath(path);

		sut.InitializeIn(path);

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		await That(sut.Directory.GetCurrentDirectory()).IsEqualTo(expectedPath);
	}

	#region Helpers

	private static void SkipIfRealFileSystemShouldBeSkipped()
	{
#if DEBUG
		Skip.If(Settings.RealFileSystemTests == Settings.TestSettingStatus.AlwaysDisabled,
			$"Tests against the real file system are {Settings.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
#else
		Skip.If(Settings.RealFileSystemTests != Settings.TestSettingStatus.AlwaysEnabled,
			$"Tests against the real file system are {Settings.RealFileSystemTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.RealFileSystemTests.");
#endif
	}

	#endregion
}
