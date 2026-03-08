#if FEATURE_FILESYSTEM_LINK
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemInfo;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IFileSystemInfo>>, string, bool)>
		GetFileSystemInfoCallbacksWithEmptyPath()
		=> GetFileSystemInfoCallbacks("");

	public static IEnumerable<(Expression<Action<IFileSystemInfo>>, string, bool)>
		GetFileSystemInfoCallbacksWithNullPath()
		=> GetFileSystemInfoCallbacks(null);

	[Test]
	[MethodDataSource(nameof(GetFileSystemInfoCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileSystemInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because($"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetFileSystemInfoCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileSystemInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because($"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static IEnumerable<(Expression<Action<IFileSystemInfo>>, string, bool)>
		GetFileSystemInfoCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileSystemInfo>> Callback) item in
			GetFileSystemInfoCallbackTestParameters(path!)
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
			Expression<Action<IFileSystemInfo>> Callback)>
		GetFileSystemInfoCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "pathToTarget",
			fileSystemInfo
				=> fileSystemInfo.CreateAsSymbolicLink(value));
	}

	#endregion
}
#endif
