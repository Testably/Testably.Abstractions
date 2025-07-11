using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[InlineData(259)]
	[InlineData(260)]
	public async Task New_PathTooLong_ShouldThrowPathTooLongException_OnNetFramework(
		int maxLength)
	{
		string rootDrive = FileTestHelper.RootDrive(Test);
		string path = new('a', maxLength - rootDrive.Length);

		void Act()
		{
			_ = FileSystem.FileInfo.New(rootDrive + path);
		}

		if (Test.IsNetFramework)
		{
			await That(Act).Throws<PathTooLongException>().WithHResult(-2147024690);
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Theory]
	[AutoData]
	public async Task New_ShouldCreateNewFileInfoFromPath(string path)
	{
		IFileInfo result = FileSystem.FileInfo.New(path);

		await That(result.ToString()).IsEqualTo(path);
		await That(result.Exists).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task New_ShouldOpenWithExistingContent(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		IFileInfo sut = FileSystem.FileInfo.New(path);

		using StreamReader streamReader = new(sut.OpenRead());
		string result = streamReader.ReadToEnd();
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task New_ShouldSetLength(string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open,
			FileAccess.Read, FileShare.Write);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		long result = sut.Length;

		stream.Dispose();

		await That(result).IsEqualTo(bytes.Length);
	}

	[Theory]
	[AutoData]
	public async Task New_WithTrailingDirectorySeparatorChar_ShouldHaveEmptyName(string path)
	{
		IFileInfo result =
			FileSystem.FileInfo.New($"{path}{FileSystem.Path.DirectorySeparatorChar}");

		await That(result.Name).IsEqualTo(string.Empty);
	}

	[Fact]
	public async Task New_WithUnicodeWhitespace_ShouldNotThrow()
	{
		void Act()
		{
			FileSystem.FileInfo.New("\u00A0"); // Unicode char that's treated as whitespace
		}

		if (Test.IsNetFramework)
		{
			await That(Act).ThrowsExactly<ArgumentException>();
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Fact]
	public async Task New_WithWhitespace_ShouldThrowOnlyOnWindows()
	{
		void Act()
		{
			FileSystem.FileInfo.New("   ");
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).ThrowsExactly<ArgumentException>();
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Fact]
	public async Task Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IFileInfo? result = FileSystem.FileInfo.Wrap(null);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task Wrap_ShouldWrapFromFileInfo(string path)
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.FileInfo fileInfo = new(path);

		IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

		await That(result.FullName).IsEqualTo(fileInfo.FullName);
		await That(result.Exists).IsEqualTo(fileInfo.Exists);
	}

	[Theory]
	[AutoData]
	public async Task Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException(string path)
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		           mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.FileInfo fileInfo = new(path);

		void Act()
		{
			_ = FileSystem.FileInfo.Wrap(fileInfo);
		}

		await That(Act).ThrowsExactly<NotSupportedException>()
			.WithMessage("Wrapping a FileInfo in a simulated file system is not supported")
			.AsPrefix();
	}
}
