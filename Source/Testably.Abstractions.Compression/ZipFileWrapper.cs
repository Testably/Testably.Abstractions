using System.IO.Compression;
using System.Text;
using System.Threading;
using Testably.Abstractions.Internal;
#if FEATURE_COMPRESSION_STREAM
using System.IO;
#endif
#if FEATURE_COMPRESSION_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

internal sealed class ZipFileWrapper : IZipFile
{
	public ZipFileWrapper(IFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IZipFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destination),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool, Encoding)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding));
#endif

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName));

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory));

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding));

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destination,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destination,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destinationArchiveFileName,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public async Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.CreateFromDirectoryAsync(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding,
				cancellationToken),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string)" />
	public void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				source,
				destinationDirectoryName),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, bool)" />
	public void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				source,
				destinationDirectoryName,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				overwriteFiles: overwriteFiles));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, Encoding)" />
	public void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				source,
				destinationDirectoryName,
				entryNameEncoding),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				entryNameEncoding));
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, Encoding, bool)" />
	public void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				source,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles));
#endif

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string)" />
	public void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName));

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, bool)" />
	public void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles: overwriteFiles));
#endif

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?)" />
	public void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding: entryNameEncoding));

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?, bool)" />
	public void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles));
#endif
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				source,
				destinationDirectoryName,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, bool, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				source,
				destinationDirectoryName,
				overwriteFiles,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				overwriteFiles: overwriteFiles));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, Encoding, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				source,
				destinationDirectoryName,
				entryNameEncoding,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				entryNameEncoding));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, Encoding, bool, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		bool overwriteFiles,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				source,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				source,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.ExtractToDirectoryAsync(string, string, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				sourceArchiveFileName,
				destinationDirectoryName,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.ExtractToDirectoryAsync(string, string, bool, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles: overwriteFiles));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.ExtractToDirectoryAsync(string, string, Encoding?, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding: entryNameEncoding));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.ExtractToDirectoryAsync(string, string, Encoding?, bool, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		bool overwriteFiles,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			() => ZipFile.ExtractToDirectoryAsync(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles,
				cancellationToken),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles));
#endif
	
	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode)" />
	public IZipArchive Open(
		string archiveFileName,
		ZipArchiveMode mode)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.Open(archiveFileName, mode),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode)));

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode, Encoding?)" />
	public IZipArchive Open(
		string archiveFileName,
		ZipArchiveMode mode,
		Encoding? entryNameEncoding)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.Open(archiveFileName, mode, entryNameEncoding),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode,
					entryNameEncoding)));
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.OpenAsync(string, ZipArchiveMode, CancellationToken)" />
	public async Task<IZipArchive> OpenAsync(
		string archiveFileName,
		ZipArchiveMode mode,
		CancellationToken cancellationToken = default)
		=> new ZipArchiveWrapper(FileSystem,
			await Execute.WhenRealFileSystemAsync(FileSystem,
				() => ZipFile.OpenAsync(archiveFileName, mode, cancellationToken),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode)));
#endif
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.OpenAsync(string, ZipArchiveMode, Encoding?, CancellationToken)" />
	public async Task<IZipArchive> OpenAsync(
		string archiveFileName,
		ZipArchiveMode mode,
		Encoding? entryNameEncoding,
		CancellationToken cancellationToken = default)
		=> new ZipArchiveWrapper(FileSystem,
			await Execute.WhenRealFileSystemAsync(FileSystem,
				() => ZipFile.OpenAsync(archiveFileName, mode, entryNameEncoding, cancellationToken),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode,
					entryNameEncoding)));
#endif

	/// <inheritdoc cref="IZipFile.OpenRead(string)" />
	public IZipArchive OpenRead(
		string archiveFileName)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.OpenRead(archiveFileName),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					ZipArchiveMode.Read)));
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.OpenReadAsync(string, CancellationToken)" />
	public async Task<IZipArchive> OpenReadAsync(
		string archiveFileName,
		CancellationToken cancellationToken = default)
		=> new ZipArchiveWrapper(FileSystem,
			await Execute.WhenRealFileSystemAsync(FileSystem,
				() => ZipFile.OpenReadAsync(archiveFileName, cancellationToken),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					ZipArchiveMode.Read)));
#endif

	#endregion
}
