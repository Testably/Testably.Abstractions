using AutoFixture.Xunit2;
using FluentAssertions;
using System.IO;
using System.IO.Compression;
using Testably.Abstractions;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Xunit;

namespace ZipFile.Tests;

public class ZipFileHelperTests
{
	#region Test Setup

	public IFileSystem FileSystem { get; }
	public ZipFileHelper ZipFileHelper { get; }

	public ZipFileHelperTests()
	{
		FileSystem = new MockFileSystem();
		ZipFileHelper = new ZipFileHelper(FileSystem);
	}

	#endregion

	[Theory]
	[AutoData]
	public void CreateZipFromDirectory_ShouldIncludeAllFilesAndSubdirectories(string directory)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized
			= FileSystem.Initialize()
				.WithSubdirectory(directory).Initialized(s => s
					.WithAFile(".txt")
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()));
		using (Stream zipStream = ZipFileHelper.CreateZipFromDirectory(directory))
		{
			using FileSystemStream fileStream = FileSystem.File.Create("test.zip");
			zipStream.CopyTo(fileStream);
		}

		FileSystem.File.Exists("test.zip");
		ZipArchive archive = new(
			FileSystem.File.OpenRead("test.zip"));
		archive.Entries.Count.Should().Be(4);
		archive.Entries.Should()
			.Contain(e => e.FullName == initialized[1].Name);
		archive.Entries.Should()
			.Contain(e => e.FullName == initialized[2].Name);
		archive.Entries.Should()
			.Contain(e => e.FullName == initialized[3].Name + "/");
		archive.Entries.Should()
			.Contain(e => e.FullName == initialized[3].Name + "/" + initialized[4].Name);
	}

	[Theory]
	[AutoData]
	public void ExtractZipToDirectory_ShouldExtractAllFilesAndDirectories(string directory)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized
			= FileSystem.Initialize()
				.WithSubdirectory("source").Initialized(s => s
					.WithAFile(".txt")
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()))
				.WithSubdirectory(directory);
		using (Stream zipStream = ZipFileHelper.CreateZipFromDirectory("source"))
		{
			using FileSystemStream fileStream = FileSystem.File.Create("test.zip");
			zipStream.CopyTo(fileStream);
		}

		FileSystem.Directory
			.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories)
			.Should().BeEmpty();

		ZipFileHelper.ExtractZipToDirectory(FileSystem.File.OpenRead("test.zip"), directory);

		FileSystem.Directory
			.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories)
			.Should().HaveCount(4);
		FileSystem.File.ReadAllBytes(FileSystem.Path.Combine(directory, initialized[2].Name))
			.Should().BeEquivalentTo(FileSystem.File.ReadAllBytes(initialized[2].FullName));
	}
}