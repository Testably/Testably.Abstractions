using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
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
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[] { item.Callback, item.ParamName });
	
	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IFile>> Callback)>
		GetFileCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllLines(value, new[] { "foo" }));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllLines(value, new[] { "foo" }, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllLinesAsync(value, new[] { "foo" }, CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllLinesAsync(value, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllText(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllText(value, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllTextAsync(value, "foo", CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.AppendAllTextAsync(value, "foo", Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path", file
			=> file.AppendText(value));
		yield return (ExceptionTestHelper.TestTypes.All, "sourceFileName", file
			=> file.Copy(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "destFileName", file
			=> file.Copy("foo", value));
		yield return (ExceptionTestHelper.TestTypes.All, "sourceFileName", file
			=> file.Copy(value, "foo", false));
		yield return (ExceptionTestHelper.TestTypes.All, "destFileName", file
			=> file.Copy("foo", value, false));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Create(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Create(value, 1023));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Create(value, 1023, FileOptions.None));
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.CreateSymbolicLink(value, "foo"));
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path", file
			=> file.CreateText(value));

		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (ExceptionTestHelper.TestTypes.All, "path", file
				=> file.Decrypt(value));
#pragma warning restore CA1416
		}

		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Delete(value));
		if (Test.RunsOnWindows)
		{
#pragma warning disable CA1416
			yield return (ExceptionTestHelper.TestTypes.All, "path", file
				=> file.Encrypt(value));
#pragma warning restore CA1416
		}

		// `File.Exists` doesn't throw an exception on `null`
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetAttributes(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetCreationTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetCreationTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetLastAccessTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetLastAccessTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetLastWriteTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.GetLastWriteTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.All, "sourceFileName", file
			=> file.Move(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "destFileName", file
			=> file.Move("foo", value));
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (ExceptionTestHelper.TestTypes.All, "sourceFileName", file
			=> file.Move(value, "foo", false));
		yield return (ExceptionTestHelper.TestTypes.All, "destFileName", file
			=> file.Move("foo", value, false));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Open(value, FileMode.Open));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Open(value, FileMode.Open, FileAccess.ReadWrite));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Open(value, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.Open(value, new FileStreamOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.OpenRead(value));
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path", file
			=> file.OpenText(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.OpenWrite(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllBytes(value));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllBytesAsync(value, CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllLines(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllLines(value, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllLinesAsync(value, CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllLinesAsync(value, Encoding.UTF8, CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllText(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllText(value, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllTextAsync(value, CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadAllTextAsync(value, Encoding.UTF8, CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadLines(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.ReadLines(value, Encoding.UTF8));
		yield return (ExceptionTestHelper.TestTypes.All, null, file
			=> file.Replace(value, "foo", "bar"));
		yield return (ExceptionTestHelper.TestTypes.All, null, file
			=> file.Replace("foo", value, "bar"));
		yield return (ExceptionTestHelper.TestTypes.All, null, file
			=> file.Replace(value, "foo", "bar", false));
		yield return (ExceptionTestHelper.TestTypes.All, null, file
			=> file.Replace("foo", value, "bar", false));
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "linkPath", file
			=> file.ResolveLinkTarget(value, false));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetAttributes(value, FileAttributes.ReadOnly));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetCreationTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetCreationTimeUtc(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetLastAccessTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetLastAccessTimeUtc(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetLastWriteTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.SetLastWriteTimeUtc(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllBytes(value, new byte[] { 0, 1 }));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllBytesAsync(value, new byte[] { 0, 1 },
				CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllLines(value, new[] { "foo" }));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllLines(value, new[] { "foo" }, Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllLinesAsync(value, new[] { "foo" }, CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllLinesAsync(value, new[] { "foo" }, Encoding.UTF8,
				CancellationToken.None));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllText(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllText(value, "foo", Encoding.UTF8));
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllTextAsync(value, "foo", CancellationToken.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", file
			=> file.WriteAllTextAsync(value, "foo", Encoding.UTF8,
				CancellationToken.None));
#endif
	}

	#endregion
}