using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IDirectoryInfo>>, string, bool)>
		GetDirectoryInfoCallbacksWithEmptyPath()
		=> GetDirectoryInfoCallbacks("");

	public static IEnumerable<(Expression<Action<IDirectoryInfo>>, string, bool)>
		GetDirectoryInfoCallbacksWithIllegalPathCharacters()
		=> GetDirectoryInfoCallbacks("Illegal\tCharacter?InPath");

	public static IEnumerable<(Expression<Action<IDirectoryInfo>>, string, bool)>
		GetDirectoryInfoCallbacksWithNullPath()
		=> GetDirectoryInfoCallbacks(null);

	public static IEnumerable<(Expression<Action<IDirectoryInfo>>, string, bool)>
		GetDirectoryInfoCallbacksWithWhitespacePath()
		=> GetDirectoryInfoCallbacks("  ");

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoCallbacksWithIllegalPathCharacters))]
	public async Task
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IDirectoryInfo>> callback, string paramName,
			bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		}

		if (!Test.RunsOnWindows)
		{
			Exception? exception = Record.Exception(Act);
			if (exception is IOException ioException)
			{
				await That(ioException.HResult).IsNotEqualTo(-2147024809).Because(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
		else
		{
			if (Test.IsNetFramework)
			{
				await That(Act).Throws<ArgumentException>()
					.WithHResult(-2147024809)
					.Because(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
			else
			{
				await That(Act).Throws<IOException>()
					.WithHResult(-2147024773)
					.Because(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoCallbacksWithWhitespacePath))]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		}

		if (Test.IsNetFramework)
		{
			await That(Act).DoesNotThrow()
				.Because(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
		else
		{
			await That(Act).Throws<ArgumentException>()
				.WithHResult(-2147024809).And
				.WithParamName(paramName)
				.Because(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	#region Helpers

	#pragma warning disable MA0018
	private static IEnumerable<(Expression<Action<IDirectoryInfo>>, string, bool)>
		GetDirectoryInfoCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IDirectoryInfo>> Callback) item in
			GetDirectoryInfoCallbackTestParameters(path!)
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
			Expression<Action<IDirectoryInfo>> Callback)>
		GetDirectoryInfoCallbackTestParameters(string value)
	{
		yield return (
			ExceptionTestHelper.TestTypes.Whitespace |
			ExceptionTestHelper.TestTypes.InvalidPath, "path",
			directoryInfo
				=> directoryInfo.CreateSubdirectory(value));
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "pathToTarget",
			directoryInfo
				=> directoryInfo.CreateAsSymbolicLink(value));
#endif
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destDirName",
			directoryInfo
				=> directoryInfo.MoveTo(value));
	}

	#endregion
}
