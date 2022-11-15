using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: "")]
	public void Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: "  ")]
	public void Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		exception.Should().BeException<ArgumentException>(
			hResult: -2147024809,
			paramName: ignoreParamCheck || Test.IsNetFramework ? null : paramName,
			because:
			$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: (string?)null)]
	public void Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		exception.Should().BeException<ArgumentNullException>(
			paramName: ignoreParamCheck ? null : paramName,
			because:
			$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowArgumentException_OnNetFramework(
			Expression<Action<IFileInfoFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		if (Test.IsNetFramework)
		{
			exception.Should().BeException<ArgumentException>(
				hResult: -2147024809,
				because:
				$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
		else
		{
			exception.Should()
				.BeNull(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileInfoFactoryCallbacks(string? path)
		=> GetFileInfoFactoryCallbackTestParameters(path!)
			.Where(item => item.TestType.HasFlag(path.ToTestType()))
			.Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes
					.IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IFileInfoFactory>> Callback)>
		GetFileInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptNull, "path", fileInfoFactory
			=> fileInfoFactory.New(value));
#if NET7_0_OR_GREATER
		// https://github.com/dotnet/runtime/issues/78224
		yield return (
			ExceptionTestHelper.TestTypes.Null | ExceptionTestHelper.TestTypes.IgnoreParamNameCheck,
			"fileName", fileInfoFactory
				=> fileInfoFactory.New(value));
#else
		yield return (ExceptionTestHelper.TestTypes.Null, "fileName", fileInfoFactory
			=> fileInfoFactory.New(value));
#endif
	}

	#endregion
}
