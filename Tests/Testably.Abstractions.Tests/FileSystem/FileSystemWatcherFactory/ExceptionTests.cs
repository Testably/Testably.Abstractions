using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcherFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsEmpty(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
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
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfValueIsWhitespace(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		});

		if (!Test.IsNetFramework)
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
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfValueIsNull(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
		bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
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
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfValueContainsIllegalPathCharactersOnWindows(
			Expression<Action<IFileSystemWatcherFactory>> callback, string paramName,
			bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
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
			exception.Should().BeOfType<ArgumentException>(
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})")
			   .Which.HResult.Should().Be(-2147024809,
					$"\n{callback}\n contains invalid path characters for '{paramName}' (ignored: {ignoreParamCheck})");
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileSystemWatcherFactoryCallbacks(
		string? path)
		=> GetFileSystemWatcherFactoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[]
			{
				item.Callback, item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes
				   .IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
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