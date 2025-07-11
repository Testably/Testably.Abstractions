namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetCreationTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetCreationTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetLastAccessTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.File.GetLastWriteTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task LastAccessTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastAccessTime(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.Now)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public async Task LastAccessTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastAccessTimeUtc(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.UtcNow)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Theory]
	[AutoData]
	public async Task LastWriteTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastWriteTime(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.Now)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public async Task LastWriteTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, null);

		DateTime result = FileSystem.File.GetLastWriteTimeUtc(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.UtcNow)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTime_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToLocalTime();
		DateTime expectedTime = creationTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetCreationTime(path, creationTime);

		await That(FileSystem.File.GetCreationTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTimeUtc_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = creationTime.ToUniversalTime();
		DateTime expectedTime = creationTime.ToLocalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetCreationTimeUtc(path, creationTime);

		await That(FileSystem.File.GetCreationTime(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTime_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToLocalTime();
		DateTime expectedTime = lastAccessTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastAccessTime(path, lastAccessTime);

		await That(FileSystem.File.GetLastAccessTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToUniversalTime();
		DateTime expectedTime = lastAccessTime.ToLocalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastAccessTimeUtc(path, lastAccessTime);

		await That(FileSystem.File.GetLastAccessTime(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTime_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToLocalTime();
		DateTime expectedTime = lastWriteTime.ToUniversalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastWriteTime(path, lastWriteTime);

		await That(FileSystem.File.GetLastWriteTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToUniversalTime();
		DateTime expectedTime = lastWriteTime.ToLocalTime();
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);

		await That(FileSystem.File.GetLastWriteTime(path)).IsEqualTo(expectedTime);
	}
}
