using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileAccessTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineAutoData(FileAccess.Read, FileShare.Read,
		FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.Read,
		FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite,
		FileAccess.ReadWrite, FileShare.Read)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite,
		FileAccess.ReadWrite, FileShare.Write)]
	[InlineAutoData(FileAccess.Read, FileShare.Read,
		FileAccess.ReadWrite, FileShare.ReadWrite)]
	public void FileAccess_ConcurrentAccessWithInvalidScenarios_ShouldThrowIOException(
		FileAccess access1, FileShare share1,
		FileAccess access2, FileShare share2,
		string path, string contents)
	{
		Skip.If(LongRunningTestsShouldBeSkipped());

		FileSystem.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.FileStream.New(path, FileMode.Open,
				access1, share1);
			_ = FileSystem.FileStream.New(path, FileMode.Open,
				access2, share2);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(
				$"'{FileSystem.Path.GetFullPath(path)}'",
				hResult: -2147024864,
				because:
				$"Access {access1}, Share {share1} of file 1 is incompatible with Access {access2}, Share {share2} of file 2");
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[SkippableTheory]
	[InlineAutoData(FileAccess.Read, FileShare.Read, FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite, FileAccess.ReadWrite,
		FileShare.Read)]
	public void FileAccess_ConcurrentReadAccessWithValidScenarios_ShouldNotThrowException(
		FileAccess access1, FileShare share1,
		FileAccess access2, FileShare share2,
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Open,
			access1, share1);
		FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Open,
			access2, share2);

		using StreamReader sr1 = new(stream1, Encoding.UTF8);
		using StreamReader sr2 = new(stream2, Encoding.UTF8);
		string result1 = sr1.ReadToEnd();
		string result2 = sr2.ReadToEnd();

		result1.Should().Be(contents);
		result2.Should().Be(contents);
	}

	[SkippableTheory]
	[InlineAutoData(FileAccess.Write, FileShare.Write, FileAccess.Write, FileShare.Write)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite, FileAccess.ReadWrite,
		FileShare.ReadWrite)]
	public void
		FileAccess_ConcurrentWriteAccessWithValidScenarios_ShouldNotThrowException(
			FileAccess access1, FileShare share1,
			FileAccess access2, FileShare share2,
			string path, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Open,
			access1, share1);
		FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Open,
			access2, share2);

		byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
		stream1.Write(bytes1, 0, bytes1.Length);
		stream1.Flush();
		byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
		stream2.Write(bytes2, 0, bytes2.Length);
		stream2.Flush();

		stream1.Dispose();
		stream2.Dispose();
		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents2);
	}

	[SkippableTheory]
	[AutoData]
	public void FileAccess_ReadAfterFirstAppend_ShouldContainBothContents(
		string path, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Append,
			FileAccess.Write, FileShare.Write);

		byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
		stream1.Write(bytes1, 0, bytes1.Length);
		stream1.Flush();

		FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Append,
			FileAccess.Write, FileShare.Write);
		byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
		stream2.Write(bytes2, 0, bytes2.Length);
		stream2.Flush();

		stream1.Dispose();
		stream2.Dispose();
		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents1 + contents2);
	}

	[SkippableTheory]
	[AutoData]
	public void FileAccess_ReadBeforeFirstAppend_ShouldOnlyContainSecondContent(
		string path, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystemStream stream1 = FileSystem.FileStream.New(path, FileMode.Append,
			FileAccess.Write, FileShare.Write);
		FileSystemStream stream2 = FileSystem.FileStream.New(path, FileMode.Append,
			FileAccess.Write, FileShare.Write);

		byte[] bytes1 = Encoding.UTF8.GetBytes(contents1);
		stream1.Write(bytes1, 0, bytes1.Length);
		stream1.Flush();
		byte[] bytes2 = Encoding.UTF8.GetBytes(contents2);
		stream2.Write(bytes2, 0, bytes2.Length);
		stream2.Flush();

		stream1.Dispose();
		stream2.Dispose();
		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents2);
	}

	[SkippableTheory]
	[AutoData]
	public void FileAccess_ReadWhileWriteLockActive_ShouldThrowIOException(
		string path, string contents)
	{
		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);

		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		stream.Write(bytes, 0, bytes.Length);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(
				$"'{FileSystem.Path.GetFullPath(path)}'",
				hResult: -2147024864);
		}
		else
		{
			exception.Should().BeNull();
		}
	}

	[SkippableTheory]
	[AutoData]
	public async Task MultipleParallelReads_ShouldBeAllowed(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		ConcurrentBag<string> results = new();

		ParallelLoopResult wait = Parallel.For(0, 100, _ =>
		{
			FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
				FileAccess.Read, FileShare.Read);
			using StreamReader sr = new(stream, Encoding.UTF8);
			results.Add(sr.ReadToEnd());
		});

		while (!wait.IsCompleted)
		{
			await Task.Delay(10);
		}

		results.Should().HaveCount(100);
		results.Should().AllBeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Read_ShouldCreateValidFileStream(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents, Encoding.UTF8);
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open);
		using StreamReader sr = new(stream, Encoding.UTF8);
		string result = sr.ReadToEnd();

		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void Write_ShouldCreateValidFileStream(string path, string contents)
	{
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.CreateNew);

		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		stream.Write(bytes, 0, bytes.Length);

		stream.Dispose();

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents);
	}
}
