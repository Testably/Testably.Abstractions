using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public abstract partial class ZipArchiveTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_Null_ShouldThrowException(
		CompressionLevel compressionLevel)
	{
		IZipArchive? archive = null;
		Exception? exception = Record.Exception(() =>
		{
			archive!.ExtractToDirectory("foo");
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}
}