using System.Linq;
using System.Runtime.InteropServices;
#if !NETFRAMEWORK
using System.IO;
#endif

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Create_IllegalCharacters_ShouldThrowArgumentException()
	{
		foreach (char c in FileSystem.Path.GetInvalidPathChars().Where(c => c != '\0'))
		{
			string path = "foo" + c + "bar";
			Exception? exception =
				Record.Exception(() => FileSystem.DirectoryInfo.New(path).Create());

#if NETFRAMEWORK
			exception.Should().BeOfType<ArgumentException>()
			   .Which.Message.Should().Be("Illegal characters in path.");
#else
			string expectedMessage = $"'{FileSystem.Path.Combine(BasePath, path)}'";
			exception.Should().BeOfType<IOException>()
			   .Which.Message.Should().Contain(expectedMessage);
#endif
		}
	}

	[SkippableFact]
	public void Create_Null_ShouldThrowArgumentNullException()
	{
		Exception? exception =
			Record.Exception(() => FileSystem.DirectoryInfo.New(null!));

		exception.Should().BeOfType<ArgumentNullException>().Which.ParamName
		   .Should().Be("path");
	}

	[SkippableTheory]
	[InlineData("\0foo")]
	[InlineData("foo\0bar")]
	public void Create_NullCharacter_ShouldThrowArgumentException(string path)
	{
		string expectedMessage = "Illegal characters in path.";
		Exception? exception =
			Record.Exception(() => FileSystem.DirectoryInfo.New(path).Create());

		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain(expectedMessage);
	}

	[SkippableTheory]
	[AutoData]
	public void Create_ShouldCreateDirectory(string path)
	{
		IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
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
		IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");
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

		IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);
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
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			expectedName = expectedName.TrimEnd(' ');
		}
		else
		{
			Skip.If(suffix == "\\" || suffix == " ",
				$"The case with '{suffix}' as suffix is only supported on Windows.");
		}

		IFileSystem.IDirectoryInfo result =
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