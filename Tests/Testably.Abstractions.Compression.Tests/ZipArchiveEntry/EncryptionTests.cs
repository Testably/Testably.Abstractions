#if FEATURE_COMPRESSION_ENCRYPTION
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

[FileSystemTests]
public class EncryptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private const string Password = "secret";

	[Test]
	[Arguments(CompressionLevel.NoCompression, ZipCompressionMethod.Stored)]
	[Arguments(CompressionLevel.Optimal, ZipCompressionMethod.Deflate)]
	public async Task CompressionMethod_ShouldReturnCompressionMethodOfEntry(
		CompressionLevel compressionLevel, ZipCompressionMethod expectedCompressionMethod)
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			IZipArchiveEntry entry = archive.CreateEntry("foo.txt", compressionLevel);
			WriteContent(entry, "some content that can be compressed");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry result = readArchive.GetEntry("foo.txt")!;

		await That(result.CompressionMethod).IsEqualTo(expectedCompressionMethod);
	}

	[Test]
	public async Task ExtractToFile_WithCorrectPassword_ShouldExtractEntry()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		entry.ExtractToFile("dest1.txt", new ZipExtractionOptions
		{
			Password = Password.AsMemory(),
		});
		await entry.ExtractToFileAsync("dest2.txt", new ZipExtractionOptions
		{
			Password = Password.AsMemory(),
		}, CancellationToken);

		await That(FileSystem.File.ReadAllText("dest1.txt")).IsEqualTo("foo");
		await That(FileSystem.File.ReadAllText("dest2.txt")).IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToFile_WithEmptyPassword_UnencryptedEntry_ShouldExtractEntry()
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			WriteContent(archive.CreateEntry("foo.txt"), "foo");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = readArchive.GetEntry("foo.txt")!;

		entry.ExtractToFile("dest.txt", new ZipExtractionOptions());

		await That(FileSystem.File.ReadAllText("dest.txt")).IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToFile_WithoutPassword_EncryptedEntry_ShouldThrowArgumentException()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		void Act()
		{
			entry.ExtractToFile("dest.txt", new ZipExtractionOptions());
		}

		await That(Act).Throws<ArgumentException>().WithParamName("password")
			.Because(
				"unlike ExtractToDirectory, ExtractToFile opens the entry with the empty password");
	}

	[Test]
	public async Task ExtractToFile_WithNullOptions_ShouldThrowArgumentNullException()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		void Act()
		{
			entry.ExtractToFile("dest.txt", (ZipExtractionOptions)null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("options");
	}

	[Test]
	public async Task ExtractToFile_WithOverwriteFiles_ShouldOverwriteExistingFile()
	{
		CreateArchiveWithEncryptedEntry("foo");
		FileSystem.File.WriteAllText("dest.txt", "bar");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		entry.ExtractToFile("dest.txt", new ZipExtractionOptions
		{
			Password = Password.AsMemory(),
			OverwriteFiles = true,
		});

		await That(FileSystem.File.ReadAllText("dest.txt")).IsEqualTo("foo");
	}

	[Test]
	public async Task ExtractToFile_WithoutOverwriteFiles_ShouldThrowIOException()
	{
		CreateArchiveWithEncryptedEntry("foo");
		FileSystem.File.WriteAllText("dest.txt", "bar");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		void Act()
		{
			entry.ExtractToFile("dest.txt", new ZipExtractionOptions
			{
				Password = Password.AsMemory(),
			});
		}

		await That(Act).Throws<IOException>();
		await That(FileSystem.File.ReadAllText("dest.txt")).IsEqualTo("bar");
	}

	[Test]
	public async Task Open_EncryptedEntryWithoutPassword_ShouldThrowInvalidDataException()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		void Act()
		{
			using Stream _ = entry.Open();
		}

		await That(Act).Throws<InvalidDataException>();
	}

	[Test]
	public async Task Open_WithFileAccess_ShouldOpenEntry()
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			WriteContent(archive.CreateEntry("foo.txt"), "foo");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = readArchive.GetEntry("foo.txt")!;
		using Stream stream = entry.Open(FileAccess.Read);

		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsFalse();
		await That(new StreamReader(stream).ReadToEnd()).IsEqualTo("foo");
	}

	[Test]
	public async Task Open_WithPassword_ShouldDecryptEntry()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		string result1 = new StreamReader(entry.Open(Password)).ReadToEnd();
		string result2 = new StreamReader(entry.Open(FileAccess.Read, Password)).ReadToEnd();

		await That(entry.EncryptionMethod).IsEqualTo(ZipEncryptionMethod.Aes256);
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("foo");
	}

	[Test]
	public async Task OpenAsync_WithFileAccessAndPassword_ShouldDecryptEntry()
	{
		CreateArchiveWithEncryptedEntry("foo");
		using IZipArchive archive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.GetEntry("foo.txt")!;

		await using Stream stream1 = await entry.OpenAsync(Password, CancellationToken);
		await using Stream stream2 =
			await entry.OpenAsync(FileAccess.Read, Password, CancellationToken);
		string result1 = await new StreamReader(stream1).ReadToEndAsync(CancellationToken);
		string result2 = await new StreamReader(stream2).ReadToEndAsync(CancellationToken);

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("foo");
	}

	[Test]
	public async Task OpenAsync_WithFileAccess_ShouldOpenEntry()
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			WriteContent(archive.CreateEntry("foo.txt"), "foo");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = readArchive.GetEntry("foo.txt")!;
		await using Stream stream = await entry.OpenAsync(FileAccess.Read, CancellationToken);

		await That(stream.CanRead).IsTrue();
		await That(await new StreamReader(stream).ReadToEndAsync(CancellationToken))
			.IsEqualTo("foo");
	}

	[Test]
	public async Task VersionMadeBy_ShouldNotBeZero()
	{
		using (IZipArchive archive = OpenArchive(ZipArchiveMode.Create))
		{
			WriteContent(archive.CreateEntry("foo.txt"), "foo");
		}

		using IZipArchive readArchive = OpenArchive(ZipArchiveMode.Read);
		IZipArchiveEntry entry = readArchive.GetEntry("foo.txt")!;

		await That(entry.VersionMadeBy).IsNotEqualTo((ushort)0);
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

	private static void WriteContent(IZipArchiveEntry entry, string content)
	{
		using StreamWriter writer = new(entry.Open());
		writer.Write(content);
	}
}
#endif
