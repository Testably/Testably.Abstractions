using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

[FileSystemTests]
public partial class ExceptionTests
{
	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), "Illegal\tCharacter?InPath")]
	public async Task Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowArgumentException_OnNetFramework(
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
			await That(exception).IsNull().Because($"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), "")]
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

	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), (string?)null)]
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

	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), "  ")]
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

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFileInfoFactory>>, string, bool>
		GetFileInfoFactoryCallbacks(string? path)
	{
		TheoryData<Expression<Action<IFileInfoFactory>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileInfoFactory>> Callback) item in
			GetFileInfoFactoryCallbackTestParameters(path!)
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
			Expression<Action<IFileInfoFactory>> Callback)>
		GetFileInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptNull, "path", fileInfoFactory
			=> fileInfoFactory.New(value));
#if NET8_0_OR_GREATER
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
