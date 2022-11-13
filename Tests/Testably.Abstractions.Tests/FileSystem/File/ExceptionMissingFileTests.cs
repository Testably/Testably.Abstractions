using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionMissingFileTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: (int)MissingFileTestCase.FileMissing)]
	public void Operations_WhenFileIsMissing_ShouldThrowFileNotFoundException(
		Expression<Action<IFile, string>> callback)
	{
		string path = "missing-file.txt";

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File, path);
		});

		exception.Should().BeOfType<FileNotFoundException>(
				$"\n{callback}\n was called with a missing file.")
			.Which.HResult.Should().Be(-2147024894,
				$"\n{callback}\n was called with a missing file.");
	}

	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: (int)MissingFileTestCase.DirectoryMissing)]
	public void Operations_WhenDirectoryIsMissing_ShouldThrowDirectoryNotFoundException(
		Expression<Action<IFile, string>> callback)
	{
		string path = FileSystem.Path.Combine("missing-directory", "file.txt");

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File, path);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>(
				$"\n{callback}\n was called with a missing file.")
			.Which.HResult.Should().Be(-2147024893,
				$"\n{callback}\n was called with a missing file.");
	}

	#region Helpers

	[Flags]
	public enum MissingFileTestCase
	{
		FileMissing = 1,
		DirectoryMissing = 2,
		All = FileMissing | DirectoryMissing
	}

	public static IEnumerable<object?[]> GetFileCallbacks(int testCases)
		=> GetFileCallbackTestParameters()
			.Where(item => (item.TestCase & (MissingFileTestCase)testCases) != 0)
			.Select(item => new object?[]
			{
				item.Callback
			});

	private static IEnumerable<(MissingFileTestCase TestCase, Expression<Action<IFile, string>> Callback)>
		GetFileCallbackTestParameters()
	{
		yield return (MissingFileTestCase.All, (file, path)
			=> file.GetAttributes(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetAttributes(path, FileAttributes.ReadOnly));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.Copy(path, "destination.txt"));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllLines(path, new[]
			{
						"foo"
			}));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllLines(path, new[]
			{
				"foo"
			}));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllLines(path, new[]
			{
				"foo"
			}, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllLinesAsync(path, new[]
				{
					"foo"
				}, CancellationToken.None)
				.GetAwaiter().GetResult());
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllLinesAsync(path, new[]
				{
					"foo"
				}, Encoding.UTF8,
				CancellationToken.None).GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllText(path, "foo"));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllText(path, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllTextAsync(path, "foo", CancellationToken.None).GetAwaiter()
				.GetResult());
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendAllTextAsync(path, "foo", Encoding.UTF8,
				CancellationToken.None).GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.AppendText(path));
		yield return (MissingFileTestCase.FileMissing, (file, path)
				=> file.Copy(path, "foo"));
		yield return (MissingFileTestCase.FileMissing, (file, path)
				=> file.Copy("foo", path));
		yield return (MissingFileTestCase.FileMissing, (file, path)
				=> file.Copy(path, "foo", false));
		yield return (MissingFileTestCase.FileMissing, (file, path)
				=> file.Copy("foo", path, false));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.Create(path));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.Create(path, 1023));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.Create(path, 1023, FileOptions.None));
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.CreateSymbolicLink(path, "foo"));
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.CreateText(path));

		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (MissingFileTestCase.All, (file, path)
				=> file.Decrypt(path));
#pragma warning restore CA1416
		}

		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.Delete(path));
		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			//yield return (MissingFileTestCase.All, (file, path)
			//	=> file.Encrypt(path));
#pragma warning restore CA1416
		}

		// `File.Exists` doesn't throw an exception on `null`
		yield return (MissingFileTestCase.All, (file, path)
			=> file.GetAttributes(path));
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		if (!Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (MissingFileTestCase.All, (file, path)
				=> file.GetUnixFileMode(path));
#pragma warning restore CA1416
		}
#endif
		yield return (MissingFileTestCase.FileMissing, (file, path)
			=> file.Move(path, "foo"));
		yield return (MissingFileTestCase.FileMissing, (file, path)
			=> file.Move("foo", path));
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (MissingFileTestCase.FileMissing, (file, path)
			=> file.Move(path, "foo", false));
		yield return (MissingFileTestCase.FileMissing, (file, path)
			=> file.Move("foo", path, false));
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.Open(path, FileMode.Open));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (MissingFileTestCase.All, (file, path)
			=> file.Open(path, new FileStreamOptions()));
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.OpenRead(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.OpenText(path));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.OpenWrite(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllBytes(path));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllBytesAsync(path, CancellationToken.None).GetAwaiter()
				.GetResult());
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllLines(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllLines(path, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllLinesAsync(path, CancellationToken.None).GetAwaiter()
				.GetResult());
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllLinesAsync(path, Encoding.UTF8, CancellationToken.None)
				.GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllText(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllText(path, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllTextAsync(path, CancellationToken.None).GetAwaiter()
				.GetResult());
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadAllTextAsync(path, Encoding.UTF8, CancellationToken.None)
				.GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadLines(path));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadLines(path, Encoding.UTF8));
#if FEATURE_FILESYSTEM_NET7
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadLinesAsync(path, CancellationToken.None));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ReadLinesAsync(path, Encoding.UTF8, CancellationToken.None));
#endif
#if FEATURE_FILESYSTEM_LINK
		yield return (MissingFileTestCase.All, (file, path)
			=> file.ResolveLinkTarget(path, false));
#endif
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetAttributes(path, FileAttributes.ReadOnly));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetCreationTime(path, DateTime.Now));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetCreationTimeUtc(path, DateTime.Now));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetLastAccessTime(path, DateTime.Now));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetLastAccessTimeUtc(path, DateTime.Now));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetLastWriteTime(path, DateTime.Now));
		yield return (MissingFileTestCase.All, (file, path)
			=> file.SetLastWriteTimeUtc(path, DateTime.Now));
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		if (!Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (MissingFileTestCase.All, (file, path)
				=> file.SetUnixFileMode(path, UnixFileMode.None));
#pragma warning restore CA1416
		}
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllBytes(path, new byte[]
			{
				0, 1
			}));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllBytesAsync(path, new byte[]
				{
					0, 1
				},
				CancellationToken.None).GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllLines(path, new[]
			{
				"foo"
			}));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllLines(path, new[]
			{
				"foo"
			}, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllLinesAsync(path, new[]
				{
					"foo"
				}, CancellationToken.None)
				.GetAwaiter().GetResult());
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllLinesAsync(path, new[]
				{
					"foo"
				}, Encoding.UTF8,
				CancellationToken.None).GetAwaiter().GetResult());
#endif
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllText(path, "foo"));
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllText(path, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllTextAsync(path, "foo", CancellationToken.None).GetAwaiter()
				.GetResult());
		yield return (MissingFileTestCase.DirectoryMissing, (file, path)
			=> file.WriteAllTextAsync(path, "foo", Encoding.UTF8,
				CancellationToken.None).GetAwaiter().GetResult());
#endif
	}

	#endregion
}