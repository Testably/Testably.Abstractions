using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class RefreshTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private static readonly DateTime OtherTime = new(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	[Test]
	[AutoArguments]
	public async Task Attributes_ShouldReturnCachedValueUntilRefresh(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Attributes).DoesNotHaveFlag(FileAttributes.ReadOnly);

		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);

		await That(sut.Attributes).DoesNotHaveFlag(FileAttributes.ReadOnly);
		await That(sut.IsReadOnly).IsFalse();
		sut.Refresh();
		await That(sut.Attributes).HasFlag(FileAttributes.ReadOnly);
		await That(sut.IsReadOnly).IsTrue();
		FileSystem.File.SetAttributes(path, FileAttributes.Normal);
	}

	[Test]
	[AutoArguments]
	public async Task Attributes_Set_ShouldResetCachedState(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(3);

		sut.Attributes = FileAttributes.Normal;
		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(10);
	}

	[Test]
	[AutoArguments]
	public async Task CreationTime_Set_ShouldResetCachedState(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(3);

		sut.CreationTimeUtc = OtherTime;
		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(10);
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldReturnCachedValueFromOtherProperty(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(3);

		FileSystem.File.Delete(path);

		await That(sut.Exists).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task IsReadOnly_Set_ShouldResetCachedState(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.IsReadOnly).IsFalse();

		sut.IsReadOnly = true;

		await That(sut.IsReadOnly).IsTrue();
		sut.IsReadOnly = false;
		await That(sut.IsReadOnly).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task LastAccessTime_Set_ShouldResetCachedState(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(3);

		sut.LastAccessTimeUtc = OtherTime;
		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(10);
	}

	[Test]
	[AutoArguments]
	public async Task LastWriteTime_Set_ShouldResetCachedState(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.LastWriteTimeUtc).IsNotEqualTo(OtherTime);

		sut.LastWriteTimeUtc = OtherTime;

		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
		await That(sut.LastWriteTime).IsEqualTo(OtherTime.ToLocalTime());
	}

	[Test]
	[AutoArguments]
	public async Task Length_Missing_ShouldReturnCachedStateUntilRefresh(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsFalse();

		FileSystem.File.WriteAllText(path, "abc");

		void Act()
		{
			_ = sut.Length;
		}

		await That(Act).Throws<FileNotFoundException>();
		sut.Refresh();
		await That(sut.Length).IsEqualTo(3);
	}

	[Test]
	[AutoArguments]
	public async Task Length_ShouldReturnCachedValueFromExists(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsTrue();

		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(3);
	}

	[Test]
	[AutoArguments]
	public async Task Length_ShouldReturnCachedValueUntilRefresh(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Length).IsEqualTo(3);

		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(3);
		sut.Refresh();
		await That(sut.Length).IsEqualTo(10);
	}

	[Test]
	[AutoArguments]
	public async Task Refresh_ShouldCaptureStateImmediately(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Refresh();
		FileSystem.File.WriteAllText(path, "abcdefghij");

		await That(sut.Length).IsEqualTo(3);
		FileSystem.File.Delete(path);
		await That(sut.Exists).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task Times_ShouldReturnCachedValuesUntilRefresh(string path)
	{
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsTrue();

		FileSystem.File.SetCreationTimeUtc(path, OtherTime);
		FileSystem.File.SetLastAccessTimeUtc(path, OtherTime);
		FileSystem.File.SetLastWriteTimeUtc(path, OtherTime);

		await That(sut.CreationTimeUtc).IsNotEqualTo(OtherTime);
		await That(sut.CreationTime).IsNotEqualTo(OtherTime.ToLocalTime());
		await That(sut.LastAccessTimeUtc).IsNotEqualTo(OtherTime);
		await That(sut.LastAccessTime).IsNotEqualTo(OtherTime.ToLocalTime());
		await That(sut.LastWriteTimeUtc).IsNotEqualTo(OtherTime);
		await That(sut.LastWriteTime).IsNotEqualTo(OtherTime.ToLocalTime());
		sut.Refresh();
		await That(sut.LastAccessTimeUtc).IsEqualTo(OtherTime);
		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	[Test]
	[AutoArguments]
	public async Task UnixFileMode_ShouldReturnCachedValueUntilRefresh(string path)
	{
		Skip.If(Test.RunsOnWindows);

		UnixFileMode unixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsTrue();

		#pragma warning disable CA1416
		FileSystem.File.SetUnixFileMode(path, unixFileMode);
		#pragma warning restore CA1416

		await That(sut.UnixFileMode).IsNotEqualTo(unixFileMode);
		sut.Refresh();
		await That(sut.UnixFileMode).IsEqualTo(unixFileMode);
	}

	[Test]
	[AutoArguments]
	public async Task UnixFileMode_Set_ShouldResetCachedState(string path)
	{
		Skip.If(Test.RunsOnWindows);

		UnixFileMode unixFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
		FileSystem.File.WriteAllText(path, "abc");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.UnixFileMode).IsNotEqualTo(unixFileMode);

		#pragma warning disable CA1416
		sut.UnixFileMode = unixFileMode;
		#pragma warning restore CA1416

		await That(sut.UnixFileMode).IsEqualTo(unixFileMode);
	}
#endif
}
