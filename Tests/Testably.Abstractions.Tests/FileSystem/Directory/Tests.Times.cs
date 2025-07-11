using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task GetCreationTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetCreationTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetCreationTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetCreationTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastAccessTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetLastAccessTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastAccessTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastWriteTime_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToLocalTime();

		DateTime result = FileSystem.Directory.GetLastWriteTime(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task GetLastWriteTimeUtc_PathNotFound_ShouldReturnNullTime(string path)
	{
		DateTime expectedTime = FileTestHelper.NullTime.ToUniversalTime();

		DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);

		await That(result).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task LastAccessTime_CreateSubDirectory_ShouldUpdateLastAccessAndLastWriteTime(
		string path, string subPath)
	{
		SkipIfBrittleTestsShouldBeSkipped();

		DateTime start = TimeSystem.DateTime.Now;
		IDirectoryInfo result = FileSystem.Directory.CreateDirectory(path);
		TimeSystem.Thread.Sleep(100);
		DateTime sleepTime = TimeSystem.DateTime.Now;
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subPath));

		await That(result.CreationTime).IsOnOrAfter(start.ApplySystemClockTolerance());
		await That(result.CreationTime).IsBefore(sleepTime);
		// Last Access Time is only updated on Windows
		if (Test.RunsOnWindows)
		{
			await That(result.LastAccessTime).IsBetween(sleepTime).And(TimeSystem.DateTime.Now)
				.Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(result.LastAccessTime).IsBetween(start).And(sleepTime)
				.Within(TimeComparison.Tolerance);
		}

		await That(result.LastWriteTime).IsBetween(sleepTime).And(TimeSystem.DateTime.Now)
			.Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task LastAccessTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastAccessTime(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.Now)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public async Task LastAccessTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastAccessTimeUtc(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.UtcNow)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Theory]
	[AutoData]
	public async Task LastWriteTime_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.Now;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastWriteTime(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.Now)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
	}

	[Theory]
	[AutoData]
	public async Task LastWriteTimeUtc_ShouldBeSet(string path)
	{
		DateTime start = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.CreateDirectory(path);

		DateTime result = FileSystem.Directory.GetLastWriteTimeUtc(path);
		await That(result).IsBetween(start).And(TimeSystem.DateTime.UtcNow)
			.Within(TimeComparison.Tolerance);
		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime creationTime)
	{
		void Act()
		{
			FileSystem.Directory.SetCreationTime(path, creationTime);
		}

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
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
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTime(path, creationTime);

		await That(FileSystem.Directory.GetCreationTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTime_Unspecified_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = DateTime.SpecifyKind(creationTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTime(path, creationTime);

		await That(FileSystem.Directory.GetCreationTimeUtc(path))
			.IsEqualTo(creationTime.ToUniversalTime());
		await That(FileSystem.Directory.GetCreationTime(path)).IsEqualTo(creationTime);
		await That(FileSystem.Directory.GetCreationTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime creationTime)
	{
		void Act()
		{
			FileSystem.Directory.SetCreationTimeUtc(path, creationTime);
		}

		if (Test.RunsOnWindows || (Test.IsNet8OrGreater && !Test.RunsOnMac))
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
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
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTimeUtc(path, creationTime);

		await That(FileSystem.Directory.GetCreationTime(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetCreationTimeUtc_Unspecified_ShouldChangeCreationTime(
		string path, DateTime creationTime)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Linux does not have a creation timestamp: https://unix.stackexchange.com/a/102692");

		creationTime = DateTime.SpecifyKind(creationTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetCreationTimeUtc(path, creationTime);

		await That(FileSystem.Directory.GetCreationTimeUtc(path)).IsEqualTo(creationTime);
		await That(FileSystem.Directory.GetCreationTime(path))
			.IsEqualTo(creationTime.ToLocalTime());
		await That(FileSystem.Directory.GetCreationTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		void Act()
		{
			FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTime_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToLocalTime();
		DateTime expectedTime = lastAccessTime.ToUniversalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);

		await That(FileSystem.Directory.GetLastAccessTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTime_Unspecified_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = DateTime.SpecifyKind(lastAccessTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTime(path, lastAccessTime);

		await That(FileSystem.Directory.GetLastAccessTimeUtc(path))
			.IsEqualTo(lastAccessTime.ToUniversalTime());
		await That(FileSystem.Directory.GetLastAccessTime(path)).IsEqualTo(lastAccessTime);
		await That(FileSystem.Directory.GetLastAccessTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastAccessTime)
	{
		void Act()
		{
			FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTimeUtc_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = lastAccessTime.ToUniversalTime();
		DateTime expectedTime = lastAccessTime.ToLocalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);

		await That(FileSystem.Directory.GetLastAccessTime(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastAccessTimeUtc_Unspecified_ShouldChangeLastAccessTime(
		string path, DateTime lastAccessTime)
	{
		lastAccessTime = DateTime.SpecifyKind(lastAccessTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastAccessTimeUtc(path, lastAccessTime);

		await That(FileSystem.Directory.GetLastAccessTimeUtc(path)).IsEqualTo(lastAccessTime);
		await That(FileSystem.Directory.GetLastAccessTime(path))
			.IsEqualTo(lastAccessTime.ToLocalTime());
		await That(FileSystem.Directory.GetLastAccessTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTime_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		void Act()
		{
			FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTime_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToLocalTime();
		DateTime expectedTime = lastWriteTime.ToUniversalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);

		await That(FileSystem.Directory.GetLastWriteTimeUtc(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTime_Unspecified_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTime(path, lastWriteTime);

		await That(FileSystem.Directory.GetLastWriteTimeUtc(path))
			.IsEqualTo(lastWriteTime.ToUniversalTime());
		await That(FileSystem.Directory.GetLastWriteTime(path)).IsEqualTo(lastWriteTime);
		await That(FileSystem.Directory.GetLastWriteTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTimeUtc_PathNotFound_ShouldThrowCorrectException(
		string path, DateTime lastWriteTime)
	{
		void Act()
		{
			FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);
		}

		if (Test.RunsOnWindows || Test.IsNet8OrGreater)
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024894);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024893);
		}
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTimeUtc_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = lastWriteTime.ToUniversalTime();
		DateTime expectedTime = lastWriteTime.ToLocalTime();
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);

		await That(FileSystem.Directory.GetLastWriteTime(path)).IsEqualTo(expectedTime);
	}

	[Theory]
	[AutoData]
	public async Task SetLastWriteTimeUtc_Unspecified_ShouldChangeLastWriteTime(
		string path, DateTime lastWriteTime)
	{
		lastWriteTime = DateTime.SpecifyKind(lastWriteTime, DateTimeKind.Unspecified);
		FileSystem.Directory.CreateDirectory(path);

		FileSystem.Directory.SetLastWriteTimeUtc(path, lastWriteTime);

		await That(FileSystem.Directory.GetLastWriteTimeUtc(path)).IsEqualTo(lastWriteTime);
		await That(FileSystem.Directory.GetLastWriteTime(path))
			.IsEqualTo(lastWriteTime.ToLocalTime());
		await That(FileSystem.Directory.GetLastWriteTime(path).Kind)
			.IsNotEqualTo(DateTimeKind.Unspecified);
	}
}
