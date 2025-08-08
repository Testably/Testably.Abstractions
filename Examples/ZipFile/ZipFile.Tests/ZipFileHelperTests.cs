using aweXpect;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using Xunit;

namespace Testably.Abstractions.Examples.ZipFile.Tests;

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
	[InlineData("foo")]
	public async Task CreateZipFromDirectory_ShouldIncludeAllFilesAndSubdirectories(
		string directory)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized
			= FileSystem.Initialize()
				.WithSubdirectory(directory)
				.Initialized(s => s
					.WithAFile(".txt")
					.WithAFile()
					.WithASubdirectory()
					.Initialized(t => t
						.WithAFile()));
		using (Stream zipStream = ZipFileHelper.CreateZipFromDirectory(directory))
		{
			using FileSystemStream fileStream = FileSystem.File.Create("test.zip");
			zipStream.CopyTo(fileStream);
		}

		FileSystem.File.Exists("test.zip");
		ZipArchive archive = new(
			FileSystem.File.OpenRead("test.zip"));
		await Expect.That(archive.Entries).HasCount(4);
		await Expect.That(archive.Entries)
			.Contains(e => e.FullName == initialized[1].Name);
		await Expect.That(archive.Entries)
			.Contains(e => e.FullName == initialized[2].Name);
		await Expect.That(archive.Entries)
			.Contains(e => e.FullName == initialized[3].Name + "/");
		await Expect.That(archive.Entries)
			.Contains(e => e.FullName == initialized[3].Name + "/" + initialized[4].Name);
	}

	[Theory]
	[InlineData("foo")]
	public async Task ExtractZipToDirectory_ShouldExtractAllFilesAndDirectories(
		string directory)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized
			= FileSystem.Initialize()
				.WithSubdirectory("source")
				.Initialized(s => s
					.WithAFile(".txt")
					.WithAFile()
					.WithASubdirectory()
					.Initialized(t => t
						.WithAFile()))
				.WithSubdirectory(directory);
		using (Stream zipStream = ZipFileHelper.CreateZipFromDirectory("source"))
		{
			using FileSystemStream fileStream = FileSystem.File.Create("test.zip");
			zipStream.CopyTo(fileStream);
		}

		await Expect.That(FileSystem.Directory
				.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories))
			.IsEmpty();

		ZipFileHelper.ExtractZipToDirectory(FileSystem.File.OpenRead("test.zip"),
			directory);

		await Expect.That(FileSystem.Directory
				.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories))
			.HasCount(4);
		await Expect.That(FileSystem
				.File.ReadAllBytes(FileSystem.Path.Combine(directory, initialized[2].Name)))
			.IsEqualTo(FileSystem.File.ReadAllBytes(initialized[2].FullName));
	}
}
