using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
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

	[Theory]
	[AutoData]
	public async Task Create_ShouldCreateDirectory(string path)
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsFalse();

		sut.Create();

#if NETFRAMEWORK
		// The DirectoryInfo is not updated in .NET Framework!
		sut.Exists.Should().BeFalse();
#else
		await That(sut.Exists).IsTrue();
#endif
		FileSystem.Directory.Exists(sut.FullName).Should().BeTrue();
	}

	[Fact]
	public async Task Create_ShouldCreateInBasePath()
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");

		result.Create();

		FileSystem.Directory.Exists("foo").Should().BeTrue();
		await That(result.FullName).StartsWith(BasePath);
	}

	[Fact]
	public async Task Create_ShouldCreateParentDirectories()
	{
		string directoryLevel1 = "lvl1";
		string directoryLevel2 = "lvl2";
		string directoryLevel3 = "lvl3";
		string path =
			FileSystem.Path.Combine(directoryLevel1, directoryLevel2, directoryLevel3);

		IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);
		result.Create();

		await That(result.Name).IsEqualTo(directoryLevel3);
		await That(result.Parent!.Name).IsEqualTo(directoryLevel2);
		await That(result.Parent.Parent!.Name).IsEqualTo(directoryLevel1);
		await That(result.Exists).IsTrue();
		await That(result.Parent.Exists).IsTrue();
		await That(result.Parent.Parent.Exists).IsTrue();
		result.ToString().Should().Be(path);
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
	{
		IDirectoryInfo sut1 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut2 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut3 = FileSystem.DirectoryInfo.New(path);
		await That(sut1.Exists).IsFalse();
		await That(sut2.Exists).IsFalse();
		// Do not call Exists for `sut3`

		sut1.Create();

		if (Test.IsNetFramework)
		{
			await That(sut1.Exists).IsFalse();
			await That(sut2.Exists).IsFalse();
			await That(sut3.Exists).IsTrue();
		}
		else
		{
			await That(sut1.Exists).IsTrue();
			await That(sut2.Exists).IsFalse();
			await That(sut3.Exists).IsTrue();
		}

		FileSystem.Directory.Exists(path).Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("/")]
	[InlineData("\\")]
	public async Task Create_TrailingDirectorySeparator_ShouldNotBeTrimmed(
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
		await That(result.Name).IsEqualTo(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		await That(result.FullName).IsEqualTo(FileSystem.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		FileSystem.Directory.Exists(nameWithSuffix).Should().BeTrue();
	}
}
