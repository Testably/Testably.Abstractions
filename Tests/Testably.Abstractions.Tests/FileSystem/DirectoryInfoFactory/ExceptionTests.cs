using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

[FileSystemTests]
public class ExceptionTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	public static IEnumerable<(Expression<Action<IDirectoryInfoFactory>>, string, bool)>
		GetDirectoryInfoFactoryCallbacksWithEmptyPath()
		=> GetDirectoryInfoFactoryCallbacks("");

	public static IEnumerable<(Expression<Action<IDirectoryInfoFactory>>, string, bool)>
		GetDirectoryInfoFactoryCallbacksWithIllegalPathCharacters()
		=> GetDirectoryInfoFactoryCallbacks("Illegal\tCharacter?InPath");

	public static IEnumerable<(Expression<Action<IDirectoryInfoFactory>>, string, bool)>
		GetDirectoryInfoFactoryCallbacksWithNullPath()
		=> GetDirectoryInfoFactoryCallbacks(null);

	public static IEnumerable<(Expression<Action<IDirectoryInfoFactory>>, string, bool)>
		GetDirectoryInfoFactoryCallbacksWithWhitespacePath()
		=> GetDirectoryInfoFactoryCallbacks("  ");
	
	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoFactoryCallbacksWithIllegalPathCharacters))]
	public async Task
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowArgumentException_OnNetFramework(
			Expression<Action<IDirectoryInfoFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		}

		if (Test.IsNetFramework)
		{
			await That(Act).Throws<ArgumentException>()
				.WithHResult(-2147024809)
				.Because(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
		else
		{
			await That(Act).DoesNotThrow()
				.Because(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoFactoryCallbacksWithEmptyPath))]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoFactoryCallbacksWithNullPath))]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IDirectoryInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Test]
	[MethodDataSource(nameof(GetDirectoryInfoFactoryCallbacksWithWhitespacePath))]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IDirectoryInfoFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	private static IEnumerable<(Expression<Action<IDirectoryInfoFactory>>, string, bool)>
		GetDirectoryInfoFactoryCallbacks(string? path)
	{
		foreach ((ExceptionTestHelper.TestTypes TestType,
				string ParamName,
				Expression<Action<IDirectoryInfoFactory>> Callback) in
			GetDirectoryInfoFactoryCallbackTestParameters(path!)
				.Where(item => item.TestType.HasFlag(path.ToTestType())))
		{
			yield return (
				Callback,
				ParamName,
				TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck));
		}
	}

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IDirectoryInfoFactory>> Callback)>
		GetDirectoryInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", directoryInfoFactory
			=> directoryInfoFactory.New(value));
	}

	#endregion
}
