namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData("/foo")]
	[InlineData("./foo")]
	[InlineData("foo")]
	public void ToString_ShouldReturnProvidedPath(string path)
	{
		IFileVersionInfo fileInfo = FileSystem.FileVersionInfo.GetVersionInfo(path);

		string? result = fileInfo.ToString();

		result.Should().Be(path);
	}
}
