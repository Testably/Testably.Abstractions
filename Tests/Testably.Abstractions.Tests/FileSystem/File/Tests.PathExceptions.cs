using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class Tests<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public static IEnumerable<object[]> GetFileCallbacks(string? path)
		=> GetFileCallbackTestParameters(path!)
		   .Select(callback => new object[] { callback });

	#endregion

	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFile>> callback)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		if (!Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be("path");
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFile>> callback)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("path");
	}

	#region Helpers

	private static IEnumerable<Expression<Action<IFile>>>
		GetFileCallbackTestParameters(string path)
	{
		yield return file
			=> file.AppendAllLines(path, new[] { "foo" });
		yield return file
			=> file.AppendAllLines(path, new[] { "foo" }, Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.AppendAllLinesAsync(path, new[] { "foo" }, CancellationToken.None);
		yield return file
			=> file.AppendAllLinesAsync(path, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None);
#endif
		yield return file
			=> file.AppendAllText(path, "foo");
		yield return file
			=> file.AppendAllText(path, "foo", Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.AppendAllTextAsync(path, "foo", CancellationToken.None);
		yield return file
			=> file.AppendAllTextAsync(path, "foo", Encoding.UTF8,
				CancellationToken.None);
#endif
		if (path != "")
		{
			yield return file
				=> file.AppendText(path);
		}

		yield return file
			=> file.Create(path);
		yield return file
			=> file.Create(path, 1023);
		yield return file
			=> file.Create(path, 1023, FileOptions.None);
#if FEATURE_FILESYSTEM_LINK
		yield return file
			=> file.CreateSymbolicLink(path, "foo");
#endif
		if (path != "")
		{
			yield return file
				=> file.CreateText(path);
		}

		if (Test.RunsOnWindows)
		{
			yield return file
				=> file.Decrypt(path);
		}

		yield return file
			=> file.Delete(path);
		if (Test.RunsOnWindows)
		{
			yield return file
				=> file.Encrypt(path);
		}

		// `File.Exists` doesn't throw an exception on `null`
		yield return file
			=> file.GetAttributes(path);
		yield return file
			=> file.GetCreationTime(path);
		yield return file
			=> file.GetCreationTimeUtc(path);
		yield return file
			=> file.GetLastAccessTime(path);
		yield return file
			=> file.GetLastAccessTimeUtc(path);
		yield return file
			=> file.GetLastWriteTime(path);
		yield return file
			=> file.GetLastWriteTimeUtc(path);
		yield return file
			=> file.Open(path, FileMode.Open);
		yield return file
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite);
		yield return file
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return file
			=> file.Open(path, new FileStreamOptions());
#endif
		yield return file
			=> file.OpenRead(path);
		if (path != "")
		{
			yield return file
				=> file.OpenText(path);
		}

		yield return file
			=> file.OpenWrite(path);
		yield return file
			=> file.ReadAllBytes(path);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.ReadAllBytesAsync(path, CancellationToken.None);
#endif
		yield return file
			=> file.ReadAllLines(path);
		yield return file
			=> file.ReadAllLines(path, Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.ReadAllLinesAsync(path, CancellationToken.None);
		yield return file
			=> file.ReadAllLinesAsync(path, Encoding.UTF8, CancellationToken.None);
#endif
		yield return file
			=> file.ReadAllText(path);
		yield return file
			=> file.ReadAllText(path, Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.ReadAllTextAsync(path, CancellationToken.None);
		yield return file
			=> file.ReadAllTextAsync(path, Encoding.UTF8, CancellationToken.None);
#endif
		yield return file
			=> file.ReadLines(path);
		yield return file
			=> file.ReadLines(path, Encoding.UTF8);
		yield return file
			=> file.SetAttributes(path, FileAttributes.ReadOnly);
		yield return file
			=> file.SetCreationTime(path, DateTime.Now);
		yield return file
			=> file.SetCreationTimeUtc(path, DateTime.Now);
		yield return file
			=> file.SetLastAccessTime(path, DateTime.Now);
		yield return file
			=> file.SetLastAccessTimeUtc(path, DateTime.Now);
		yield return file
			=> file.SetLastWriteTime(path, DateTime.Now);
		yield return file
			=> file.SetLastWriteTimeUtc(path, DateTime.Now);
		yield return file
			=> file.WriteAllBytes(path, new byte[] { 0, 1 });
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.WriteAllBytesAsync(path, new byte[] { 0, 1 }, CancellationToken.None);
#endif
		yield return file
			=> file.WriteAllLines(path, new[] { "foo" });
		yield return file
			=> file.WriteAllLines(path, new[] { "foo" }, Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.WriteAllLinesAsync(path, new[] { "foo" }, CancellationToken.None);
		yield return file
			=> file.WriteAllLinesAsync(path, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None);
#endif
		yield return file
			=> file.WriteAllText(path, "foo");
		yield return file
			=> file.WriteAllText(path, "foo", Encoding.UTF8);
#if FEATURE_FILESYSTEM_ASYNC
		yield return file
			=> file.WriteAllTextAsync(path, "foo", CancellationToken.None);
		yield return file
			=> file.WriteAllTextAsync(path, "foo", Encoding.UTF8, CancellationToken.None);
#endif
	}

	#endregion
}