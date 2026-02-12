using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task Extensibility_ShouldWrapFileSystemInfoOnRealFileSystem(
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
			await That(result).IsTrue();
			await That(fileSystemInfo!.Name).IsEqualTo(fileInfo.Name);
		}
		else
		{
			await That(result).IsFalse();
		}
	}

#if FEATURE_FILESYSTEM_LINK
	[Theory]
	[AutoData]
	public async Task LinkTarget_ShouldBeSetByCreateAsSymbolicLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.LinkTarget).IsNull();

		sut.CreateAsSymbolicLink(pathToTarget);

		await That(sut.LinkTarget).IsEqualTo(pathToTarget);
	}
#endif

	[Theory]
	[AutoData]
	public async Task SetAttributes_Hidden_OnFileStartingWithDot_ShouldBeSet(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		path = $".{path}";
		FileSystem.File.WriteAllText(path, null);

		FileAttributes result1 = FileSystem.File.GetAttributes(path);
		FileSystem.File.SetAttributes(path, FileAttributes.Normal);
		FileAttributes result2 = FileSystem.File.GetAttributes(path);

		await That(result1).IsEqualTo(FileAttributes.Hidden);
		await That(result2).IsEqualTo(FileAttributes.Hidden);
	}

	[Theory]
	[AutoData]
	public async Task SetAttributes_Hidden_OnNormalFile_ShouldBeIgnored(string path)
	{
		Skip.IfNot(Test.RunsOnLinux);

		FileSystem.File.WriteAllText(path, null);

		FileAttributes result1 = FileSystem.File.GetAttributes(path);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);
		FileAttributes result2 = FileSystem.File.GetAttributes(path);

		await That(result1).IsEqualTo(FileAttributes.Normal);
		await That(result2).IsEqualTo(FileAttributes.Normal);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Compressed)]
	[InlineAutoData(FileAttributes.Device)]
	[InlineAutoData(FileAttributes.Encrypted)]
	[InlineAutoData(FileAttributes.IntegrityStream)]
	[InlineAutoData(FileAttributes.SparseFile)]
	[InlineAutoData(FileAttributes.ReparsePoint)]
	public async Task SetAttributes_ShouldBeIgnoredOnAllPlatforms(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		await That(result).IsEqualTo(FileAttributes.Normal);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Hidden)]
	public async Task SetAttributes_ShouldBeIgnoredOnLinux(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		if (Test.RunsOnLinux)
		{
			await That(result).IsEqualTo(FileAttributes.Normal);
		}
		else
		{
			await That(result).IsEqualTo(attributes);
		}
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	public async Task SetAttributes_ShouldBeSupportedOnAllPlatforms(
		FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		await That(result).IsEqualTo(attributes);
	}

	[Theory]
	[InlineAutoData(FileAttributes.Archive)]
	[InlineAutoData(FileAttributes.NoScrubData)]
	[InlineAutoData(FileAttributes.NotContentIndexed)]
	[InlineAutoData(FileAttributes.Offline)]
	[InlineAutoData(FileAttributes.System)]
	[InlineAutoData(FileAttributes.Temporary)]
	public async Task SetAttributes_ShouldOnlyWork_OnWindows(FileAttributes attributes,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);

		if (Test.RunsOnWindows)
		{
			await That(result).IsEqualTo(attributes);
		}
		else
		{
			await That(result).IsEqualTo(FileAttributes.Normal);
		}
	}

	[Theory]
	[AutoData]
	public async Task Attributes_WhenDotFile_ShouldHaveHiddenFlag(bool isFile)
	{
		Skip.If(Test.RunsOnWindows);

		const string path = ".env";

		FileAttributes result;

		if (isFile)
		{
			FileSystem.File.WriteAllText(path, null);

			result = FileSystem.FileInfo.New(path).Attributes;
		}
		else
		{
			FileSystem.Directory.CreateDirectory(path);

			result = FileSystem.DirectoryInfo.New(path).Attributes;
		}

		await That(result).HasFlag(FileAttributes.Hidden);
	}
}
