using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsEmpty(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		});

		if (!Test.IsNetFramework && !ignoreParamCheck)
		{
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})")
				.Which.ParamName.Should().Be(paramName,
					$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}

		exception.Should().BeOfType<ArgumentException>(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})")
			.Which.HResult.Should().Be(-2147024809,
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfValueIsNull(
		Expression<Action<IFileInfo>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
		});

		if (ignoreParamCheck)
		{
			exception.Should().BeOfType<ArgumentNullException>(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
		else
		{
			exception.Should().BeOfType<ArgumentNullException>(
					$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})")
				.Which.ParamName.Should().Be(paramName,
					$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfValueContainsIllegalPathCharactersOnWindows(
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
				exception.Should().BeOfType<ArgumentException>(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})")
					.Which.HResult.Should().Be(-2147024809,
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
			else
			{
				exception.Should().BeOfType<IOException>(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})")
					.Which.HResult.Should().Be(-2147024773,
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileInfoCallbacks(string? path)
		=> GetFileInfoCallbackTestParameters(path!)
			.Where(item => item.TestType.HasFlag(path.ToTestType()))
			.Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes
					.IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
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