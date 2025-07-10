using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class MoveToTests
{
	[Theory]
	[AutoData]
	public async Task MoveTo_ShouldMoveDirectoryWithContent(string source, string destination)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);

		sut.MoveTo(destination);

		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		await That(FileSystem.Directory.GetFiles(destination, initialized[1].Name)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[2].Name)).HasSingle();
		await That(FileSystem.Directory.GetFiles(destination, initialized[3].Name,	SearchOption.AllDirectories)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_ShouldUpdatePropertiesOfDirectoryInfo(
		string source, string destination)
	{
		FileSystem.Initialize()
			.WithSubdirectory(source).Initialized(s => s
				.WithAFile()
				.WithASubdirectory().Initialized(t => t
					.WithAFile()
					.WithASubdirectory()));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);

		sut.MoveTo(destination);

		await That(sut.FullName.TrimEnd(FileSystem.Path.DirectorySeparatorChar))
			.IsEqualTo(FileSystem.Path.GetFullPath(destination));
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_WithLockedFile_ShouldMoveDirectory_NotOnWindows(
		string source, string destination)
	{
		Skip.If(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		void Act()
		{
			sut.MoveTo(destination);
		}

		await That(Act).DoesNotThrow();
		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		await That(destinationDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_WithLockedFile_ShouldThrowIOException_AndNotMoveDirectory_OnWindows(
		string source, string destination)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		void Act()
		{
			sut.MoveTo(destination);
		}

		if (Test.IsNetFramework)
		{
			// On .NET Framework the HResult is "-2146232800", but only in `DirectoryInfo.MoveTo` (not in `Directory.Move`)
			// This peculiar deviation is not supported by the FileSystemMock.
			await That(Act).Throws<IOException>();
		}
		else
		{
			await That(Act).Throws<IOException>().WithHResult(-2147024891);
		}

		await That(FileSystem.Directory.Exists(source)).IsTrue();
		await That(FileSystem.Directory.Exists(destination)).IsFalse();
		IDirectoryInfo sourceDirectory =
			FileSystem.DirectoryInfo.New(source);
		await That(sourceDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(sourceDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(sourceDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle();
		await That(sourceDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task MoveTo_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
		string source, string destination)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		initialized[3].Attributes = FileAttributes.ReadOnly;
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);

		sut.MoveTo(destination);

		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		await That(destinationDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle().Which.For(x => x.Attributes, it => it.HasFlag(FileAttributes.ReadOnly));
		await That(destinationDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}
}
