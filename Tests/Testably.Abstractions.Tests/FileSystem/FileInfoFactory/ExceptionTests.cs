using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsEmpty(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
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
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsWhitespace(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		if (!Test.IsNetFramework && !ignoreParamCheck)
		{
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})")
			   .Which.ParamName.Should().Be(paramName,
					$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
		}

		exception.Should().BeOfType<ArgumentException>(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})")
		   .Which.HResult.Should().Be(-2147024809,
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfValueIsNull(
		Expression<Action<IFileInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
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
	[MemberData(nameof(GetFileInfoFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfValueContainsIllegalPathCharactersOnWindows(
			Expression<Action<IFileInfoFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
		});

		if (Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})")
			   .Which.HResult.Should().Be(-2147024809,
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
				item.Callback, item.ParamName, item.TestType.HasFlag(ExceptionTestHelper
				   .TestTypes
				   .IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IFileInfoFactory>> Callback)>
		GetFileInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptNull, "path", fileInfoFactory
			=> fileInfoFactory.New(value));
		yield return (ExceptionTestHelper.TestTypes.Null, "fileName", fileInfoFactory
			=> fileInfoFactory.New(value));
	}

	#endregion
}