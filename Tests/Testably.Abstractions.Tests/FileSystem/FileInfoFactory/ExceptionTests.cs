using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFileInfoFactory>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
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
	[MemberData(nameof(GetFileInfoFactoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFileInfoFactory>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo);
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

	public static IEnumerable<object?[]> GetFileInfoFactoryCallbacks(string? path)
		=> GetFileInfoFactoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object?[] { item.Callback, item.ParamName });

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string? ParamName,
			Expression<Action<IFileInfoFactory>> Callback)>
		GetFileInfoFactoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.Empty, "path", fileInfoFactory
			=> fileInfoFactory.New(value));
		yield return (ExceptionTestHelper.TestTypes.Null, "fileName", fileInfoFactory
			=> fileInfoFactory.New(value));
	}

	#endregion
}