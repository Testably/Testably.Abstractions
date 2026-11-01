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

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="IZipArchiveEntry.CompressionMethod" />
	public ZipCompressionMethod CompressionMethod
		=> _instance.CompressionMethod;
#endif

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

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.EncryptionMethod" />
	public ZipEncryptionMethod EncryptionMethod
		=> _instance.EncryptionMethod;
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

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="IZipArchiveEntry.VersionMadeBy" />
	public ushort VersionMadeBy
		=> _instance.VersionMadeBy;
#endif

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

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFile(string, ZipExtractionOptions)" />
	public void ExtractToFile(string destinationFileName, ZipExtractionOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(destinationFileName);

		Execute.WhenRealFileSystem(FileSystem,
			() => _instance.ExtractToFile(destinationFileName, options),
			() => ZipUtilities.ExtractToFile(this, destinationFileName, options.OverwriteFiles,
				options.Password));
	}
#endif

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

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.ExtractToFileAsync(string, ZipExtractionOptions, CancellationToken)" />
	public Task ExtractToFileAsync(string destinationFileName, ZipExtractionOptions options,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(destinationFileName);

		return Execute.WhenRealFileSystemAsync(FileSystem,
			() => _instance.ExtractToFileAsync(destinationFileName, options, cancellationToken),
			() => ZipUtilities.ExtractToFile(this, destinationFileName, options.OverwriteFiles,
				options.Password));
	}
#endif

	/// <inheritdoc cref="IZipArchiveEntry.Open()" />
	public Stream Open()
		=> _instance.Open();

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="IZipArchiveEntry.Open(FileAccess)" />
	public Stream Open(FileAccess access)
		=> _instance.Open(access);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.Open(ReadOnlySpan{char})" />
	public Stream Open(ReadOnlySpan<char> password)
		=> _instance.Open(password);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.Open(FileAccess, ReadOnlySpan{char})" />
	public Stream Open(FileAccess access, ReadOnlySpan<char> password)
		=> _instance.Open(access, password);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchiveEntry.OpenAsync(CancellationToken)" />
	public Task<Stream> OpenAsync(CancellationToken cancellationToken = default)
		=> _instance.OpenAsync(cancellationToken);
#endif

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="IZipArchiveEntry.OpenAsync(FileAccess, CancellationToken)" />
	public Task<Stream> OpenAsync(FileAccess access, CancellationToken cancellationToken = default)
		=> _instance.OpenAsync(access, cancellationToken);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.OpenAsync(ReadOnlySpan{char}, CancellationToken)" />
	public Task<Stream> OpenAsync(ReadOnlySpan<char> password,
		CancellationToken cancellationToken = default)
		=> _instance.OpenAsync(password, cancellationToken);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="IZipArchiveEntry.OpenAsync(FileAccess, ReadOnlySpan{char}, CancellationToken)" />
	public Task<Stream> OpenAsync(FileAccess access, ReadOnlySpan<char> password,
		CancellationToken cancellationToken = default)
		=> _instance.OpenAsync(access, password, cancellationToken);
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
