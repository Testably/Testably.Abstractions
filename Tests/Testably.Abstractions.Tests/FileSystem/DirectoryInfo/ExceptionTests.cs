using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class ExceptionTests
{
	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), "Illegal\tCharacter?InPath")]
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

	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), "")]
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

	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), (string?)null)]
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

	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), "  ")]
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
	public static TheoryData<Expression<Action<IDirectoryInfo>>, string, bool>
		GetDirectoryInfoCallbacks(string? path)
	{
		TheoryData<Expression<Action<IDirectoryInfo>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IDirectoryInfo>> Callback) item in
			GetDirectoryInfoCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			theoryData.Add(
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck));
		}

		return theoryData;
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
