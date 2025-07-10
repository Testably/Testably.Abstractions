using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public async Task Extension_ShouldReturnEmptyString(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		string result = sut.Extension;

		await That(result).IsEmpty();
	}

	[Theory]
	[InlineData(@"/temp\\folder")]
	[InlineData("/temp/folder")]
	[InlineData(@"/temp/\\/folder")]
	public async Task FullName_ShouldNotNormalizePathOnLinux(string path)
	{
		Skip.If(Test.RunsOnWindows);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.FullName).IsEqualTo(path);
	}

	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public async Task FullName_ShouldReturnFullPath(string path)
	{
		string expectedPath = FileSystem.Path.GetFullPath(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.FullName).IsEqualTo(expectedPath);
	}

	[Theory]
	[InlineData(@"\\unc\folder", @"\\unc\folder")]
	[InlineData(@"\\unc/folder\\foo", @"\\unc\folder\foo")]
	[InlineData(@"c:\temp\\folder", @"c:\temp\folder")]
	[InlineData(@"c:\temp//folder", @"c:\temp\folder")]
	[InlineData(@"c:\temp//\\///folder", @"c:\temp\folder")]
	public async Task FullName_ShouldReturnNormalizedPath_OnWindows(
		string path, string expectedPath)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.FullName).IsEqualTo(expectedPath);
	}

	[Theory]
	[AutoData]
	public async Task FullName_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		path = FileSystem.Path.GetFullPath(path);
		string pathWithSpaces = path + "  ";

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			await That(sut.FullName).IsEqualTo(path);
		}
		else
		{
			await That(sut.FullName).IsEqualTo(pathWithSpaces);
		}
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_Attributes_ShouldAlwaysBeNegativeOne_AndSetterShouldThrowFileNotFoundException(
			FileAttributes fileAttributes)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.Attributes).IsEqualTo((FileAttributes)(-1));

		void Act()
		{
			sut.Attributes = fileAttributes;
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		await That(sut.Attributes).IsEqualTo((FileAttributes)(-1));
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_CreationTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime creationTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.CreationTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());

		void Act()
		{
			sut.CreationTime = creationTime;
		}

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.CreationTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_CreationTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime creationTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.CreationTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());

		void Act()
		{
			sut.CreationTimeUtc = creationTimeUtc;
		}

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.CreationTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_LastAccessTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastAccessTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.LastAccessTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());

		void Act()
		{
			sut.LastAccessTime = lastAccessTime;
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.LastAccessTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_LastAccessTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastAccessTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.LastAccessTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());

		void Act()
		{
			sut.LastAccessTimeUtc = lastAccessTimeUtc;
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.LastAccessTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_LastWriteTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastWriteTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.LastWriteTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());

		void Act()
		{
			sut.LastWriteTime = lastWriteTime;
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.LastWriteTime).IsEqualTo(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public async Task
		MissingFile_LastWriteTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastWriteTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		await That(sut.LastWriteTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());

		void Act()
		{
			sut.LastWriteTimeUtc = lastWriteTimeUtc;
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
		}

		await That(sut.LastWriteTimeUtc).IsEqualTo(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public async Task Name_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		string pathWithSpaces = path + "  ";

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			await That(sut.Name).IsEqualTo(path);
		}
		else
		{
			await That(sut.Name).IsEqualTo(pathWithSpaces);
		}
	}

	[Theory]
	[AutoData]
	public async Task Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
		string path2,
		string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		await That(sut.Parent).IsNotNull();
		await That(sut?.Exists).IsFalse();
		await That(sut?.Parent).IsNotNull();
		await That(sut?.Parent?.Exists).IsFalse();
	}

	[Fact]
	public async Task Parent_Root_ShouldBeNull()
	{
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(FileTestHelper.RootDrive(Test));

		await That(sut.Parent).IsNull();
	}

	[Theory]
	[InlineAutoData("./foo/bar", "foo")]
	[InlineAutoData("./foo", ".")]
	public async Task Parent_ToString_ShouldBeAbsolutePathOnNetCore(
		string path, string expectedParent)
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.Initialize();
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.ToString()).IsEqualTo(path);

		IDirectoryInfo? parent = sut.Parent;

		await That(parent).IsNotNull();
		if (Test.IsNetFramework)
		{
			await That(parent!.ToString()).IsEqualTo(expectedParent);
		}
		else
		{
			await That(parent!.ToString()).IsEqualTo(FileSystem.Path.GetFullPath(expectedParent));
		}
	}

	[Theory]
	[InlineAutoData("./foo/bar", "foo")]
	[InlineAutoData("./foo", "bar", "bar")]
	public async Task Parent_ToString_ShouldBeDirectoryNameOnNetFramework(
		string path, string expectedParent, string directory)
	{
		Skip.IfNot(Test.IsNetFramework);

		FileSystem.InitializeIn(directory);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.ToString()).IsEqualTo(path);

		IDirectoryInfo? parent = sut.Parent;

		await That(parent).IsNotNull();
		if (Test.IsNetFramework)
		{
			await That(parent!.ToString()).IsEqualTo(expectedParent);
		}
		else
		{
			await That(parent!.ToString()).IsEqualTo(FileSystem.Path.GetFullPath(expectedParent));
		}
	}

	[Fact]
	public async Task Root_Name_ShouldBeCorrect()
	{
		string rootName = FileTestHelper.RootDrive(Test);
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(rootName);

		await That(sut.FullName).IsEqualTo(rootName);
		await That(sut.Name).IsEqualTo(rootName);
	}

	[Theory]
	[AutoData]
	public async Task Root_ShouldExist(string path)
	{
		string expectedRoot = FileTestHelper.RootDrive(Test);
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		await That(result.Root.Exists).IsTrue();
		await That(result.Root.FullName).IsEqualTo(expectedRoot);
	}

	[Theory]
	[InlineData("/foo")]
	[InlineData("./foo")]
	[InlineData("foo")]
	public async Task ToString_ShouldReturnProvidedPath(string path)
	{
		IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(path);

		string? result = directoryInfo.ToString();

		await That(result).IsEqualTo(path);
	}
}
