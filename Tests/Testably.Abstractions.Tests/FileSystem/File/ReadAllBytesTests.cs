using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllBytesTests
{
	[Theory]
	[AutoData]
	public void ReadAllBytes_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllBytes(path);
		});

		exception.Should()
			.BeOfType<FileNotFoundException>()
			.Which.HResult.Should()
			.Be(-2147024894);
		exception.Should()
			.BeOfType<FileNotFoundException>()
			.Which.Message.Should()
			.Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[Theory]
	[AutoData]
	public void ReadAllBytes_ShouldAdjustTimes(string path, byte[] bytes)
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
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}
		else
		{
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		lastWriteTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
	}

	[Theory]
	[AutoData]
	public void ReadAllBytes_ShouldNotGetAReferenceToFileContent(
		string path, byte[] bytes)
	{
		FileSystem.File.WriteAllBytes(path, bytes.ToArray());

		byte[] results = FileSystem.File.ReadAllBytes(path);
		results[0] = (byte)~results[0];

		byte[] result = FileSystem.File.ReadAllBytes(path);

		result.Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void ReadAllBytes_ShouldReturnWrittenBytes(
		byte[] bytes, string path)
	{
		FileSystem.File.WriteAllBytes(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);

		result.Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void ReadAllBytes_ShouldTolerateAltDirectorySeparatorChar(
		byte[] bytes, string directory, string fileName)
	{
		FileSystem.Directory.CreateDirectory(directory);
		string filePath = $"{directory}{FileSystem.Path.DirectorySeparatorChar}{fileName}";
		string altFilePath = $"{directory}{FileSystem.Path.AltDirectorySeparatorChar}{fileName}";
		FileSystem.File.WriteAllBytes(filePath, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(altFilePath);

		result.Should().BeEquivalentTo(bytes);
	}
}
