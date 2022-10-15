using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileSystemFileSystemInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileSystemInfoTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
	[InlineAutoData(FileAttributes.Archive)]
	[InlineAutoData(FileAttributes.NoScrubData)]
	[InlineAutoData(FileAttributes.NotContentIndexed)]
	[InlineAutoData(FileAttributes.Offline)]
	[InlineAutoData(FileAttributes.System)]
	[InlineAutoData(FileAttributes.Temporary)]
	public void SetAttributes_ShouldOnlyWorkOnWindows(FileAttributes attributes,
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