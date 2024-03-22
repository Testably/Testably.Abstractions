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
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IFile>> callback, string paramName, bool ignoreParamCheck,
			Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		if (!Test.RunsOnWindows)
		{
			if (exception is IOException ioException)
			{
				ioException.HResult.Should().NotBe(-2147024809,
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
		else
		{
			if (Test.IsNetFramework)
			{
				exception.Should().BeException<ArgumentException>(
					hResult: -2147024809,
					because:
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
			else
			{
				exception.Should().BeException<IOException>(
					hResult: -2147024773,
					because:
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFile>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFile>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileCallbacks), parameters: "  ")]
	public void Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IFile>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.File);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFile>>, string, bool, Func<Test, bool>>
		GetFileCallbacks(string? path)
	{
		TheoryData<Expression<Action<IFile>>, string, bool, Func<Test, bool>> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IFile>> Callback, Func<Test, bool>? SkipTest) item in
			GetFileCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			theoryData.Add(
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck),
				item.SkipTest ?? (_ => false));
		}

		return theoryData;
	}
	#pragma warning restore MA0018

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IFile>> Callback, Func<Test, bool>? SkipTest)>
		GetFileCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllLines(value, new[]
				{
					"foo"
				}),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllLines(value, new[]
				{
					"foo"
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllLinesAsync(value, new[]
					{
						"foo"
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllLinesAsync(value, new[]
					{
						"foo"
					}, Encoding.UTF8,
					CancellationToken.None).GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllText(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllText(value, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllTextAsync(value, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.AppendAllTextAsync(value, "foo", Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path",
			file
				=> file.AppendText(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.AllExceptWhitespace, "sourceFileName",
			file
				=> file.Copy(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destFileName",
			file
				=> file.Copy("foo", value),
			null);
		yield return (ExceptionTestHelper.TestTypes.AllExceptWhitespace, "sourceFileName",
			file
				=> file.Copy(value, "foo", false),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destFileName",
			file
				=> file.Copy("foo", value, false), null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Copy(value, "foo"),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destFileName",
			file
				=> file.Copy("foo", value),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Copy(value, "foo", false),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destFileName",
			file
				=> file.Copy("foo", value, false),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Create(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Create(value, 1023),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Create(value, 1023, FileOptions.None),
			null);
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.CreateSymbolicLink(value, "foo"),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path",
			file
				=> file.CreateText(value),
			null);
		#pragma warning disable CA1416
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path",
			file
				=> file.Decrypt(value),
			test => !test.RunsOnWindows);
		#pragma warning restore CA1416
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Delete(value),
			null);
		#pragma warning disable CA1416
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path",
			file
				=> file.Encrypt(value),
			test => !test.RunsOnWindows);
		#pragma warning restore CA1416
		// `File.Exists` doesn't throw an exception on `null`
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetAttributes(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetCreationTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetCreationTimeUtc(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetLastAccessTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetLastAccessTimeUtc(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetLastWriteTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetLastWriteTimeUtc(value),
			null);
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		#pragma warning disable CA1416
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.GetUnixFileMode(value),
			test => test.RunsOnWindows);
		#pragma warning restore CA1416
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "sourceFileName",
			file
				=> file.Move(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destFileName",
			file
				=> file.Move("foo", value),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Move(value, "foo"),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destFileName",
			file
				=> file.Move("foo", value),
			null);
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "sourceFileName",
			file
				=> file.Move(value, "foo", false),
			null);
#endif
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destFileName",
			file
				=> file.Move("foo", value, false),
			null);
#endif
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Move(value, "foo", false),
			null);
#endif
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destFileName",
			file
				=> file.Move("foo", value, false),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Open(value, FileMode.Open),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Open(value, FileMode.Open, FileAccess.ReadWrite),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Open(value, FileMode.Open, FileAccess.ReadWrite, FileShare.None),
			null);
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.Open(value, new FileStreamOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.OpenRead(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrInvalidPath, "path",
			file
				=> file.OpenText(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.OpenWrite(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllBytes(value),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllBytesAsync(value, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllLines(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllLines(value, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllLinesAsync(value, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllLinesAsync(value, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllText(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllText(value, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllTextAsync(value, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadAllTextAsync(value, Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadLines(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadLines(value, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_NET7
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadLinesAsync(value, CancellationToken.None),
			null);
#endif
#if FEATURE_FILESYSTEM_NET7
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.ReadLinesAsync(value, Encoding.UTF8, CancellationToken.None),
			null);
#endif
		yield return (
			ExceptionTestHelper.TestTypes.AllExceptInvalidPath |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Replace(value, "foo", "bar"),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.All |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destinationFileName",
			file
				=> file.Replace("foo", value, "bar"),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.AllExceptInvalidPath |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "sourceFileName",
			file
				=> file.Replace(value, "foo", "bar", false),
			null);
		yield return (
			ExceptionTestHelper.TestTypes.All |
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck, "destinationFileName",
			file
				=> file.Replace("foo", value, "bar", false),
			null);
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.AllExceptWhitespace, "linkPath",
			file
				=> file.ResolveLinkTarget(value, false),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetAttributes(value, FileAttributes.ReadOnly),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetCreationTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetCreationTimeUtc(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetLastAccessTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetLastAccessTimeUtc(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetLastWriteTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetLastWriteTimeUtc(value, DateTime.Now),
			null);
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		#pragma warning disable CA1416
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.SetUnixFileMode(value, UnixFileMode.None),
			test => test.RunsOnWindows);
		#pragma warning restore CA1416
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllBytes(value, new byte[]
				{
					0, 1
				}),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllBytesAsync(value, new byte[]
					{
						0, 1
					},
					CancellationToken.None).GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllLines(value, new[]
				{
					"foo"
				}),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllLines(value, new[]
				{
					"foo"
				}, Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllLinesAsync(value, new[]
					{
						"foo"
					}, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllLinesAsync(value, new[]
					{
						"foo"
					}, Encoding.UTF8,
					CancellationToken.None).GetAwaiter().GetResult(),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllText(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllText(value, "foo", Encoding.UTF8),
			null);
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllTextAsync(value, "foo", CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
#if FEATURE_FILESYSTEM_ASYNC
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			file
				=> file.WriteAllTextAsync(value, "foo", Encoding.UTF8, CancellationToken.None)
					.GetAwaiter().GetResult(),
			null);
#endif
	}

	#endregion
}
