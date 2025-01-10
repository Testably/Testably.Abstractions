namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void ToString_ShouldReturnProvidedPath(string fileName)
	{
		FileSystem.File.WriteAllText(fileName, "");
		string fullPath = FileSystem.Path.GetFullPath(fileName);
		IFileVersionInfo fileInfo = FileSystem.FileVersionInfo.GetVersionInfo(fullPath);

		string? result = fileInfo.ToString();

		result.Should().Contain(fullPath);
	}
}
