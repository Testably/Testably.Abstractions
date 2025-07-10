using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

[FileSystemTests]
public partial class ExceptionTests
{
	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), "Illegal\tCharacter?InPath")]
	public async Task
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		}

		if (!Test.RunsOnWindows)
		{
			Exception? exception = Record.Exception(Act);
			if (exception is IOException ioException)
			{
				await That(ioException.HResult).IsNotEqualTo(-2147024809).Because(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
		else
		{
			await That(Act).Throws<ArgumentException>()
				.WithHResult(-2147024809)
				.Because(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), "")]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), (string?)null)]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), "  ")]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFileSystemWatcherFactory>>, string, bool>
		GetFileSystemWatcherFactoryCallbacks(string? path)
	{
		TheoryData<Expression<Action<IFileSystemWatcherFactory>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileSystemWatcherFactory>> Callback) item in
			GetFileSystemWatcherFactoryCallbackTestParameters(path!)
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
			Expression<Action<IFileSystemWatcherFactory>> Callback)>
		GetFileSystemWatcherFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileSystemWatcherFactory
			=> fileSystemWatcherFactory.New(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileSystemWatcherFactory
			=> fileSystemWatcherFactory.New(value, "*"));
	}

	#endregion
}
