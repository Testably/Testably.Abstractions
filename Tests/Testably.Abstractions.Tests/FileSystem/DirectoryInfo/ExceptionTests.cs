using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because: $"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: "  ")]
	public void Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		});

		if (Test.IsNetFramework)
		{
			exception.Should()
				.BeNull(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
		else
		{
			exception.Should().BeException<ArgumentException>(
				hResult: -2147024809,
				paramName: paramName,
				because: $"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IDirectoryInfo>> callback, string paramName,
			bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
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
					because: $"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
			else
			{
				exception.Should().BeException<IOException>(
					hResult: -2147024773,
					because: $"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetDirectoryInfoCallbacks(string? path)
		=> GetDirectoryInfoCallbackTestParameters(path!)
			.Where(item => item.TestType.HasFlag(path.ToTestType()))
			.Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes
					.IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
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