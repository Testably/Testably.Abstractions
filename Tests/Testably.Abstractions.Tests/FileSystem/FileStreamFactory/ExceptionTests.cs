using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

[FileSystemTests]
public partial class ExceptionTests
{
	[Theory]
	[MemberData(nameof(GetFileStreamFactoryCallbacks),
		"Illegal\tCharacter?InPath")]
	public async Task
		Operations_WhenValueContainsIllegalPathCharacters_ShouldThrowCorrectException_OnWindows(
			Expression<Action<IFileStreamFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileStream);
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
			if (Test.IsNetFramework)
			{
				await That(Act).Throws<ArgumentException>()
					.WithHResult(-2147024809)
					.Because(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
			else
			{
				await That(Act).Throws<IOException>()
					.WithHResult(-2147024773)
					.Because(
						$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
			}
		}
	}

	[Theory]
	[MemberData(nameof(GetFileStreamFactoryCallbacks), "")]
	public async Task Operations_WhenValueIsEmpty_ShouldThrowArgumentException(
		Expression<Action<IFileStreamFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileStream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has empty parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileStreamFactoryCallbacks), (string?)null)]
	public async Task Operations_WhenValueIsNull_ShouldThrowArgumentNullException(
		Expression<Action<IFileStreamFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileStream);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName(ignoreParamCheck ? null : paramName)
			.Because(
				$"\n{callback}\n has `null` parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	[Theory]
	[MemberData(nameof(GetFileStreamFactoryCallbacks), "  ")]
	public async Task Operations_WhenValueIsWhitespace_ShouldThrowArgumentException(
		Expression<Action<IFileStreamFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		void Act()
		{
			callback.Compile().Invoke(FileSystem.FileStream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithHResult(-2147024809).And
			.WithParamName(ignoreParamCheck || Test.IsNetFramework ? null : paramName)
			.Because(
				$"\n{callback}\n has whitespace parameter for '{paramName}' (ignored: {ignoreParamCheck})");
	}

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Expression<Action<IFileStreamFactory>>, string, bool>
		GetFileStreamFactoryCallbacks(string? path)
	{
		TheoryData<Expression<Action<IFileStreamFactory>>, string, bool> theoryData = new();
		foreach ((ExceptionTestHelper.TestTypes TestType,
			string ParamName,
			Expression<Action<IFileStreamFactory>> Callback) item in
			GetFileStreamFactoryCallbackTestParameters(path!)
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
			Expression<Action<IFileStreamFactory>> Callback)>
		GetFileStreamFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite,
				FileShare.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite,
				FileShare.None, 1024));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite,
				FileShare.None, 1024, FileOptions.None));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, new FileStreamOptions()));
#endif
	}

	#endregion
}
