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
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		});

		if (!Test.IsNetFramework && !ignoreParamCheck)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsWhitespace(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		});

		if (!Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFileSystemWatcherFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		});

		if (ignoreParamCheck)
		{
			exception.Should().BeOfType<ArgumentNullException>();
		}
		else
		{
			exception.Should().BeOfType<ArgumentNullException>()
			   .Which.ParamName.Should().Be(paramName);
		}
	}

	[SkippableTheory]
	[MemberData(nameof(GetFileSystemWatcherFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfPathContainsIllegalCharactersOnWindows(
			Expression<Action<IFileSystemWatcherFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileSystemWatcher);
		});

		if (!Test.RunsOnWindows)
		{
			if (exception is IOException ioException)
			{
				ioException.HResult.Should().NotBe(-2147024809);
			}
		}
		else
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.HResult.Should().Be(-2147024809);
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileSystemWatcherFactoryCallbacks(string? path)
		=> GetFileSystemWatcherFactoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck)
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