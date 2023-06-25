using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Create_FileWithSameNameAlreadyExists_ShouldThrowIOException(string name)
	{
		FileSystem.File.WriteAllText(name, "");
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(name);

		Exception? exception = Record.Exception(() =>
		{
			sut.Create();
		});

		exception.Should().BeException<IOException>(
			hResult: Test.RunsOnWindows ? -2147024713 : 17);
		FileSystem.Directory.Exists(name).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldCreateDirectory(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Exists.Should().BeFalse();

		sut.Create();

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		sut.Exists.Should().BeFalse();
#else
		sut.Exists.Should().BeTrue();
#endif
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
	}

	[SkippableFact]
	public void Create_ShouldCreateInBasePath()
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");
		result.Create();
		bool exists = FileSystem.Directory.Exists("foo");

		exists.Should().BeTrue();
		result.FullName.Should().StartWith(BasePath);
	}

	[SkippableFact]
	public void Create_ShouldCreateParentDirectories()
	{
		string directoryLevel1 = "lvl1";
		string directoryLevel2 = "lvl2";
		string directoryLevel3 = "lvl3";
		string path =
			FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);
		result.Create();

		result.Name.Should().Be(directoryLevel3);
		result.Parent!.Name.Should().Be(directoryLevel2);
		result.Parent.Parent!.Name.Should().Be(directoryLevel1);
		result.Exists.Should().BeTrue();
		result.Parent.Exists.Should().BeTrue();
		result.Parent.Parent.Exists.Should().BeTrue();
		result.ToString().Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
	{
		IDirectoryInfo sut1 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut2 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut3 = FileSystem.DirectoryInfo.New(path);
		sut1.Exists.Should().BeFalse();
		sut2.Exists.Should().BeFalse();
		// Do not call Exists for `sut3`

		sut1.Create();

		if (Test.IsNetFramework)
		{
			sut1.Exists.Should().BeFalse();
			sut2.Exists.Should().BeFalse();
			sut3.Exists.Should().BeTrue();
		}
		else
		{
			sut1.Exists.Should().BeTrue();
			sut2.Exists.Should().BeFalse();
			sut3.Exists.Should().BeTrue();
		}

		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
	[InlineData("")]
	[InlineData("/")]
	[InlineData("\\")]
	public void Create_TrailingDirectorySeparator_ShouldNotBeTrimmed(
		string suffix)
	{
		string nameWithSuffix = "foobar" + suffix;
		string expectedName = nameWithSuffix;
		if (Test.RunsOnWindows)
		{
			expectedName = expectedName.TrimEnd(' ');
		}
		else
		{
			Skip.If(suffix == "\\" || suffix == " ",
				$"The case with '{suffix}' as suffix is only supported on Windows.");
		}

		IDirectoryInfo result =
			FileSystem.DirectoryInfo.New(nameWithSuffix);
		result.Create();

		result.ToString().Should().Be(nameWithSuffix);
		result.Name.Should().Be(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		result.FullName.Should().Be(System.IO.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
}
