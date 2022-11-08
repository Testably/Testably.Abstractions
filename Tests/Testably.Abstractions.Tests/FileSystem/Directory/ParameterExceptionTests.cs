using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ParameterExceptionTests<TFileSystem>
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
		Operations_ShouldThrowArgumentExceptionIfPathContainsIllegalCharactersOnWindows(
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
		   .Where(item => item.TestType.HasFlag(ToTestType(path)))
		   .Select(item => new object[] { item.Callback, item.ParamName });

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

	private static IEnumerable<(TestTypes TestType, string ParamName,
			Expression<Action<IDirectory>> Callback)>
		GetDirectoryCallbackTestParameters(string path)
	{
		yield return (TestTypes.All, "path", directory
			=> directory.CreateDirectory(path));
#if FEATURE_FILESYSTEM_LINK
		yield return (TestTypes.All, "path", directory
			=> directory.CreateSymbolicLink(path, "foo"));
		yield return (TestTypes.Null | TestTypes.Empty, "pathToTarget", directory
			=> directory.CreateSymbolicLink("foo", path));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.Delete(path));
		yield return (TestTypes.All, "path", directory
			=> directory.Delete(path, true));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(path));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(path, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateDirectories(path, "foo", new EnumerationOptions()));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFiles(path));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFiles(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFiles(path, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFiles(path, "foo", new EnumerationOptions()));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(path));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(path, "foo",
				SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.EnumerateFileSystemEntries(path, "foo",
				new EnumerationOptions()));
#endif
		// `Directory.Exists` doesn't throw an exception on `null`
		yield return (TestTypes.All, "path", directory
			=> directory.GetCreationTime(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetCreationTimeUtc(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetDirectories(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetDirectories(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.GetDirectories(path, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.GetDirectories(path, "foo", new EnumerationOptions()));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.GetFiles(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetFiles(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.GetFiles(path, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.GetFiles(path, "foo", new EnumerationOptions()));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(path, "foo"));
		yield return (TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(path, "foo", SearchOption.AllDirectories));
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return (TestTypes.All, "path", directory
			=> directory.GetFileSystemEntries(path, "foo", new EnumerationOptions()));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.GetLastAccessTime(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetLastAccessTimeUtc(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetLastWriteTime(path));
		yield return (TestTypes.All, "path", directory
			=> directory.GetLastWriteTimeUtc(path));
		yield return (TestTypes.Null | TestTypes.Empty, "path", directory
			=> directory.GetParent(path));
		yield return (TestTypes.Null | TestTypes.Empty, "sourceDirName", directory
			=> directory.Move(path, "foo"));
		yield return (TestTypes.Null | TestTypes.Empty, "destDirName", directory
			=> directory.Move("foo", path));
#if FEATURE_FILESYSTEM_LINK
		yield return (TestTypes.All, "linkPath", directory
			=> directory.ResolveLinkTarget(path, false));
#endif
		yield return (TestTypes.All, "path", directory
			=> directory.SetCreationTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", directory
			=> directory.SetCreationTimeUtc(path, DateTime.Now));
		yield return (TestTypes.All, "path", directory
			=> directory.SetLastAccessTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", directory
			=> directory.SetLastAccessTimeUtc(path, DateTime.Now));
		yield return (TestTypes.All, "path", directory
			=> directory.SetLastWriteTime(path, DateTime.Now));
		yield return (TestTypes.All, "path", directory
			=> directory.SetLastWriteTimeUtc(path, DateTime.Now));
	}

	#endregion
}