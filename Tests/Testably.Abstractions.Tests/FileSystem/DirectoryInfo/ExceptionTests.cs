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
	public void Operations_ShouldThrowArgumentExceptionIfValueIsEmpty(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
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

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsWhitespace(
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
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})")
				.Which.ParamName.Should().Be(paramName,
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})")
				.Which.HResult.Should().Be(-2147024809,
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfValueIsNull(
		Expression<Action<IDirectoryInfo>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
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
	[MemberData(nameof(GetDirectoryInfoCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfValueContainsIllegalPathCharactersOnWindows(
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
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "destDirName",
			directoryInfo
				=> directoryInfo.MoveTo(value));
	}

	#endregion
}