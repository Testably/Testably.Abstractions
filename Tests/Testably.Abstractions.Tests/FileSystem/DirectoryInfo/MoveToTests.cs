using System.IO;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystemInitializer;
#if !NETFRAMEWORK
#endif

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class MoveToTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void MoveTo_ShouldMoveDirectoryWithContent(string source, string destination)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithSubdirectory(source).Initialized(s => s
				   .WithAFile()
				   .WithASubdirectory().Initialized(t => t
					   .WithAFile()
					   .WithASubdirectory()));
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);

		sut.MoveTo(destination);

		FileSystem.Directory.Exists(source).Should().BeFalse();
		FileSystem.Directory.Exists(destination).Should().BeTrue();
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
	public void MoveTo_WithLockedFile_ShouldNotMoveDirectoryAtAll(
		string source, string destination)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
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

		if (Test.RunsOnWindows)
		{
			if (Test.IsNetFramework)
			{
				// On .NET Framework the HResult is "-2146232800", but only in `DirectoryInfo.MoveTo` (not in `Directory.Move`)
				// This peculiar deviation is not supported by the FileSystemMock.
				exception.Should().BeOfType<IOException>();
			}
			else
			{
				exception.Should().BeOfType<IOException>()
				   .Which.HResult.Should().Be(-2147024891);
			}

			FileSystem.Directory.Exists(source).Should().BeTrue();
			FileSystem.Directory.Exists(destination).Should().BeFalse();
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
		else
		{
			exception.Should().BeNull();
			FileSystem.Directory.Exists(source).Should().BeFalse();
			FileSystem.Directory.Exists(destination).Should().BeTrue();
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
	}

	[SkippableTheory]
	[AutoData]
	public void MoveTo_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
		string source, string destination)
	{
		IFileSystemDirectoryInitializer<TFileSystem> initialized =
			FileSystem.Initialize()
			   .WithSubdirectory(source).Initialized(s => s
				   .WithAFile()
				   .WithASubdirectory().Initialized(t => t
					   .WithAFile()
					   .WithASubdirectory()));
		initialized[3].Attributes = FileAttributes.ReadOnly;
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(source);

		sut.MoveTo(destination);

		FileSystem.Directory.Exists(source).Should().BeFalse();
		FileSystem.Directory.Exists(destination).Should().BeTrue();
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