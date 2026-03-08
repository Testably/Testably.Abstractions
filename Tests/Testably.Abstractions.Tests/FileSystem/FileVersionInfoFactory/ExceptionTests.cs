using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfoFactory;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IFileVersionInfoFactory>>, string, bool)>
		GetFileVersionInfoFactoryCallbacksWithEmptyPath()
		=> GetFileVersionInfoFactoryCallbacks("");

	public static IEnumerable<(Expression<Action<IFileVersionInfoFactory>>, string, bool)>
		GetFileVersionInfoFactoryCallbacksWithIllegalPathCharacters()
		=> GetFileVersionInfoFactoryCallbacks("Illegal\tCharacter?InPath");

	public static IEnumerable<(Expression<Action<IFileVersionInfoFactory>>, string, bool)>
		GetFileVersionInfoFactoryCallbacksWithNullPath()
		=> GetFileVersionInfoFactoryCallbacks(null);

	public static IEnumerable<(Expression<Action<IFileVersionInfoFactory>>, string, bool)>
		GetFileVersionInfoFactoryCallbacksWithWhitespacePath()
		=> GetFileVersionInfoFactoryCallbacks("  ");

	[Test]
	[MethodDataSource(nameof(GetFileVersionInfoFactoryCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileVersionInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileVersionInfo);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetFileVersionInfoFactoryCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileVersionInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileVersionInfo);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetFileVersionInfoFactoryCallbacksWithWhitespacePath))]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IFileVersionInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileVersionInfo);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	private static IEnumerable<(Expression<Action<IFileVersionInfoFactory>>, string, bool)>
		GetFileVersionInfoFactoryCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileVersionInfoFactory>> Callback) item in
			GetFileVersionInfoFactoryCallbackTestParameters(path!)
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
			Expression<Action<IFileVersionInfoFactory>> Callback)>
		GetFileVersionInfoFactoryCallbackTestParameters(string value)
	{
		yield return (
			ExceptionTestHelper.TestTypes.IgnoreParamNameCheck | ExceptionTestHelper.TestTypes.All,
			"fileName", fileVersionInfoFactory
				=> fileVersionInfoFactory.GetVersionInfo(value));
	}

	#endregion
}
