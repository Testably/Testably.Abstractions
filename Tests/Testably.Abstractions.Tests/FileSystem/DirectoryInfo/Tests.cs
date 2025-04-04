using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void Extension_ShouldReturnEmptyString(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		string result = sut.Extension;

		result.Should().BeEmpty();
	}

	[Theory]
	[InlineData(@"/temp\\folder")]
	[InlineData("/temp/folder")]
	[InlineData(@"/temp/\\/folder")]
	public void FullName_ShouldNotNormalizePathOnLinux(string path)
	{
		Skip.If(Test.RunsOnWindows);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.FullName.Should().Be(path);
	}

	[Theory]
	[InlineData("foo")]
	[InlineData("foo/")]
	public void FullName_ShouldReturnFullPath(string path)
	{
		string expectedPath = FileSystem.Path.GetFullPath(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.FullName.Should().Be(expectedPath);
	}

	[Theory]
	[InlineData(@"\\unc\folder", @"\\unc\folder")]
	[InlineData(@"\\unc/folder\\foo", @"\\unc\folder\foo")]
	[InlineData(@"c:\temp\\folder", @"c:\temp\folder")]
	[InlineData(@"c:\temp//folder", @"c:\temp\folder")]
	[InlineData(@"c:\temp//\\///folder", @"c:\temp\folder")]
	public void FullName_ShouldReturnNormalizedPath_OnWindows(
		string path, string expectedPath)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.FullName.Should().Be(expectedPath);
	}

	[Theory]
	[AutoData]
	public void FullName_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		path = FileSystem.Path.GetFullPath(path);
		string pathWithSpaces = path + "  ";

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			sut.FullName.Should().Be(path);
		}
		else
		{
			sut.FullName.Should().Be(pathWithSpaces);
		}
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_Attributes_ShouldAlwaysBeNegativeOne_AndSetterShouldThrowFileNotFoundException(
			FileAttributes fileAttributes)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.Attributes.Should().Be((FileAttributes)(-1));
		Exception? exception = Record.Exception(() =>
		{
			sut.Attributes = fileAttributes;
		});
		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		sut.Attributes.Should().Be((FileAttributes)(-1));
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_CreationTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime creationTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.CreationTime = creationTime;
		});

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.CreationTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_CreationTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime creationTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.CreationTimeUtc = creationTimeUtc;
		});

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.CreationTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_LastAccessTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastAccessTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastAccessTime = lastAccessTime;
		});

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.LastAccessTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_LastAccessTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastAccessTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastAccessTimeUtc = lastAccessTimeUtc;
		});

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.LastAccessTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_LastWriteTime_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastWriteTime)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastWriteTime = lastWriteTime;
		});

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.LastWriteTime.Should().Be(FileTestHelper.NullTime.ToLocalTime());
	}

	[Theory]
	[AutoData]
	public void
		MissingFile_LastWriteTimeUtc_ShouldAlwaysBeNullTime_AndSetterShouldThrowCorrectException(
			DateTime lastWriteTimeUtc)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("Missing File");
		sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
		Exception? exception = Record.Exception(() =>
		{
			sut.LastWriteTimeUtc = lastWriteTimeUtc;
		});

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
		}
		else
		{
			exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
		}

		sut.LastWriteTimeUtc.Should().Be(FileTestHelper.NullTime.ToUniversalTime());
	}

	[Theory]
	[AutoData]
	public void Name_ShouldTrimTrailingSpaces_OnWindows(string path)
	{
		string pathWithSpaces = path + "  ";

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(pathWithSpaces);

		if (Test.RunsOnWindows)
		{
			sut.Name.Should().Be(path);
		}
		else
		{
			sut.Name.Should().Be(pathWithSpaces);
		}
	}

	[Theory]
	[AutoData]
	public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
		string path2,
		string path3)
	{
		string path = FileSystem.Path.Combine(path1, path2, path3);

		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Parent.Should().NotBeNull();
		sut.Parent?.Exists.Should().BeFalse();
		sut.Parent?.Parent.Should().NotBeNull();
		sut.Parent?.Parent?.Exists.Should().BeFalse();
	}

	[Fact]
	public void Parent_Root_ShouldBeNull()
	{
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(FileTestHelper.RootDrive(Test));

		sut.Parent.Should().BeNull();
	}

	[Theory]
	[InlineAutoData("./foo/bar", "foo")]
	[InlineAutoData("./foo", ".")]
	public void Parent_ToString_ShouldBeAbsolutePathOnNetCore(
		string path, string expectedParent)
	{
		Skip.If(Test.IsNetFramework);

		FileSystem.Initialize();
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.ToString().Should().Be(path);

		IDirectoryInfo? parent = sut.Parent;

		parent.Should().NotBeNull();
		if (Test.IsNetFramework)
		{
			parent!.ToString().Should().Be(expectedParent);
		}
		else
		{
			parent!.ToString().Should().Be(FileSystem.Path.GetFullPath(expectedParent));
		}
	}

	[Theory]
	[InlineAutoData("./foo/bar", "foo")]
	[InlineAutoData("./foo", "bar", "bar")]
	public void Parent_ToString_ShouldBeDirectoryNameOnNetFramework(
		string path, string expectedParent, string directory)
	{
		Skip.IfNot(Test.IsNetFramework);

		FileSystem.InitializeIn(directory);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.ToString().Should().Be(path);

		IDirectoryInfo? parent = sut.Parent;

		parent.Should().NotBeNull();
		if (Test.IsNetFramework)
		{
			parent!.ToString().Should().Be(expectedParent);
		}
		else
		{
			parent!.ToString().Should().Be(FileSystem.Path.GetFullPath(expectedParent));
		}
	}

	[Fact]
	public void Root_Name_ShouldBeCorrect()
	{
		string rootName = FileTestHelper.RootDrive(Test);
		IDirectoryInfo sut =
			FileSystem.DirectoryInfo.New(rootName);

		sut.FullName.Should().Be(rootName);
		sut.Name.Should().Be(rootName);
	}

	[Theory]
	[AutoData]
	public void Root_ShouldExist(string path)
	{
		string expectedRoot = FileTestHelper.RootDrive(Test);
		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

		result.Root.Exists.Should().BeTrue();
		result.Root.FullName.Should().Be(expectedRoot);
	}

	[Theory]
	[InlineData("/foo")]
	[InlineData("./foo")]
	[InlineData("foo")]
	public void ToString_ShouldReturnProvidedPath(string path)
	{
		IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(path);

		string? result = directoryInfo.ToString();

		result.Should().Be(path);
	}
}
