using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IFileInfo>>, string, bool)>
		GetFileInfoCallbacksWithEmptyPath()
		=> GetFileInfoCallbacks("");

	public static IEnumerable<(Expression<Action<IFileInfo>>, string, bool)>
		GetFileInfoCallbacksWithIllegalPathCharacters()
		=> GetFileInfoCallbacks("Illegal\tCharacter?InPath");

	public static IEnumerable<(Expression<Action<IFileInfo>>, string, bool)>
		GetFileInfoCallbacksWithNullPath()
		=> GetFileInfoCallbacks(null);

	public static IEnumerable<(Expression<Action<IFileInfo>>, string, bool)>
		GetFileInfoCallbacksWithWhitespacePath()
		=> GetFileInfoCallbacks("  ");

	[Test]
	[MethodDataSource(nameof(GetFileInfoCallbacksWithIllegalPathCharacters))]
	public async Task
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IFileInfo>> callback, string paramName,
			bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
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
	[MethodDataSource(nameof(GetFileInfoCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetFileInfoCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static IEnumerable<(Expression<Action<IFileInfo>>, string, bool)> GetFileInfoCallbacks(
		string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileInfo>> Callback) item in GetFileInfoCallbackTestParameters(path!)
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
			Expression<Action<IFileInfo>> Callback)>
		GetFileInfoCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destFileName",
			fileInfo
				=> fileInfo.CopyTo(value));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destFileName",
			fileInfo
				=> fileInfo.CopyTo(value, false));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destFileName",
			fileInfo
				=> fileInfo.MoveTo(value));
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destFileName",
			fileInfo
				=> fileInfo.MoveTo(value, false));
#endif
		yield return (ExceptionTestHelper.TestTypes.All |
		              ExceptionTestHelper.TestTypes.IgnoreParamNameCheck,
			"destinationFileName", fileInfo
				=> fileInfo.Replace(value, "bar"));
		yield return (ExceptionTestHelper.TestTypes.All |
		              ExceptionTestHelper.TestTypes.IgnoreParamNameCheck,
			"destinationFileName", fileInfo
				=> fileInfo.Replace(value, "bar", false));
	}

	#endregion
}
