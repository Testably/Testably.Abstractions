using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using Testably.Abstractions.Internal;
#if FEATURE_COMPRESSION_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

internal sealed class ZipArchiveEntryWrapper : IZipArchiveEntry
{
	private readonly ZipArchiveEntry _instance;

	public ZipArchiveEntryWrapper(IFileSystem fileSystem,
		IZipArchive archive,
		ZipArchiveEntry instance)
	{
		_instance = instance;
		FileSystem = fileSystem;
		Archive = archive;
	}

	#region IZipArchiveEntry Members

	/// <inheritdoc cref="IZipArchiveEntry.Archive" />
	public IZipArchive Archive { get; }

#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="IZipArchiveEntry.Comment" />
	public string Comment
	{
		get => _instance.Comment;
		set => _instance.Comment = value;
	}
#endif

	/// <inheritdoc cref="IZipArchiveEntry.CompressedLength" />
	public long CompressedLength
		=> _instance.CompressedLength;

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="IZipArchiveEntry.Crc32" />
	public uint Crc32
		=> _instance.Crc32;
#endif

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="IZipArchiveEntry.ExternalAttributes" />
	public int ExternalAttributes
	{
		get => _instance.ExternalAttributes;
		set => _instance.ExternalAttributes = value;
	}
#endif

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IZipArchiveEntry.FullName" />
	public string FullName
		=> _instance.FullName;

#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="IZipArchiveEntry.IsEncrypted" />
	public bool IsEncrypted
		=> _instance.IsEncrypted;
#endif

	/// <inheritdoc cref="IZipArchiveEntry.LastWriteTime" />
	public DateTimeOffset LastWriteTime
	{
		get => _instance.LastWriteTime;
		set => _instance.LastWriteTime = value;
	}

	/// <inheritdoc cref="IZipArchiveEntry.Length" />
	public long Length
		=> _instance.Length;

	/// <inheritdoc cref="IZipArchiveEntry.Name" />
	public string Name
		=> _instance.Name;

	/// <inheritdoc cref="IZipArchiveEntry.Delete()" />
	public void Delete()
		=> _instance.Delete();

	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFile(string)" />
	public void ExtractToFile(string destinationFileName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => _instance.ExtractToFile(destinationFileName),
			() => ExtractToFile(destinationFileName, overwrite: false));

	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFile(string, bool)" />
	public void ExtractToFile(string destinationFileName, bool overwrite)
	{
		if (destinationFileName == null)
		{
			throw new ArgumentNullException(nameof(destinationFileName));
		}

		Execute.WhenRealFileSystem(FileSystem,
			() => _instance.ExtractToFile(destinationFileName, overwrite),
			() => ZipUtilities.ExtractToFile(this, destinationFileName, overwrite));
	}
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFileAsync(string, CancellationToken)" />
	public Task ExtractToFileAsync(string destinationFileName, CancellationToken cancellationToken = default)
	{
		if (destinationFileName == null)
		{
			throw new ArgumentNullException(nameof(destinationFileName));
		}

		return ExtractToFileImplAsync(destinationFileName, cancellationToken);
		
		async Task ExtractToFileImplAsync(string d, CancellationToken c)
		{
			await Execute.WhenRealFileSystemAsync(FileSystem,
				async () => await _instance.ExtractToFileAsync(d, c),
				() => ZipUtilities.ExtractToFile(this, d, false));
		}
	}
#endif
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFileAsync(string, bool, CancellationToken)" />
	public Task ExtractToFileAsync(string destinationFileName, bool overwrite, CancellationToken cancellationToken = default)
	{
		if (destinationFileName == null)
		{
			throw new ArgumentNullException(nameof(destinationFileName));
		}

		return ExtractToFileImplAsync(destinationFileName, overwrite, cancellationToken);
		
		async Task ExtractToFileImplAsync(string d, bool o, CancellationToken c)
		{
			await Execute.WhenRealFileSystemAsync(FileSystem,
				async () => await _instance.ExtractToFileAsync(d, o, c),
				() => ZipUtilities.ExtractToFile(this, d, o));
		}
	}
#endif

	/// <inheritdoc cref="IZipArchiveEntry.Open()" />
	public Stream Open()
		=> _instance.Open();

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchiveEntry.OpenAsync(CancellationToken)" />
	public Task<Stream> OpenAsync(CancellationToken cancellationToken = default)
		=> _instance.OpenAsync(cancellationToken);
#endif

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _instance.ToString();

	[return: NotNullIfNotNull("instance")]
	internal static IZipArchiveEntry? New(IFileSystem fileSystem, IZipArchive archive,
		ZipArchiveEntry? instance)
	{
		if (instance == null)
		{
			return null;
		}

		return new ZipArchiveEntryWrapper(fileSystem, archive, instance);
	}
}
