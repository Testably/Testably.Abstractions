using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class GetAttributesTests
{
	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.Normal)]
	public async Task GetAttributes_ShouldReturnAttributes(
		FileAttributes attributes, string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		await That(result).IsEqualTo(attributes);
	}

	[Fact]
	public async Task GetAttributes_WhenDotEntry_ShouldHaveHiddenFlag()
	{
		Skip.If(Test.RunsOnWindows);

		const string path = ".env";
		FileAttributes result;

		FileSystem.File.WriteAllText(path, null);

		result = FileSystem.File.GetAttributes(path);

		await That(result).HasFlag(FileAttributes.Hidden);
	}
}
