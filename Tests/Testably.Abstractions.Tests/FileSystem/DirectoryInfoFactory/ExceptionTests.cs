using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetDirectoryInfoFactoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IDirectoryInfoFactory>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		});

		if (!Test.IsNetFramework && paramName != null)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[MemberData(nameof(GetDirectoryInfoFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IDirectoryInfoFactory>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
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

	#region Helpers

	public static IEnumerable<object?[]> GetDirectoryInfoFactoryCallbacks(string? path)
		=> GetDirectoryInfoFactoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[] { item.Callback, item.ParamName });

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoFactoryCallbacks),
		parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldNotThrowAnyExceptionIfPathContainsIllegalCharactersOnWindows(
			Expression<Action<IDirectoryInfoFactory>> callback, string paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
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

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryInfoFactoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsWhitespace(
		Expression<Action<IDirectoryInfoFactory>> callback, string paramName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.DirectoryInfo);
		});

		if (!Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IDirectoryInfoFactory>> Callback)>
		GetDirectoryInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", directoryInfoFactory
			=> directoryInfoFactory.New(value));
	}

	#endregion
}