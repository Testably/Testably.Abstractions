using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(GetPathCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Path);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetPathCallbacks), parameters: "  ")]
	public void Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Path);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetPathCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IPath>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Path);
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	public static IEnumerable<object?[]> GetPathCallbacks(string? path)
		=> GetPathCallbackTestParameters(path!)
			.Where(item => item.TestType.HasFlag(path.ToTestType()))
			.Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes
					.IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IPath>> Callback)>
		GetPathCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path", path
			=> path.GetFullPath(value));
#if FEATURE_PATH_RELATIVE
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "relativeTo", path
			=> path.GetRelativePath(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path", path
			=> path.GetRelativePath("foo", value));
		yield return (ExceptionTestHelper.TestTypes.Null, "path", path
			=> path.IsPathFullyQualified(value));
#endif
	}

#endregion
}
