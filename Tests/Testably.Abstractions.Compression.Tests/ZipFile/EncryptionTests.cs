#if FEATURE_COMPRESSION_ENCRYPTION
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public class EncryptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private const string Password = "secret";

	[Test]
	[Arguments(ZipEncryptionMethod.ZipCrypto)]
	[Arguments(ZipEncryptionMethod.Aes256)]
	public async Task CreateFromDirectory_WithPassword_ShouldEncryptFileEntries(
		ZipEncryptionMethod encryptionMethod)
	{
		InitializeSourceDirectory("foo");

		FileSystem.ZipFile.CreateFromDirectory("src", "archive.zip",
			new ZipFileCreationOptions
			{
				Password = Password.AsMemory(),
				EncryptionMethod = encryptionMethod,
			});

		using IZipArchive archive = FileSystem.ZipFile.OpenRead("archive.zip");
		IZipArchiveEntry fileEntry = archive.Entries.Single(e => e.Name == "file.txt");
		IZipArchiveEntry directoryEntry = archive.Entries.Single(e => e.Name == "");
		await That(fileEntry.EncryptionMethod).IsEqualTo(encryptionMethod);
		await That(fileEntry.IsEncrypted).IsTrue();
		await That(directoryEntry.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.None)
			.Because("only the entries for files are encrypted");
		await That(ReadContent(fileEntry, Password)).IsEqualTo("foo");
	}

	[Test]
	public async Task CreateFromDirectory_WithEncryptionAndEmptyPassword_ShouldThrowArgumentException()
	{
		InitializeSourceDirectory("foo");

		void Act()
		{
			FileSystem.ZipFile.CreateFromDirectory("src", "archive.zip",
				new ZipFileCreationOptions
				{
					EncryptionMethod = ZipEncryptionMethod.Aes256,
				});
		}

		await That(Act).Throws<ArgumentException>().WithParamName("options");
		await That(FileSystem.File.Exists("archive.zip")).IsFalse();
	}

	[Test]
	public async Task CreateFromDirectory_WithNullOptions_ShouldThrowArgumentNullException()
	{
		InitializeSourceDirectory("foo");

		void Act()
		{
			FileSystem.ZipFile.CreateFromDirectory("src", "archive.zip",
				(ZipFileCreationOptions)null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("options");
	}

	[Test]
	public async Task CreateFromDirectory_WithOptions_ShouldApplyCompressionLevelAndBaseDirectory()
	{
		InitializeSourceDirectory("foo");

		FileSystem.ZipFile.CreateFromDirectory("src", "archive.zip",
			new ZipFileCreationOptions
			{
				CompressionLevel = CompressionLevel.NoCompression,
				IncludeBaseDirectory = true,
			});

		using IZipArchive archive = FileSystem.ZipFile.OpenRead("archive.zip");
		IZipArchiveEntry fileEntry = archive.Entries.Single(e => e.Name == "file.txt");
		await That(fileEntry.FullName).IsEqualTo("src/file.txt");
		await That(fileEntry.CompressionMethod).IsEqualTo(ZipCompressionMethod.Stored);
		await That(fileEntry.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.None);
	}

	[Test]
	public async Task CreateFromDirectory_WithPassword_ToStream_ShouldEncryptFileEntries()
	{
		InitializeSourceDirectory("foo");
		using MemoryStream stream = new();

		FileSystem.ZipFile.CreateFromDirectory("src", stream,
			new ZipFileCreationOptions
			{
				Password = Password.AsMemory(),
				EncryptionMethod = ZipEncryptionMethod.Aes128,
			});

		stream.Position = 0;
		using IZipArchive archive = FileSystem.ZipArchive.New(stream, ZipArchiveMode.Read);
		IZipArchiveEntry fileEntry = archive.Entries.Single(e => e.Name == "file.txt");
		await That(fileEntry.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes128);
		await That(ReadContent(fileEntry, Password)).IsEqualTo("foo");
	}

	[Test]
	public async Task CreateFromDirectoryAsync_WithPassword_ShouldEncryptFileEntries()
	{
		InitializeSourceDirectory("foo");
		using MemoryStream stream = new();

		await FileSystem.ZipFile.CreateFromDirectoryAsync("src", "archive.zip",
			new ZipFileCreationOptions
			{
				Password = Password.AsMemory(),
				EncryptionMethod = ZipEncryptionMethod.Aes256,
			}, CancellationToken);
		await FileSystem.ZipFile.CreateFromDirectoryAsync("src", stream,
			new ZipFileCreationOptions
			{
				Password = Password.AsMemory(),
				EncryptionMethod = ZipEncryptionMethod.ZipCrypto,
			}, CancellationToken);

		using IZipArchive fileArchive = FileSystem.ZipFile.OpenRead("archive.zip");
		stream.Position = 0;
		using IZipArchive streamArchive = FileSystem.ZipArchive.New(stream, ZipArchiveMode.Read);
		await That(fileArchive.Entries.Single(e => e.Name == "file.txt").EncryptionMethod)
			.IsEqualTo(ZipEncryptionMethod.Aes256);
		await That(streamArchive.Entries.Single(e => e.Name == "file.txt").EncryptionMethod)
			.IsEqualTo(ZipEncryptionMethod.ZipCrypto);
	}

	[Test]
	public async Task ExtractToDirectory_WithCorrectPassword_ShouldExtractEncryptedFiles()
	{
		CreateEncryptedArchive("foo");

		FileSystem.ZipFile.ExtractToDirectory("archive.zip", "dest",
			new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			});

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest", "file.txt")))
			.IsEqualTo("foo");
		await That(FileSystem.Directory.Exists(FileSystem.Path.Combine("dest", "empty")))
			.IsTrue();
	}

	[Test]
	public async Task ExtractToDirectory_WithoutPassword_ShouldThrowInvalidDataException()
	{
		CreateEncryptedArchive("foo");

		void Act()
		{
			FileSystem.ZipFile.ExtractToDirectory("archive.zip", "dest",
				new ZipExtractionOptions());
		}

		await That(Act).Throws<InvalidDataException>();
	}

	[Test]
	public async Task ExtractToDirectory_WithNullOptions_ShouldThrowArgumentNullException()
	{
		CreateEncryptedArchive("foo");

		void Act()
		{
			FileSystem.ZipFile.ExtractToDirectory("archive.zip", "dest",
				(ZipExtractionOptions)null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("options");
	}

	[Test]
	public async Task ExtractToDirectory_WithOverwriteFiles_ShouldOverwriteExistingFiles()
	{
		CreateEncryptedArchive("foo");
		FileSystem.Directory.CreateDirectory("dest");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("dest", "file.txt"), "bar");

		FileSystem.ZipFile.ExtractToDirectory("archive.zip", "dest",
			new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
				OverwriteFiles = true,
			});

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest", "file.txt")))
			.IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToDirectory_WithoutOverwriteFiles_ShouldThrowIOException()
	{
		CreateEncryptedArchive("foo");
		FileSystem.Directory.CreateDirectory("dest");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("dest", "file.txt"), "bar");

		void Act()
		{
			FileSystem.ZipFile.ExtractToDirectory("archive.zip", "dest",
				new ZipExtractionOptions
				{
					Password = Password.AsMemory(),
				});
		}

		await That(Act).Throws<IOException>();
		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest", "file.txt")))
			.IsEqualTo("bar");
	}

	[Test]
	public async Task ExtractToDirectory_FromStream_WithCorrectPassword_ShouldExtractEncryptedFiles()
	{
		CreateEncryptedArchive("foo");
		using FileSystemStream stream = FileSystem.File.OpenRead("archive.zip");

		FileSystem.ZipFile.ExtractToDirectory(stream, "dest",
			new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			});

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest", "file.txt")))
			.IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToDirectoryAsync_WithCorrectPassword_ShouldExtractEncryptedFiles()
	{
		CreateEncryptedArchive("foo");

		await FileSystem.ZipFile.ExtractToDirectoryAsync("archive.zip", "dest1",
			new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			}, CancellationToken);
		await using (FileSystemStream stream = FileSystem.File.OpenRead("archive.zip"))
		{
			await FileSystem.ZipFile.ExtractToDirectoryAsync(stream, "dest2",
				new ZipExtractionOptions
				{
					Password = Password.AsMemory(),
				}, CancellationToken);
		}

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest1", "file.txt")))
			.IsEqualTo("foo");
		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest2", "file.txt")))
			.IsEqualTo("foo");
	}

	private void CreateEncryptedArchive(string content)
	{
		InitializeSourceDirectory(content);
		FileSystem.ZipFile.CreateFromDirectory("src", "archive.zip",
			new ZipFileCreationOptions
			{
				Password = Password.AsMemory(),
				EncryptionMethod = ZipEncryptionMethod.Aes256,
			});
	}

	private void InitializeSourceDirectory(string content)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine("src", "empty"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("src", "file.txt"), content);
	}

	private static string ReadContent(IZipArchiveEntry entry, string password)
	{
		using StreamReader reader = new(entry.Open(password));
		return reader.ReadToEnd();
	}
}
#endif
