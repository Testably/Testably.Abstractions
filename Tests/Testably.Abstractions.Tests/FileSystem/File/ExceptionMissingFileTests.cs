using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ExceptionMissingFileTests
{
	[Theory]
	[MemberData(nameof(GetFileCallbacks), (int)MissingFileTestCases.DirectoryMissing)]
	public async Task Operations_WhenDirectoryIsMissing_ShouldThrowDirectoryNotFoundException(
		Expression<Action<IFile, string>> callback, Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		string path = FileSystem.Path.Combine("missing-directory", "file.txt");

		void Act()
		{
			callback.Compile().Invoke(FileSystem.File, path);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithHResult(-2147024893)
				.Because($"\n{callback}\n was called with a missing directory");
		}
		else
		{
			await That(Act).ThrowsAFileOrDirectoryNotFoundException()
				.Because($"\n{callback}\n was called with a missing directory");
		}
	}

	[Theory]
	[MemberData(nameof(GetFileCallbacks), (int)MissingFileTestCases.FileMissing)]
	public async Task Operations_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		Expression<Action<IFile, string>> callback, Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		string path = "missing-file.txt";

		void Act()
		{
			callback.Compile().Invoke(FileSystem.File, path);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithHResult(-2147024894)
				.Because($"\n{callback}\n was called with a missing file");
		}
		else
		{
			await That(Act).ThrowsAFileOrDirectoryNotFoundException()
				.Because($"\n{callback}\n was called with a missing file");
		}
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFile, string>>, Func<Test, bool>> GetFileCallbacks(
		int testCases)
	{
		TheoryData<Expression<Action<IFile, string>>, Func<Test, bool>> theoryData = new();
		foreach ((MissingFileTestCases TestCase, ExpectedExceptionType ExceptionType,
			Expression<Action<IFile, string>> Callback, Func<Test, bool>? SkipTest) item in
			GetFileCallbackTestParameters()
				.Where(item => (item.TestCase & (MissingFileTestCases)testCases) != 0))
		{
			theoryData.Add(item.Callback, item.SkipTest ?? (_ => false));
		}

		return theoryData;
	}
	#pragma warning restore MA0018

	private static IEnumerable<(MissingFileTestCases TestCase, ExpectedExceptionType ExceptionType,
			Expression<Action<IFile, string>> Callback, Func<Test, bool>? SkipTest)>
		GetFileCallbackTestParameters()
	{
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.GetAttributes(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.SetAttributes(path, FileAttributes.ReadOnly),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "destination.txt"),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo",
				}),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo",
				}),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo",
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLinesAsync(path, new[]
					{
						"foo",
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLinesAsync(path, new[]
						{
							"foo",
						}, Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllText(path, "foo"),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllText(path, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllTextAsync(path, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllTextAsync(path, "foo", Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendText(path),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "foo"),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy("foo", path),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "foo", false),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy("foo", path, false),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path, 1023),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path, 1023, FileOptions.None),
			null);
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.CreateSymbolicLink(path, "foo"),
			test => !test.RunsOnWindows);
#endif
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.CreateText(path),
			null);

		#pragma warning disable CA1416
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Decrypt(path),
			test => !test.RunsOnWindows);
		#pragma warning restore CA1416

		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Delete(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.GetAttributes(path),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move(path, "foo"),
			null);
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move("foo", path),
			null);
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move(path, "foo", false),
			null);
#endif
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (MissingFileTestCases.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move("foo", path, false),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open, FileAccess.ReadWrite),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None),
			null);
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, new FileStreamOptions()),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenRead(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenText(path),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenWrite(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllBytes(path),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllBytesAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLines(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLines(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLinesAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLinesAsync(path, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllText(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllText(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllTextAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllTextAsync(path, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLines(path),
			null);
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLines(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLinesAsync(path, CancellationToken.None),
			null);
#endif
#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLinesAsync(path, Encoding.UTF8, CancellationToken.None),
			null);
#endif
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.ResolveLinkTarget(path, false),
			null);
#endif
		yield return (MissingFileTestCases.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.SetAttributes(path, FileAttributes.ReadOnly),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetCreationTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetCreationTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastAccessTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastAccessTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastWriteTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastWriteTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllBytes(path, new byte[]
				{
					0, 1,
				}),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllBytesAsync(path, new byte[]
						{
							0, 1,
						},
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLines(path, new[]
				{
					"foo",
				}),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLines(path, new[]
				{
					"foo",
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLinesAsync(path, new[]
					{
						"foo",
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLinesAsync(path, new[]
						{
							"foo",
						}, Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllText(path, "foo"),
			null);
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllText(path, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllTextAsync(path, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCases.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllTextAsync(path, "foo", Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
	}

	#endregion

	public enum ExpectedExceptionType
	{
		Default,
	}

	[Flags]
	public enum MissingFileTestCases
	{
		FileMissing = 1,
		DirectoryMissing = 2,
		All = FileMissing | DirectoryMissing,
	}
}
