using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ParameterExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFile>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		if (!Test.IsNetFramework && paramName != null)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[MemberData(nameof(GetFileCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFile>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		if (paramName == null)
		{
			exception.Should().BeOfType<ArgumentNullException>();
		}
		else
		{
			exception.Should().BeOfType<ArgumentNullException>()
			   .Which.ParamName.Should().Be(paramName);
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileCallbacks(string? path)
		=> GetFileCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(ToTestType(path)))
		   .Select(item => new object?[] { item.Callback, item.ParamName });

	[Flags]
	private enum TestTypes
	{
		Null,
		Empty,
		InvalidPath,
		All = Null | Empty | InvalidPath
	}

	private static TestTypes ToTestType(string? parameter)
	{
		if (parameter == null)
		{
			return TestTypes.Null;
		}

		if (parameter == "")
		{
			return TestTypes.Empty;
		}

		return TestTypes.InvalidPath;
	}

	private static IEnumerable<(TestTypes TestType, string? ParamName,
			Expression<Action<IFile>> Callback)>
		GetFileCallbackTestParameters(string path)
	{
		yield return (TestTypes.All, "path", file
			=> file.AppendAllLines(path, new[] { "foo" }));
		yield return (TestTypes.All, "path", file
			=> file.AppendAllLines(path, new[] { "foo" }, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.AppendAllLinesAsync(path, new[] { "foo" }, CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.AppendAllLinesAsync(path, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.AppendAllText(path, "foo"));
		yield return (TestTypes.All, "path", file
			=> file.AppendAllText(path, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.AppendAllTextAsync(path, "foo", CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.AppendAllTextAsync(path, "foo", Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (TestTypes.Null | TestTypes.InvalidPath, "path", file
			=> file.AppendText(path));
		yield return (TestTypes.All, "sourceFileName", file
			=> file.Copy(path, "foo"));
		yield return (TestTypes.All, "destFileName", file
			=> file.Copy("foo", path));
		yield return (TestTypes.All, "sourceFileName", file
			=> file.Copy(path, "foo", false));
		yield return (TestTypes.All, "destFileName", file
			=> file.Copy("foo", path, false));
		yield return (TestTypes.All, "path", file
			=> file.Create(path));
		yield return (TestTypes.All, "path", file
			=> file.Create(path, 1023));
		yield return (TestTypes.All, "path", file
			=> file.Create(path, 1023, FileOptions.None));
#if FEATURE_FILESYSTEM_LINK
		yield return (TestTypes.All, "path", file
			=> file.CreateSymbolicLink(path, "foo"));
#endif
		yield return (TestTypes.Null | TestTypes.InvalidPath, "path", file
			=> file.CreateText(path));

		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (TestTypes.All, "path", file
				=> file.Decrypt(path));
#pragma warning restore CA1416
		}

		yield return (TestTypes.All, "path", file
			=> file.Delete(path));
		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (TestTypes.All, "path", file
				=> file.Encrypt(path));
#pragma warning restore CA1416
		}

		// `File.Exists` doesn't throw an exception on `null`
		yield return (TestTypes.All, "path", file
			=> file.GetAttributes(path));
		yield return (TestTypes.All, "path", file
			=> file.GetCreationTime(path));
		yield return (TestTypes.All, "path", file
			=> file.GetCreationTimeUtc(path));
		yield return (TestTypes.All, "path", file
			=> file.GetLastAccessTime(path));
		yield return (TestTypes.All, "path", file
			=> file.GetLastAccessTimeUtc(path));
		yield return (TestTypes.All, "path", file
			=> file.GetLastWriteTime(path));
		yield return (TestTypes.All, "path", file
			=> file.GetLastWriteTimeUtc(path));
		yield return (TestTypes.All, "sourceFileName", file
			=> file.Move(path, "foo"));
		yield return (TestTypes.All, "destFileName", file
			=> file.Move("foo", path));
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (TestTypes.All, "sourceFileName", file
			=> file.Move(path, "foo", false));
		yield return (TestTypes.All, "destFileName", file
			=> file.Move("foo", path, false));
#endif
		yield return (TestTypes.All, "path", file
			=> file.Open(path, FileMode.Open));
		yield return (TestTypes.All, "path", file
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite));
		yield return (TestTypes.All, "path", file
			=> file.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (TestTypes.All, "path", file
			=> file.Open(path, new FileStreamOptions()));
#endif
		yield return (TestTypes.All, "path", file
			=> file.OpenRead(path));
		yield return (TestTypes.Null | TestTypes.InvalidPath, "path", file
			=> file.OpenText(path));
		yield return (TestTypes.All, "path", file
			=> file.OpenWrite(path));
		yield return (TestTypes.All, "path", file
			=> file.ReadAllBytes(path));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.ReadAllBytesAsync(path, CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.ReadAllLines(path));
		yield return (TestTypes.All, "path", file
			=> file.ReadAllLines(path, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.ReadAllLinesAsync(path, CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.ReadAllLinesAsync(path, Encoding.UTF8, CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.ReadAllText(path));
		yield return (TestTypes.All, "path", file
			=> file.ReadAllText(path, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.ReadAllTextAsync(path, CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.ReadAllTextAsync(path, Encoding.UTF8, CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.ReadLines(path));
		yield return (TestTypes.All, "path", file
			=> file.ReadLines(path, Encoding.UTF8));
		yield return (TestTypes.All, null, file
			=> file.Replace(path, "foo", "bar"));
		yield return (TestTypes.All, null, file
			=> file.Replace("foo", path, "bar"));
		yield return (TestTypes.All, null, file
			=> file.Replace(path, "foo", "bar", false));
		yield return (TestTypes.All, null, file
			=> file.Replace("foo", path, "bar", false));
#if FEATURE_FILESYSTEM_LINK
		yield return (TestTypes.All, "linkPath", file
			=> file.ResolveLinkTarget(path, false));
#endif
		yield return (TestTypes.All, "path", file
			=> file.SetAttributes(path, FileAttributes.ReadOnly));
		yield return (TestTypes.All, "path", file
			=> file.SetCreationTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.SetCreationTimeUtc(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.SetLastAccessTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.SetLastAccessTimeUtc(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.SetLastWriteTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.SetLastWriteTimeUtc(path, DateTime.Now));
		yield return (TestTypes.All, "path", file
			=> file.WriteAllBytes(path, new byte[] { 0, 1 }));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.WriteAllBytesAsync(path, new byte[] { 0, 1 },
				CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.WriteAllLines(path, new[] { "foo" }));
		yield return (TestTypes.All, "path", file
			=> file.WriteAllLines(path, new[] { "foo" }, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.WriteAllLinesAsync(path, new[] { "foo" }, CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.WriteAllLinesAsync(path, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (TestTypes.All, "path", file
			=> file.WriteAllText(path, "foo"));
		yield return (TestTypes.All, "path", file
			=> file.WriteAllText(path, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (TestTypes.All, "path", file
			=> file.WriteAllTextAsync(path, "foo", CancellationToken.None));
		yield return (TestTypes.All, "path", file
			=> file.WriteAllTextAsync(path, "foo", Encoding.UTF8,
				CancellationToken.None));
#endif
	}

	#endregion
}