using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public void Extensibility_ShouldWrapFileSystemInfoOnRealFileSystem(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo fileInfo = FileSystem.FileInfo.New(path);
		IFileSystemExtensibility extensibility = fileInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{fileInfo.GetType()} does not implement IFileSystemExtensibility");

		bool result = extensibility
			.TryGetWrappedInstance(out System.IO.FileSystemInfo? fileSystemInfo);

		if (FileSystem is RealFileSystem)
		{
			result.Should().BeTrue();
			fileSystemInfo!.Name.Should().Be(fileInfo.Name);
		}
		else
		{
			result.Should().BeFalse();
		}
	}

#if FEATURE_FILESYSTEM_LINK
	[Theory]
	[AutoData]
	public void LinkTarget_ShouldBeSetByCreateAsSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.LinkTarget.Should().BeNull();

		sut.CreateAsSymbolicLink(pathToTarget);

		sut.LinkTarget.Should().Be(pathToTarget);
	}
#endif

	[Theory]
	[AutoData]
	public void SetAttributes_Hidden_OnFileStartingWithDot_ShouldBeSet(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		path = $".{path}";
		FileSystem.File.WriteAllText(path, null);

		FileAttributes result1 = FileSystem.File.GetAttributes(path);
		FileSystem.File.SetAttributes(path, FileAttributes.Normal);
		FileAttributes result2 = FileSystem.File.GetAttributes(path);

		result1.Should().Be(FileAttributes.Hidden);
		result2.Should().Be(FileAttributes.Hidden);
	}

	[Theory]
	[AutoData]
	public void SetAttributes_Hidden_OnNormalFile_ShouldBeIgnored(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		FileSystem.File.WriteAllText(path, null);

		FileAttributes result1 = FileSystem.File.GetAttributes(path);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);
		FileAttributes result2 = FileSystem.File.GetAttributes(path);

		result1.Should().Be(FileAttributes.Normal);
		result2.Should().Be(FileAttributes.Normal);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Compressed)]
	[InlineAutoData(FileAttributes.Device)]
	[InlineAutoData(FileAttributes.Encrypted)]
	[InlineAutoData(FileAttributes.IntegrityStream)]
	[InlineAutoData(FileAttributes.SparseFile)]
	[InlineAutoData(FileAttributes.ReparsePoint)]
	public void SetAttributes_ShouldBeIgnoredOnAllPlatforms(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		result.Should().Be(FileAttributes.Normal);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Hidden)]
	public void SetAttributes_ShouldBeIgnoredOnLinux(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		if (Test.RunsOnLinux)
		{
			result.Should().Be(FileAttributes.Normal);
		}
		else
		{
			result.Should().Be(attributes);
		}
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	public void SetAttributes_ShouldBeSupportedOnAllPlatforms(
		FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		result.Should().Be(attributes);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Archive)]
	[InlineAutoData(FileAttributes.NoScrubData)]
	[InlineAutoData(FileAttributes.NotContentIndexed)]
	[InlineAutoData(FileAttributes.Offline)]
	[InlineAutoData(FileAttributes.System)]
	[InlineAutoData(FileAttributes.Temporary)]
	public void SetAttributes_ShouldOnlyWork_OnWindows(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		if (Test.RunsOnWindows)
		{
			result.Should().Be(attributes);
		}
		else
		{
			result.Should().Be(FileAttributes.Normal);
		}
	}
}
