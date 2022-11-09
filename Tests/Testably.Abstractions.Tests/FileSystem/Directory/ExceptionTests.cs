using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExceptionTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[Theory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IDirectory>> callback, string paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		if (!Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be(paramName);
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "  ")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsWhitespace(
		Expression<Action<IDirectory>> callback, string paramName)
	{
		Skip.IfNot(Test.RunsOnWindows);

		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
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
	[MemberData(nameof(GetDirectoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IDirectory>> callback, string paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be(paramName);
	}

	[SkippableTheory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "Illegal\tCharacter?InPath")]
	public void
		Operations_ShouldThrowCorrectExceptionIfPathContainsIllegalCharactersOnWindows(
			Expression<Action<IDirectory>> callback, string paramName)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
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

	public static IEnumerable<object[]> GetDirectoryCallbacks(string? path)
		=> GetDirectoryCallbackTestParameters(path!)
		   .Where(item => item.TestType.HasFlag(path.ToTestType()))
		   .Select(item => new object[] { item.Callback, item.ParamName });

	private static IEnumerable<(ExceptionTestHelper.TestTypes TestType, string ParamName,
			Expression<Action<IDirectory>> Callback)>
		GetDirectoryCallbackTestParameters(string value)
	{
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.CreateDirectory(value));
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.CreateSymbolicLink(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.NullOrEmpty, "pathToTarget", directory
			=> directory.CreateSymbolicLink("foo", value));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.Delete(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.Delete(value, true));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(value, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(value, "foo", new EnumerationOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFiles(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFiles(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFiles(value, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFiles(value, "foo", new EnumerationOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(value, "foo",
				SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(value, "foo",
				new EnumerationOptions()));
#endif
		// `Directory.Exists` doesn't throw an exception on `null`
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetCreationTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetCreationTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetDirectories(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetDirectories(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetDirectories(value, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetDirectories(value, "foo", new EnumerationOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFiles(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFiles(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFiles(value, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFiles(value, "foo", new EnumerationOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(value, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(value, "foo", new EnumerationOptions()));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetLastAccessTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetLastAccessTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetLastWriteTime(value));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.GetLastWriteTimeUtc(value));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "path",
			directory
				=> directory.GetParent(value));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "sourceDirName",
			directory
				=> directory.Move(value, "foo"));
		yield return (ExceptionTestHelper.TestTypes.AllExceptInvalidPath, "destDirName",
			directory
				=> directory.Move("foo", value));
#if FEATURE_FILESYSTEM_LINK
		yield return (ExceptionTestHelper.TestTypes.All, "linkPath", directory
			=> directory.ResolveLinkTarget(value, false));
#endif
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetCreationTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetCreationTimeUtc(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetLastAccessTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetLastAccessTimeUtc(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetLastWriteTime(value, DateTime.Now));
		yield return (ExceptionTestHelper.TestTypes.All, "path", directory
			=> directory.SetLastWriteTimeUtc(value, DateTime.Now));
	}

	#endregion
}