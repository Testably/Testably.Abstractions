using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IFileInfo>> callback, string paramName,
			bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
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
	[MemberData(nameof(GetFileInfoCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFileInfo>>, string, bool> GetFileInfoCallbacks(
		string? path)
	{
		TheoryData<Expression<Action<IFileInfo>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileInfo>> Callback) item in GetFileInfoCallbackTestParameters(path!)
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
