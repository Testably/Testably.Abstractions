using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
#if FEATURE_FILESYSTEM_ASYNC
using System.Threading;
#endif

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionMissingFileTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: (int)MissingFileTestCase.DirectoryMissing)]
	public void Operations_WhenDirectoryIsMissing_ShouldThrowDirectoryNotFoundException(
		Expression<Action<IFile, string>> callback, Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		string path = FileSystem.Path.Combine("missing-directory", "file.txt");

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File, path);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<DirectoryNotFoundException>(
				hResult: -2147024893,
				because: $"\n{callback}\n was called with a missing directory");
		}
		else
		{
			exception.Should()
				.BeFileOrDirectoryNotFoundException(
					$"\n{callback}\n was called with a missing directory");
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: (int)MissingFileTestCase.FileMissing)]
	public void Operations_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		Expression<Action<IFile, string>> callback, Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		string path = "missing-file.txt";

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File, path);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<FileNotFoundException>(
				hResult: -2147024894,
				because: $"\n{callback}\n was called with a missing file");
		}
		else
		{
			exception.Should()
				.BeFileOrDirectoryNotFoundException(
					$"\n{callback}\n was called with a missing file");
		}
	}

	#region Helpers

	public static TheoryData<Expression<Action<IFile, string>>, Func<Test, bool>> GetFileCallbacks(
		int testCases)
	{
		TheoryData<Expression<Action<IFile, string>>, Func<Test, bool>> theoryData = new();
		foreach ((MissingFileTestCase TestCase, ExpectedExceptionType ExceptionType,
			Expression<Action<IFile, string>> Callback, Func<Test, bool>? SkipTest) item in
			GetFileCallbackTestParameters()
				.Where(item => (item.TestCase & (MissingFileTestCase)testCases) != 0))
		{
			theoryData.Add(item.Callback, item.SkipTest ?? (_ => false));
		}

		return theoryData;
	}

	private static IEnumerable<(MissingFileTestCase TestCase, ExpectedExceptionType ExceptionType,
			Expression<Action<IFile, string>> Callback, Func<Test, bool>? SkipTest)>
		GetFileCallbackTestParameters()
	{
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.GetAttributes(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.SetAttributes(path, FileAttributes.ReadOnly),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "destination.txt"),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo"
				}),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo"
				}),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLines(path, new[]
				{
					"foo"
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLinesAsync(path, new[]
					{
						"foo"
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllLinesAsync(path, new[]
						{
							"foo"
						}, Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllText(path, "foo"),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllText(path, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllTextAsync(path, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendAllTextAsync(path, "foo", Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.AppendText(path),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "foo"),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy("foo", path),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy(path, "foo", false),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Copy("foo", path, false),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path, 1023),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Create(path, 1023, FileOptions.None),
			null);
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.CreateSymbolicLink(path, "foo"),
			test => !test.RunsOnWindows);
#endif
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.CreateText(path),
			null);

		#pragma warning disable CA1416
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Decrypt(path),
			test => !test.RunsOnWindows);
		#pragma warning restore CA1416

		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Delete(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.GetAttributes(path),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move(path, "foo"),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move("foo", path),
			null);
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move(path, "foo", false),
			null);
		yield return (MissingFileTestCase.FileMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.Move("foo", path, false),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open, FileAccess.ReadWrite),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None),
			null);
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.Open(path, new FileStreamOptions()),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenRead(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenText(path),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.OpenWrite(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllBytes(path),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllBytesAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLines(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLines(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLinesAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllLinesAsync(path, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllText(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllText(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllTextAsync(path, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadAllTextAsync(path, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLines(path),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLines(path, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_NET7
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLinesAsync(path, CancellationToken.None),
			null);
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.ReadLinesAsync(path, Encoding.UTF8, CancellationToken.None),
			null);
#endif
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.ResolveLinkTarget(path, false),
			null);
#endif
		yield return (MissingFileTestCase.All, ExpectedExceptionType.Default,
			(file, path)
				=> file.SetAttributes(path, FileAttributes.ReadOnly),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetCreationTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetCreationTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastAccessTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastAccessTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastWriteTime(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.All,
			ExpectedExceptionType.Default,
			(file, path)
				=> file.SetLastWriteTimeUtc(path, DateTime.Now),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllBytes(path, new byte[]
				{
					0, 1
				}),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllBytesAsync(path, new byte[]
						{
							0, 1
						},
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLines(path, new[]
				{
					"foo"
				}),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLines(path, new[]
				{
					"foo"
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLinesAsync(path, new[]
					{
						"foo"
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllLinesAsync(path, new[]
						{
							"foo"
						}, Encoding.UTF8,
						CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllText(path, "foo"),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllText(path, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
			(file, path)
				=> file.WriteAllTextAsync(path, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
		yield return (MissingFileTestCase.DirectoryMissing, ExpectedExceptionType.Default,
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
	public enum MissingFileTestCase
	{
		FileMissing = 1,
		DirectoryMissing = 2,
		All = FileMissing | DirectoryMissing
	}
}
