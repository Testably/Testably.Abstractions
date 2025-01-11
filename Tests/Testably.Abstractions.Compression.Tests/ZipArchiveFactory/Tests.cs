﻿using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public async Task New_ShouldOpenWithReadMode()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream);

		await That(archive.Mode).Should().Be(ZipArchiveMode.Read);
		await That(archive.Entries).Should().HaveExactly(1).Items();
	}

	[SkippableFact]
	public async Task New_UpdateMode_ReadOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		void Act()
		{
			_ = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		}
		
		await That(Act).Should().Throw<ArgumentException>()
			.WithHResult(-2147024809);
	}

	[SkippableFact]
	public async Task New_UpdateMode_ShouldOpenArchive()
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

		await That(archive.Mode).Should().Be(ZipArchiveMode.Update);
		await That(archive.Entries).Should().HaveExactly(1).Items();
	}

	[SkippableTheory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task New_WhenLeaveOpen_ShouldDisposeStreamWhenDisposingArchive(bool leaveOpen)
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
		void Act() => stream.ReadByte();
		
		await That(Act).Should().Throw<ObjectDisposedException>().OnlyIf(!leaveOpen);
	}

	[SkippableTheory]
	[MemberData(nameof(EntryNameEncoding))]
	public async Task New_WithEntryNameEncoding_ShouldUseEncoding(
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

		var singleEntry = await That(readArchive.Entries).Should().HaveSingle();
		if (encodedCorrectly)
		{
			await That(singleEntry.Name).Should().Be(entryName);
		}
		else
		{
			await That(singleEntry.Name).Should().NotBe(entryName);
		}
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<string, Encoding, bool> EntryNameEncoding()
	{
		// ReSharper disable StringLiteralTypo
		TheoryData<string, Encoding, bool> theoryData = new()
		{
			{
				"Dans mes rêves.mp3", Encoding.Default, true
			},
			{
				"Dans mes rêves.mp3", Encoding.ASCII, false
			},
		};
		// ReSharper restore StringLiteralTypo
		return theoryData;
	}
	#pragma warning restore MA0018

	#endregion
}
