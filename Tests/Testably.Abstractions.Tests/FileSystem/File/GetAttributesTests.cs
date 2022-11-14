using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class GetAttributesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetAttributes_ShouldReturnAttributes(
		string path, FileAttributes attributes)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		result.Should().Be(attributes);
	}
}
