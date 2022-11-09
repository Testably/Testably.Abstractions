using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileStreamFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileStreamFactoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFileStreamFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileStream);
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
	[MemberData(nameof(GetFileStreamFactoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsWhitespace(
		Expression<Action<IFileStreamFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileStream);
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
	[MemberData(nameof(GetFileStreamFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFileStreamFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileStream);
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
	[MemberData(nameof(GetFileStreamFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfPathContainsIllegalCharactersOnWindows(
			Expression<Action<IFileStreamFactory>> callback, string paramName, bool ignoreParamCheck)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileStream);
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
			if (Test.IsNetFramework)
			{
				exception.Should().BeOfType<ArgumentException>()
				   .Which.HResult.Should().Be(-2147024809);
			}
			else
			{
				exception.Should().BeOfType<IOException>()
				   .Which.HResult.Should().Be(-2147024773);
			}
		}
	}

	#region Helpers

	public static IEnumerable<object?[]> GetFileStreamFactoryCallbacks(string? path)
		=> GetFileStreamFactoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[]
			{
				item.Callback,
				item.ParamName,
				item.TestType.HasFlag(ExceptionTestHelper.TestTypes.IgnoreParamNameCheck)
			});

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IFileStreamFactory>> Callback)>
		GetFileStreamFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1024));
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 1024, FileOptions.None));
#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", fileStreamFactory
			=> fileStreamFactory.New(value, new FileStreamOptions()));
#endif
	}

	#endregion
}