using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class SetAttributesTests
{
	[Theory]
	[AutoData]
	public void SetAttributes_Directory_ShouldRemainFile(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetAttributes(path, FileAttributes.Directory);

		FileSystem.Directory.Exists(path).Should().BeFalse();
		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.Normal)]
	public void SetAttributes_ShouldNotAdjustTimes(FileAttributes attributes, string path)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, null);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.SetAttributes(path, attributes);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		lastAccessTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
		lastWriteTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
	}
}
