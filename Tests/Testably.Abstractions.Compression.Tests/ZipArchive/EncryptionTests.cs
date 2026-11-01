#if FEATURE_COMPRESSION_ENCRYPTION
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

[FileSystemTests]
public class EncryptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private const string Password = "secret";

	[Test]
	public async Task CreateEntry_WithPassword_ShouldEncryptEntry()
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			IZipArchiveEntry entry1 = archive.CreateEntry("foo.txt", Password,
				ZipEncryptionMethod.Aes256);
			WriteContent(entry1, "foo");
			IZipArchiveEntry entry2 = archive.CreateEntry("bar.txt", CompressionLevel.NoCompression,
				Password, ZipEncryptionMethod.ZipCrypto);
			WriteContent(entry2, "bar");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry foo = readArchive.GetEntry("foo.txt")!;
		IZipArchiveEntry bar = readArchive.GetEntry("bar.txt")!;
		await That(foo.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes256);
		await That(bar.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.ZipCrypto);
		await That(ReadContent(foo)).IsEqualTo("foo");
		await That(ReadContent(bar)).IsEqualTo("bar");
	}

	[Test]
	public async Task CreateEntryFromFile_WithEncryptionAndEmptyPassword_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllText("source.txt", "foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Create);

		void Act()
		{
			archive.CreateEntryFromFile("source.txt", "foo.txt", ReadOnlySpan<char>.Empty,
				ZipEncryptionMethod.Aes256);
		}

		await That(Act).Throws<ArgumentException>().WithParamName("password");
	}

	[Test]
	public async Task CreateEntryFromFile_WithPassword_ShouldEncryptEntry()
	{
		FileSystem.File.WriteAllText("source1.txt", "foo");
		FileSystem.File.WriteAllText("source2.txt", "bar");

		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			archive.CreateEntryFromFile("source1.txt", "foo.txt", Password,
				ZipEncryptionMethod.Aes256);
			archive.CreateEntryFromFile("source2.txt", "bar.txt", CompressionLevel.NoCompression,
				Password, ZipEncryptionMethod.Aes128);
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry foo = readArchive.GetEntry("foo.txt")!;
		IZipArchiveEntry bar = readArchive.GetEntry("bar.txt")!;
		await That(foo.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes256);
		await That(bar.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes128);
		await That(ReadContent(foo)).IsEqualTo("foo");
		await That(ReadContent(bar)).IsEqualTo("bar");
	}

	[Test]
	public async Task CreateEntryFromFile_WithoutEncryption_ShouldIgnorePassword()
	{
		FileSystem.File.WriteAllText("source.txt", "foo");

		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			archive.CreateEntryFromFile("source.txt", "foo.txt", Password,
				ZipEncryptionMethod.None);
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry foo = readArchive.GetEntry("foo.txt")!;
		await That(foo.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.None);
		await That(foo.IsEncrypted).IsFalse();
	}

	[Test]
	public async Task CreateEntryFromFileAsync_WithPassword_ShouldEncryptEntry()
	{
		FileSystem.File.WriteAllText("source1.txt", "foo");
		FileSystem.File.WriteAllText("source2.txt", "bar");

		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			await archive.CreateEntryFromFileAsync("source1.txt", "foo.txt", Password.AsMemory(),
				ZipEncryptionMethod.Aes256, CancellationToken);
			await archive.CreateEntryFromFileAsync("source2.txt", "bar.txt",
				CompressionLevel.SmallestSize, Password.AsMemory(), ZipEncryptionMethod.ZipCrypto,
				CancellationToken);
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry foo = readArchive.GetEntry("foo.txt")!;
		IZipArchiveEntry bar = readArchive.GetEntry("bar.txt")!;
		await That(foo.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes256);
		await That(bar.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.ZipCrypto);
		await That(ReadContent(foo)).IsEqualTo("foo");
		await That(ReadContent(bar)).IsEqualTo("bar");
	}

	[Test]
	public async Task ExtractToDirectory_WithOptions_ShouldExtractEncryptedEntries()
	{
		CreateArchiveWithEncryptedEntry("foo");

		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Read))
		{
			archive.ExtractToDirectory("dest1", new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			});
		}

		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Read))
		{
			await archive.ExtractToDirectoryAsync("dest2", new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			}, CancellationToken);
		}

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest1", "foo.txt")))
			.IsEqualTo("foo");
		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest2", "foo.txt")))
			.IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToDirectory_WithNullOptions_ShouldThrowArgumentNullException()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);

		void Act()
		{
			archive.ExtractToDirectory("dest", (ZipExtractionOptions)null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("options");
	}

	[Test]
	public async Task ExtractToDirectory_WithOverwriteFiles_ShouldOverwriteExistingFiles()
	{
		CreateArchiveWithEncryptedEntry("foo");
		FileSystem.Directory.CreateDirectory("dest");
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("dest", "foo.txt"), "bar");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);

		archive.ExtractToDirectory("dest", new ZipExtractionOptions
		{
			Password = Password.AsMemory(),
			OverwriteFiles = true,
		});

		await That(FileSystem.File.ReadAllText(FileSystem.Path.Combine("dest", "foo.txt")))
			.IsEqualTo("foo");
	}

	private void CreateArchiveWithEncryptedEntry(string content)
	{
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Create);
		IZipArchiveEntry entry = archive.CreateEntry("foo.txt", Password,
			ZipEncryptionMethod.Aes256);
		WriteContent(entry, content);
	}

	private IZipArchive OpenArchive(ZipArchiveMode mode)
		=> FileSystem.ZipFile.Open("archive.zip", mode);

	private static string ReadContent(IZipArchiveEntry entry)
	{
		using StreamReader reader = new(entry.Open(Password));
		return reader.ReadToEnd();
	}

	private static void WriteContent(IZipArchiveEntry entry, string content)
	{
		using StreamWriter writer = new(entry.Open());
		writer.Write(content);
	}
}
#endif
