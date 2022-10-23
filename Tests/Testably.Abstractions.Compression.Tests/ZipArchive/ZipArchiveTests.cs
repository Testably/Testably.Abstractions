using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public abstract class ZipArchiveTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected ZipArchiveTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystemExtension_ShouldBeSet(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo");

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.FileSystem.Should().Be(FileSystem);
	}
}