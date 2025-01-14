using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class ExceptionTests
{
	[SkippableTheory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IDirectory>> callback, string paramName,
			bool ignoreParamCheck, Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
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
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDirectory>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDirectory>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "  ")]
	public void Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IDirectory>> callback, string paramName, bool ignoreParamCheck,
		Func<Test, bool> skipTest)
	{
		Skip.If(skipTest(Test));

		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IDirectory>>, string, bool, Func<Test, bool>>
		GetDirectoryCallbacks(string? path)
	{
		TheoryData<Expression<Action<IDirectory>>, string, bool, Func<Test, bool>> theoryData =
			new();
		foreach ((ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IDirectory>> Callback, Func<Test, bool>? SkipTest) item in
			GetDirectoryCallbackTestParameters(path!)
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
			Expression<Action<IDirectory>> Callback, Func<Test, bool>? SkipTest)>
		GetDirectoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.CreateDirectory(value),
			null);
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.CreateDirectory(value, UnixFileMode.None),
			test => test.RunsOnWindows);
#endif
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.CreateSymbolicLink(value, "foo"),
			null);
#endif
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "pathToTarget",
			directory
				=> directory.CreateSymbolicLink("foo", value),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.Delete(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.Delete(value, true),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateDirectories(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateDirectories(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateDirectories(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateDirectories(value, "foo", new EnumerationOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFiles(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFiles(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFiles(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFiles(value, "foo", new EnumerationOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFileSystemEntries(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFileSystemEntries(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFileSystemEntries(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.EnumerateFileSystemEntries(value, "foo", new EnumerationOptions()),
			null);
#endif
		// `Directory.Exists` doesn't throw an exception on `null`
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetCreationTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetCreationTimeUtc(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetDirectories(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetDirectories(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetDirectories(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetDirectories(value, "foo", new EnumerationOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFiles(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFiles(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFiles(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFiles(value, "foo", new EnumerationOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFileSystemEntries(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFileSystemEntries(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFileSystemEntries(value, "foo", SearchOption.AllDirectories),
			null);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetFileSystemEntries(value, "foo", new EnumerationOptions()),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetLastAccessTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetLastAccessTimeUtc(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetLastWriteTime(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.GetLastWriteTimeUtc(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path",
			directory
				=> directory.GetParent(value),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "sourceDirName",
			directory
				=> directory.Move(value, "foo"),
			null);
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destDirName",
			directory
				=> directory.Move("foo", value),
			null);
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.AllExceptWhitespace, "linkPath",
			directory
				=> directory.ResolveLinkTarget(value, false),
			null);
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetCreationTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetCreationTimeUtc(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetLastAccessTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetLastAccessTimeUtc(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetLastWriteTime(value, DateTime.Now),
			null);
		yield return (ExceptionTestHelper.TestTypes.All, "path",
			directory
				=> directory.SetLastWriteTimeUtc(value, DateTime.Now),
			null);
	}

	#endregion
}
