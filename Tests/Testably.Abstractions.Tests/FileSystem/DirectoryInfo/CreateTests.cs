using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
	[AutoData]
	public async Task Create_FileWithSameNameAlreadyExists_ShouldThrowIOException(string name)
	{
		FileSystem.File.WriteAllText(name, "");
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(name);

		void Act()
		{
			sut.Create();
		}

		await That(Act).Throws<IOException>().WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
		await That(FileSystem.Directory.Exists(name)).IsFalse();
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
		await That(sut.Exists).IsFalse();
#else
		await That(sut.Exists).IsTrue();
#endif
		await That(FileSystem.Directory.Exists(sut.FullName)).IsTrue();
	}

	[Fact]
	public async Task Create_ShouldCreateInBasePath()
	{
		IDirectoryInfo result = FileSystem.DirectoryInfo.New("foo");

		result.Create();

		await That(FileSystem.Directory.Exists("foo")).IsTrue();
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
		await That(result.ToString()).IsEqualTo(path);
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(
		string path)
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

		await That(FileSystem.Directory.Exists(path)).IsTrue();
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

		await That(result.ToString()).IsEqualTo(nameWithSuffix);
		await That(result.Name).IsEqualTo(expectedName.TrimEnd(
			FileSystem.Path.DirectorySeparatorChar,
			FileSystem.Path.AltDirectorySeparatorChar));
		await That(result.FullName).IsEqualTo(FileSystem.Path.Combine(BasePath, expectedName
			.Replace(FileSystem.Path.AltDirectorySeparatorChar,
				FileSystem.Path.DirectorySeparatorChar)));
		await That(FileSystem.Directory.Exists(nameWithSuffix)).IsTrue();
	}

	[Fact]
	public async Task
		CreateDirectory_WithoutAccessRightsToParent_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string restrictedDirectory = @"C:\Windows\System32";
		if (FileSystem is MockFileSystem mockFileSystem)
		{
			restrictedDirectory = @"C:\Restricted directory";
			mockFileSystem.Directory.CreateDirectory(restrictedDirectory);
			mockFileSystem.WithAccessControlStrategy(
				new DefaultAccessControlStrategy((p, _)
					=> !restrictedDirectory.Equals(p, StringComparison.Ordinal)));
		}

		string path = FileSystem.Path.Combine(restrictedDirectory, "my-subdirectory");
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		void Act()
		{
			sut.Create();
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage($"Access to the path '{path}' is denied.");
	}
}
