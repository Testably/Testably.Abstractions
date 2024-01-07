using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void New_ShouldOpenWithReadMode()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream);

		archive.Mode.Should().Be(ZipArchiveMode.Read);
		archive.Entries.Should().HaveCount(1);
	}

	[SkippableFact]
	public void New_UpdateMode_ReadOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[SkippableFact]
	public void New_UpdateMode_ShouldOpenArchive()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);

		archive.Mode.Should().Be(ZipArchiveMode.Update);
		archive.Entries.Should().HaveCount(1);
	}

	[SkippableTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void New_WhenLeaveOpen_ShouldDisposeStreamWhenDisposingArchive(bool leaveOpen)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update, leaveOpen);

		archive.Dispose();
		Exception? exception = Record.Exception(() => stream.ReadByte());
		if (leaveOpen)
		{
			exception.Should().BeNull();
		}
		else
		{
			exception.Should().BeOfType<ObjectDisposedException>();
		}
	}

	[SkippableTheory]
	[MemberData(nameof(EntryNameEncoding))]
	public void New_WithEntryNameEncoding_ShouldUseEncoding(
		string entryName, Encoding encoding, bool encodedCorrectly)
	{
		FileSystem.Initialize()
			.WithFile(entryName);

		FileSystemStream stream = FileSystem.File.Create("destination.zip");

		IZipArchive writeArchive =
			FileSystem.ZipArchive().New(stream, ZipArchiveMode.Create, false, encoding);
		writeArchive.CreateEntry(entryName);
		writeArchive.Dispose();

		using IZipArchive readArchive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		readArchive.Entries.Count.Should().Be(1);
		if (encodedCorrectly)
		{
			readArchive.Entries.Should().Contain(e => e.Name == entryName);
		}
		else
		{
			readArchive.Entries.Should().NotContain(e => e.Name == entryName);
		}
	}

	#region Helpers

	public static TheoryData<string, Encoding, bool> EntryNameEncoding()
	{
		// ReSharper disable StringLiteralTypo
		TheoryData<string, Encoding, bool> theoryData = new()
		{
			{ "Dans mes rêves.mp3", Encoding.Default, true },
			{ "Dans mes rêves.mp3", Encoding.ASCII, false }
		};
		// ReSharper restore StringLiteralTypo
		return theoryData;
	}

	#endregion
}
