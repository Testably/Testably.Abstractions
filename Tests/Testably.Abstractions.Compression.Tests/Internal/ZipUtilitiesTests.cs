using System.IO;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions.Compression.Tests.Internal;

public sealed class ZipUtilitiesTests
{
	[Theory]
	[AutoData]
	public async Task ExtractRelativeToDirectory_FileWithTrailingSlash_ShouldThrowIOException(
		byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		using MemoryStream stream = new(bytes);
		DummyZipArchiveEntry zipArchiveEntry = new(fileSystem, fullName: "foo/", stream: stream);

		void Act()
		{
			zipArchiveEntry.ExtractRelativeToDirectory("foo", false);
		}

		await That(Act).Should().Throw<IOException>()
			.WithMessage("*Zip entry name ends in directory separator character but contains data*")
			.AsWildcard();
	}

	[Fact]
	public async Task ExtractRelativeToDirectory_WithSubdirectory_ShouldCreateSubdirectory()
	{
		MockFileSystem fileSystem = new();
		DummyZipArchiveEntry zipArchiveEntry = new(fileSystem, fullName: "foo/");

		zipArchiveEntry.ExtractRelativeToDirectory("bar", false);

		await That(fileSystem).Should().HaveDirectory("bar");
		await That(fileSystem).Should().HaveDirectory("bar/foo");
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
		#region IZipArchiveEntry Members

		/// <inheritdoc cref="IZipArchiveEntry.Archive" />
		public IZipArchive Archive => archive ?? throw new NotSupportedException();

		/// <inheritdoc cref="IZipArchiveEntry.Comment" />
		public string Comment { get; set; } = comment;

		/// <inheritdoc cref="IZipArchiveEntry.CompressedLength" />
		public long CompressedLength => stream?.Length ?? 0L;

		#pragma warning disable MA0041
		/// <inheritdoc cref="IZipArchiveEntry.Crc32" />
		public uint Crc32 => 0u;
		#pragma warning restore MA0041

		/// <inheritdoc cref="IZipArchiveEntry.ExternalAttributes" />
		public int ExternalAttributes { get; set; }

		/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
		public IFileSystem FileSystem { get; } = fileSystem;

		/// <inheritdoc cref="IZipArchiveEntry.FullName" />
		public string FullName { get; } = fullName ?? "";

		/// <inheritdoc cref="IZipArchiveEntry.IsEncrypted" />
		public bool IsEncrypted { get; } = isEncrypted;

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
