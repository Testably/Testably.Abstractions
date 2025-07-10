using NSubstitute.ExceptionExtensions;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class FileAccessTests
{
	[Theory]
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
	public async Task FileAccess_ConcurrentAccessWithInvalidScenarios_ShouldThrowIOException(
		FileAccess access1, FileShare share1,
		FileAccess access2, FileShare share2,
		string path, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		FileSystem.File.WriteAllText(path, contents);

		void Act()
		{
			_ = FileSystem.FileStream.New(path, FileMode.Open,
				access1, share1);
			_ = FileSystem.FileStream.New(path, FileMode.Open,
				access2, share2);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024864)
				.Because($"Access {access1}, Share {share1} of file 1 is incompatible with Access {access2}, Share {share2} of file 2");
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Theory]
	[InlineAutoData(FileAccess.Read, FileShare.Read, FileAccess.Read, FileShare.Read)]
	[InlineAutoData(FileAccess.Read, FileShare.ReadWrite, FileAccess.ReadWrite,
		FileShare.Read)]
	public async Task FileAccess_ConcurrentReadAccessWithValidScenarios_ShouldNotThrowException(
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

		await That(result1).IsEqualTo(contents);
		await That(result2).IsEqualTo(contents);
	}

	[Theory]
	[InlineAutoData(FileAccess.Write, FileShare.Write, FileAccess.Write, FileShare.Write)]
	[InlineAutoData(FileAccess.ReadWrite, FileShare.ReadWrite, FileAccess.ReadWrite,
		FileShare.ReadWrite)]
	public async Task FileAccess_ConcurrentWriteAccessWithValidScenarios_ShouldNotThrowException(
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

		await That(result).IsEqualTo(contents2);
	}

	[Theory]
	[AutoData]
	public async Task FileAccess_ReadAfterFirstAppend_ShouldContainBothContents(
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
		await That(result).IsEqualTo(contents1 + contents2);
	}

	[Theory]
	[AutoData]
	public async Task FileAccess_ReadBeforeFirstAppend_ShouldOnlyContainSecondContent(
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

		await That(result).IsEqualTo(contents2);
	}

	[Theory]
	[AutoData]
	public async Task FileAccess_ReadWhileWriteLockActive_ShouldThrowIOException(
		string path, string contents)
	{
		using FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Create);

		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		stream.Write(bytes, 0, bytes.Length);

		void Act()
		{
			FileSystem.File.ReadAllText(path);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
				.WithHResult(-2147024864);
		}
		else
		{
			await That(Act).DoesNotThrow();
		}
	}

	[Theory]
	[AutoData]
	public async Task MultipleParallelReads_ShouldBeAllowed(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		ConcurrentBag<string> results = [];

		ParallelLoopResult wait = Parallel.For(0, 100, _ =>
		{
			FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open,
				FileAccess.Read, FileShare.Read);
			using StreamReader sr = new(stream, Encoding.UTF8);
			results.Add(sr.ReadToEnd());
		});

		while (!wait.IsCompleted)
		{
			await Task.Delay(10, TestContext.Current.CancellationToken);
		}

		await That(results).HasCount(100);
		await That(results).All().AreEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task Read_ShouldCreateValidFileStream(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents, Encoding.UTF8);
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.Open);
		using StreamReader sr = new(stream, Encoding.UTF8);
		string result = sr.ReadToEnd();

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task Write_ShouldCreateValidFileStream(string path, string contents)
	{
		FileSystemStream stream = FileSystem.FileStream.New(path, FileMode.CreateNew);

		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		stream.Write(bytes, 0, bytes.Length);

		stream.Dispose();

		string result = FileSystem.File.ReadAllText(path);

		await That(result).IsEqualTo(contents);
	}
}
