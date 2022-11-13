using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
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

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldCreateParentDirectories(
		string directoryLevel1, string directoryLevel2, string directoryLevel3)
	{
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