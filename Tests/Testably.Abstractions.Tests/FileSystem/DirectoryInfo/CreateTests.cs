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
		FileSystem.Should().NotHaveDirectory(name);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldCreateDirectory(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		sut.Should().NotExist();

		sut.Create();

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		sut.Should().NotExist();
#else
		sut.Should().Exist();
#endif
		FileSystem.Should().HaveDirectory(sut.FullName);
	}

	[SkippableFact]
	public void Create_ShouldCreateInBasePath()
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");

		result.Create();

		FileSystem.Should().HaveDirectory("foo");
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
		result.Should().Exist();
		result.Parent.Should().Exist();
		result.Parent.Parent.Should().Exist();
		result.ToString().Should().Be(path);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
	{
		IDirectoryInfo sut1 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut2 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut3 = FileSystem.DirectoryInfo.New(path);
		sut1.Should().NotExist();
		sut2.Should().NotExist();
		// Do not call Exists for `sut3`

		sut1.Create();

		if (Test.IsNetFramework)
		{
			sut1.Should().NotExist();
			sut2.Should().NotExist();
			sut3.Should().Exist();
		}
		else
		{
			sut1.Should().Exist();
			sut2.Should().NotExist();
			sut3.Should().Exist();
		}

		FileSystem.Should().HaveDirectory(path);
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
			Skip.If(string.Equals(suffix, "\\", StringComparison.Ordinal) ||
			        string.Equals(suffix, " ", StringComparison.Ordinal),
				$"The case with '{suffix}' as suffix is only supported on Windows.");
		}

		IDirectoryInfo result =
			FileSystem.DirectoryInfo.New(nameWithSuffix);
		result.Create();

		result.ToString().Should().Be(nameWithSuffix);
		result.Name.Should().Be(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		result.FullName.Should().Be(FileSystem.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Should().HaveDirectory(nameWithSuffix);
	}
}
