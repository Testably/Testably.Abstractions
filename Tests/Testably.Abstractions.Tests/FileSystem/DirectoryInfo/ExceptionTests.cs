using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IDirectoryInfo>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		});

		if (!Test.IsNetFramework && paramName != null)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsWhitespace(
		Expression<Action<IDirectoryInfo>> callback, string paramName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
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
	[MemberData(nameof(GetDirectoryInfoCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IDirectoryInfo>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
		});

		if (paramName == null)
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
	[MemberData(nameof(GetDirectoryInfoCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfPathContainsIllegalCharactersOnWindows(
			Expression<Action<IDirectoryInfo>> callback, string paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo.New("foo"));
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

	public static IEnumerable<object?[]> GetDirectoryInfoCallbacks(string? path)
		=> GetDirectoryInfoCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[] { item.Callback, item.ParamName });

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IDirectoryInfo>> Callback)>
		GetDirectoryInfoCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.AllExceptWhitespace, "path",
			directoryInfo
				=> directoryInfo.CreateSubdirectory(value));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destDirName",
			directoryInfo
				=> directoryInfo.MoveTo(value));
	}

	#endregion
}