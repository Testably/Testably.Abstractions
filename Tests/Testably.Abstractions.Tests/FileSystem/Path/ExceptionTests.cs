using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IPath>>, string, bool)>
		GetPathCallbacksWithEmptyPath()
		=> GetPathCallbacks("");

	public static IEnumerable<(Expression<Action<IPath>>, string, bool)>
		GetPathCallbacksWithNullPath()
		=> GetPathCallbacks(null);

	public static IEnumerable<(Expression<Action<IPath>>, string, bool)>
		GetPathCallbacksWithWhitespacePath()
		=> GetPathCallbacks("  ");

	[Test]
	[MethodDataSource(nameof(GetPathCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.Path);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetPathCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.Path);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetPathCallbacksWithWhitespacePath))]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.Path);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	private static IEnumerable<(Expression<Action<IPath>>, string, bool)> GetPathCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IPath>> Callback) item in GetPathCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			yield return (
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck));
		}
	}
	#pragma warning restore MA0018

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IPath>> Callback)>
		GetPathCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path", path
			=> path.GetFullPath(value));
#if FEATURE_PATH_RELATIVE
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "relativeTo", path
			=> path.GetRelativePath(value, "foo"));
#endif
#if FEATURE_PATH_RELATIVE
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path", path
			=> path.GetRelativePath("foo", value));
#endif
#if FEATURE_PATH_RELATIVE
		yield return (ExceptionTestHelper.TestTypes.Null, "path", path
			=> path.IsPathFullyQualified(value));
#endif
	}

	#endregion
}
