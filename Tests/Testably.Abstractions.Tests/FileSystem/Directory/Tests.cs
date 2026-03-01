using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Test]
	public async Task CreateTempSubdirectory_ShouldCreateTheTemporaryDirectory()
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory();

		await That(result.Exists).IsTrue();
	}
#endif

#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
	[Test]
	[AutoArguments]
	public async Task CreateTempSubdirectory_WithPrefix_ShouldStartWithPrefix(string prefix)
	{
		IDirectoryInfo result = FileSystem.Directory.CreateTempSubdirectory(prefix);

		await That(result.Name).StartsWith(prefix);
	}
#endif
	[Test]
	public async Task GetCurrentDirectory_ShouldNotBeRooted()
	{
		string result = FileSystem.Directory.GetCurrentDirectory();

		await That(result).IsNotEqualTo(FileTestHelper.RootDrive(Test));
	}

	[Test]
	[AutoArguments]
	public async Task GetDirectoryRoot_ShouldReturnRoot(string path)
	{
		string root = FileTestHelper.RootDrive(Test);
		string rootedPath = root + path;

		string result = FileSystem.Directory.GetDirectoryRoot(rootedPath);

		await That(result).IsEqualTo(root);
	}

	[Test]
	public async Task GetLogicalDrives_ShouldNotBeEmpty()
	{
		string[] result = FileSystem.Directory.GetLogicalDrives();

		await That(result).IsNotEmpty();
		await That(result).Contains(FileTestHelper.RootDrive(Test));
	}

	[Test]
	[AutoArguments]
	public async Task GetParent_ArbitraryPaths_ShouldNotBeNull(string path1,
		string path2,
		string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);
		IDirectoryInfo expectedParent = FileSystem.DirectoryInfo.New(
			FileSystem.Path.Combine(path1, path2));

		IDirectoryInfo? result = FileSystem.Directory.GetParent(path);

		await That(result).IsNotNull();
		await That(result!.FullName).IsEqualTo(expectedParent.FullName);
	}

	[Test]
	public async Task GetParent_Root_ShouldReturnNull()
	{
		string path = FileTestHelper.RootDrive(Test);

		IDirectoryInfo? result = FileSystem.Directory.GetParent(path);

		await That(result).IsNull();
	}

	[Test]
	[AutoArguments]
	public async Task
		SetCurrentDirectory_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string path)
	{
		string previousCurrentDirectory = FileSystem.Directory.GetCurrentDirectory();
		try
		{
			void Act()
			{
				FileSystem.Directory.SetCurrentDirectory(path);
			}

			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
			await That(FileSystem.Directory.GetCurrentDirectory())
				.IsEqualTo(previousCurrentDirectory);
		}
		finally
		{
			FileSystem.Directory.SetCurrentDirectory(previousCurrentDirectory);
		}
	}

	[Test]
	[AutoArguments]
	public async Task SetCurrentDirectory_RelativePath_ShouldBeFullyQualified(string path)
	{
		string previousCurrentDirectory = FileSystem.Directory.GetCurrentDirectory();
		try
		{
			string expectedPath = FileSystem.Path.GetFullPath(path);
			FileSystem.Directory.CreateDirectory(path);
			FileSystem.Directory.SetCurrentDirectory(path);
			string result = FileSystem.Directory.GetCurrentDirectory();

			await That(result).IsEqualTo(expectedPath);
		}
		finally
		{
			FileSystem.Directory.SetCurrentDirectory(previousCurrentDirectory);
		}
	}
}
