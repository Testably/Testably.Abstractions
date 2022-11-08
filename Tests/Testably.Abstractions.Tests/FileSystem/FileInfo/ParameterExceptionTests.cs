using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ParameterExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetFileInfoCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IFileInfo>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
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
	[MemberData(nameof(GetFileInfoCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IFileInfo>> callback, string? paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.FileInfo.New("foo"));
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

	public static IEnumerable<object?[]> GetFileInfoCallbacks(string? path)
		=> GetFileInfoCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(ToTestType(path)))
		   .Select(item => new object?[] { item.Callback, item.ParamName });

	[Flags]
	private enum TestTypes
	{
		Null,
		Empty,
		InvalidPath,
		All = Null | Empty | InvalidPath
	}

	private static TestTypes ToTestType(string? parameter)
	{
		if (parameter == null)
		{
			return TestTypes.Null;
		}

		if (parameter == "")
		{
			return TestTypes.Empty;
		}

		return TestTypes.InvalidPath;
	}

	private static IEnumerable<(TestTypes TestType, string? ParamName,
			Expression<Action<IFileInfo>> Callback)>
		GetFileInfoCallbackTestParameters(string path)
	{
		yield return (TestTypes.All, "destFileName", fileInfo
			=> fileInfo.CopyTo(path));
		yield return (TestTypes.All, "destFileName", fileInfo
			=> fileInfo.CopyTo(path, false));
		yield return (TestTypes.All, "destFileName", fileInfo
			=> fileInfo.MoveTo(path));
#if FEATURE_FILE_MOVETO_OVERWRITE
		yield return (TestTypes.All, "destFileName", fileInfo
			=> fileInfo.MoveTo(path, false));
#endif
		yield return (TestTypes.All, null, fileInfo
			=> fileInfo.Replace(path, "bar"));
		yield return (TestTypes.All, null, fileInfo
			=> fileInfo.Replace(path, "bar", false));
	}

	#endregion
}