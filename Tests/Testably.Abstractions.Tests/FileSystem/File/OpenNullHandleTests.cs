#if FEATURE_FILE_OPENNULLHANDLE
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class OpenNullHandleTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task OpenNullHandle_FileStream_OnUnix_ShouldSupportSeekingWithEmptyLength()
	{
		Skip.If(Test.RunsOnWindows, "Only /dev/null on Linux and macOS accepts seeking.");

		using FileSystemStream stream = FileSystem.FileStream.New(
			FileSystem.File.OpenNullHandle(), FileAccess.ReadWrite);

		await That(stream.CanSeek).IsTrue();
		await That(stream.Length).IsEqualTo(0);
		await That(stream.Position).IsEqualTo(0);
	}

	[Test]
	public async Task OpenNullHandle_FileStream_OnWindows_ShouldNotSupportSeeking()
	{
		Skip.IfNot(Test.RunsOnWindows, "Only /dev/null on Linux and macOS accepts seeking.");

		using FileSystemStream stream = FileSystem.FileStream.New(
			FileSystem.File.OpenNullHandle(), FileAccess.ReadWrite);

		void ReadLength()
		{
			_ = stream.Length;
		}

		void ReadPosition()
		{
			_ = stream.Position;
		}

		void Seek()
		{
			stream.Seek(0, SeekOrigin.Begin);
		}

		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsTrue();
		await That(stream.CanSeek).IsFalse();
		await That(ReadLength).Throws<NotSupportedException>();
		await That(ReadPosition).Throws<NotSupportedException>();
		await That(Seek).Throws<NotSupportedException>();
	}

	[Test]
	[AutoArguments]
	public async Task OpenNullHandle_FileStream_ShouldDiscardWrittenBytes(byte[] bytes)
	{
		using FileSystemStream stream = FileSystem.FileStream.New(
			FileSystem.File.OpenNullHandle(), FileAccess.ReadWrite);

		stream.Write(bytes, 0, bytes.Length);
		stream.Flush();
		int result = stream.Read(new byte[bytes.Length], 0, bytes.Length);

		await That(result).IsEqualTo(0);
	}

	[Test]
	public async Task OpenNullHandle_GetLength_OnUnix_ShouldReturnZero()
	{
		Skip.If(Test.RunsOnWindows, "Only /dev/null on Linux and macOS accepts seeking.");

		using SafeFileHandle handle = FileSystem.File.OpenNullHandle();

		long result = FileSystem.RandomAccess.GetLength(handle);

		await That(result).IsEqualTo(0);
	}

	[Test]
	public async Task OpenNullHandle_GetLength_OnWindows_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(Test.RunsOnWindows, "Only /dev/null on Linux and macOS accepts seeking.");

		using SafeFileHandle handle = FileSystem.File.OpenNullHandle();

		void Act()
		{
			FileSystem.RandomAccess.GetLength(handle);
		}

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task
		OpenNullHandle_SetLength_NegativeLength_OnWindows_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(Test.RunsOnWindows, "Only /dev/null on Linux and macOS accepts seeking.");

		using SafeFileHandle handle = FileSystem.File.OpenNullHandle();

		void Act()
		{
			FileSystem.RandomAccess.SetLength(handle, -1);
		}

		await That(Act).Throws<NotSupportedException>()
			.Because("an unseekable handle is rejected before the length is validated");
	}

	[Test]
	[AutoArguments]
	public async Task OpenNullHandle_Read_ShouldReturnNoData(byte[] bytes)
	{
		using SafeFileHandle handle = FileSystem.File.OpenNullHandle();

		FileSystem.RandomAccess.Write(handle, bytes, 0);
		int result = FileSystem.RandomAccess.Read(handle, new byte[bytes.Length], 0);

		await That(result).IsEqualTo(0)
			.Because("the null device discards everything written to it");
	}

	[Test]
	public async Task OpenNullHandle_ShouldReturnOpenHandle()
	{
		using SafeFileHandle handle1 = FileSystem.File.OpenNullHandle();
		using SafeFileHandle handle2 = FileSystem.File.OpenNullHandle();

		await That(handle1.IsInvalid).IsFalse();
		await That(handle1.IsClosed).IsFalse();
		await That(handle1).IsNotSameAs(handle2);
	}
}
#endif
