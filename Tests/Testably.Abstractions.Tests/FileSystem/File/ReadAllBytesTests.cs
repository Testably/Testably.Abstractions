using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllBytesTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllBytes_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		void Act()
		{
			FileSystem.File.ReadAllBytes(path);
		}

		await That(Act).ThrowsExactly<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessage($"*'{FileSystem.Path.GetFullPath(path)}'*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytes_ShouldAdjustTimes(string path, byte[] bytes)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllBytes(path, bytes);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		_ = FileSystem.File.ReadAllBytes(path);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytes_ShouldNotGetAReferenceToFileContent(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes.ToArray());

		byte[] results = FileSystem.File.ReadAllBytes(path);
		results[0] = (byte)~results[0];

		byte[] result = FileSystem.File.ReadAllBytes(path);

		await That(result).IsEqualTo(bytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytes_ShouldReturnWrittenBytes(
		byte[] bytes, string path)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);

		await That(result).IsEqualTo(bytes).InAnyOrder();
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytes_ShouldTolerateAltDirectorySeparatorChar(
		byte[] bytes, string directory, string fileName)
	{
		FileSystem.Directory.CreateDirectory(directory);
		string filePath = $"{directory}{FileSystem.Path.DirectorySeparatorChar}{fileName}";
		string altFilePath = $"{directory}{FileSystem.Path.AltDirectorySeparatorChar}{fileName}";
		FileSystem.File.WriteAllBytes(filePath, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(altFilePath);

		await That(result).IsEqualTo(bytes).InAnyOrder();
	}
}
