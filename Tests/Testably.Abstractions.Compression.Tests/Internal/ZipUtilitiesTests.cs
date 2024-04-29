using System.IO;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions.Compression.Tests.Internal;

public sealed class ZipUtilitiesTests
{
	[Theory]
	[AutoData]
	public void ExtractRelativeToDirectory_FileWithTrailingSlash_ShouldThrowIOException(
		byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		using MemoryStream stream = new(bytes);
		DummyZipArchiveEntry zipArchiveEntry = new(fileSystem, fullName: "foo/", stream: stream);

		Exception? exception = Record.Exception(() =>
		{
			zipArchiveEntry.ExtractRelativeToDirectory("foo", false);
		});

		exception.Should().BeException<IOException>(
			messageContains:
			"Zip entry name ends in directory separator character but contains data");
	}

	[Fact]
	public void ExtractRelativeToDirectory_WithSubdirectory_ShouldCreateSubdirectory()
	{
		MockFileSystem fileSystem = new();
		DummyZipArchiveEntry zipArchiveEntry = new(fileSystem, fullName: "foo/");

		zipArchiveEntry.ExtractRelativeToDirectory("bar", false);

		fileSystem.Directory.Exists("bar").Should().BeTrue();
		fileSystem.Directory.Exists("bar/foo").Should().BeTrue();
	}

	private sealed class DummyZipArchiveEntry(
		IFileSystem fileSystem,
		IZipArchive? archive = null,
		string? fullName = "",
		string? name = "",
		string comment = "",
		bool isEncrypted = false,
		Stream? stream = null)
		: IZipArchiveEntry
	{
		/// <inheritdoc cref="IZipArchiveEntry.Comment" />
		public string Comment { get; set; } = comment;

		/// <inheritdoc cref="IZipArchiveEntry.IsEncrypted" />
		public bool IsEncrypted { get; } = isEncrypted;

		#region IZipArchiveEntry Members

		/// <inheritdoc cref="IZipArchiveEntry.Archive" />
		public IZipArchive Archive => archive ?? throw new NotSupportedException();

		/// <inheritdoc cref="IZipArchiveEntry.CompressedLength" />
		public long CompressedLength => stream?.Length ?? 0L;

		/// <inheritdoc cref="IZipArchiveEntry.Crc32" />
		public uint Crc32 => 0u;

		/// <inheritdoc cref="IZipArchiveEntry.ExternalAttributes" />
		public int ExternalAttributes { get; set; }

		/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
		public IFileSystem FileSystem { get; } = fileSystem;

		/// <inheritdoc cref="IZipArchiveEntry.FullName" />
		public string FullName { get; } = fullName ?? "";

		/// <inheritdoc cref="IZipArchiveEntry.LastWriteTime" />
		public DateTimeOffset LastWriteTime { get; set; }

		/// <inheritdoc cref="IZipArchiveEntry.Length" />
		public long Length => stream?.Length ?? 0L;

		/// <inheritdoc cref="IZipArchiveEntry.Name" />
		public string Name { get; } = name ?? "";

		/// <inheritdoc cref="IZipArchiveEntry.Delete()" />
		public void Delete()
			=> throw new NotSupportedException();

		/// <inheritdoc cref="IZipArchiveEntry.ExtractToFile(string)" />
		public void ExtractToFile(string destinationFileName)
			=> throw new NotSupportedException();

		/// <inheritdoc cref="IZipArchiveEntry.ExtractToFile(string, bool)" />
		public void ExtractToFile(string destinationFileName, bool overwrite)
			=> throw new NotSupportedException();

		/// <inheritdoc cref="IZipArchiveEntry.Open()" />
		public Stream Open()
			=> stream ?? throw new NotSupportedException();

		#endregion
	}
}
