using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class MoveToTests
{
	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldMoveDirectoryWithContent(string source, string destination)
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

		FileSystem.Should().NotHaveDirectory(source);
		FileSystem.Should().HaveDirectory(destination);
		FileSystem.Directory.GetFiles(destination, initialized[1].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[2].Name)
			.Should().ContainSingle();
		FileSystem.Directory.GetFiles(destination, initialized[3].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
		FileSystem.Directory.GetDirectories(destination, initialized[4].Name,
				SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldUpdatePropertiesOfDirectoryInfo(
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

		sut.FullName.TrimEnd(FileSystem.Path.DirectorySeparatorChar)
			.Should().Be(FileSystem.Path.GetFullPath(destination));
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_WithLockedFile_ShouldMoveDirectory_NotOnWindows(
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

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destination);
		});

		exception.Should().BeNull();
		FileSystem.Should().NotHaveDirectory(source);
		FileSystem.Should().HaveDirectory(destination);
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		destinationDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		destinationDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		destinationDirectory
			.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
		destinationDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_WithLockedFile_ShouldThrowIOException_AndNotMoveDirectory_OnWindows(
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

		Exception? exception = Record.Exception(() =>
		{
			sut.MoveTo(destination);
		});

		if (Test.IsNetFramework)
		{
			// On .NET Framework the HResult is "-2146232800", but only in `DirectoryInfo.MoveTo` (not in `Directory.Move`)
			// This peculiar deviation is not supported by the FileSystemMock.
			exception.Should().BeException<IOException>();
		}
		else
		{
			exception.Should().BeException<IOException>(hResult: -2147024891);
		}

		FileSystem.Should().HaveDirectory(source);
		FileSystem.Should().NotHaveDirectory(destination);
		IDirectoryInfo sourceDirectory =
			FileSystem.DirectoryInfo.New(source);
		sourceDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		sourceDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		sourceDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
		sourceDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
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

		FileSystem.Should().NotHaveDirectory(source);
		FileSystem.Should().HaveDirectory(destination);
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		destinationDirectory.GetFiles(initialized[1].Name)
			.Should().ContainSingle();
		destinationDirectory.GetDirectories(initialized[2].Name)
			.Should().ContainSingle();
		destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)
			.Should().ContainSingle().Which.Attributes.Should()
			.HaveFlag(FileAttributes.ReadOnly);
		destinationDirectory
			.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)
			.Should().ContainSingle();
	}
}
