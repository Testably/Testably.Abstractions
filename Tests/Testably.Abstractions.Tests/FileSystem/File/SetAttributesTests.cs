using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class SetAttributesTests
{
	[Theory]
	[AutoData]
	public async Task SetAttributes_Directory_ShouldRemainFile(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetAttributes(path, FileAttributes.Directory);

		await That(FileSystem.Directory.Exists(path)).IsFalse();
		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.Normal)]
	public async Task SetAttributes_ShouldNotAdjustTimes(FileAttributes attributes, string path)
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd)
				.Within(TimeComparison.Tolerance);
		}

		await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd)
			.Within(TimeComparison.Tolerance);
		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd)
			.Within(TimeComparison.Tolerance);
	}
}
