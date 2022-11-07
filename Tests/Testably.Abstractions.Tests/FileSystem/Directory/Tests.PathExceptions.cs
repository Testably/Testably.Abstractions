using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class Tests<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public static IEnumerable<object[]> GetDirectoryCallbacks(string? path)
		=> GetDirectoryCallbackTestParameters(path!)
		   .Select(callback => new object[] { callback });

	#endregion

	[Theory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: "")]
	public void Operations_ShouldThrowArgumentExceptionIfPathIsEmpty(
		Expression<Action<IDirectory>> callback)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		if (!Test.IsNetFramework)
		{
			exception.Should().BeOfType<ArgumentException>()
			   .Which.ParamName.Should().Be("path");
		}

		exception.Should().BeOfType<ArgumentException>()
		   .Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[MemberData(nameof(GetDirectoryCallbacks), parameters: (string?)null)]
	public void Operations_ShouldThrowArgumentNullExceptionIfPathIsNull(
		Expression<Action<IDirectory>> callback)
	{
		Exception? exception = Record.Exception(() =>
		{
			callback.Compile().Invoke(FileSystem.Directory);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("path");
	}

	#region Helpers

	private static IEnumerable<Expression<Action<IDirectory>>>
		GetDirectoryCallbackTestParameters(string path)
	{
		yield return directory
			=> directory.CreateDirectory(path);
#if FEATURE_FILESYSTEM_LINK
		yield return directory
			=> directory.CreateSymbolicLink(path, "foo");
#endif
		yield return directory
			=> directory.Delete(path);
		yield return directory
			=> directory.Delete(path, true);
		yield return directory
			=> directory.EnumerateDirectories(path);
		yield return directory
			=> directory.EnumerateDirectories(path, "foo");
		yield return directory
			=> directory.EnumerateDirectories(path, "foo", SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.EnumerateDirectories(path, "foo", new EnumerationOptions());
#endif
		yield return directory
			=> directory.EnumerateFiles(path);
		yield return directory
			=> directory.EnumerateFiles(path, "foo");
		yield return directory
			=> directory.EnumerateFiles(path, "foo", SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.EnumerateFiles(path, "foo", new EnumerationOptions());
#endif
		yield return directory
			=> directory.EnumerateFileSystemEntries(path);
		yield return directory
			=> directory.EnumerateFileSystemEntries(path, "foo");
		yield return directory
			=> directory.EnumerateFileSystemEntries(path, "foo",
				SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.EnumerateFileSystemEntries(path, "foo",
				new EnumerationOptions());
#endif
		// `Directory.Exists` doesn't throw an exception on `null`
		yield return directory
			=> directory.GetCreationTime(path);
		yield return directory
			=> directory.GetCreationTimeUtc(path);
		yield return directory
			=> directory.GetDirectories(path);
		yield return directory
			=> directory.GetDirectories(path, "foo");
		yield return directory
			=> directory.GetDirectories(path, "foo", SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.GetDirectories(path, "foo", new EnumerationOptions());
#endif
		yield return directory
			=> directory.GetFiles(path);
		yield return directory
			=> directory.GetFiles(path, "foo");
		yield return directory
			=> directory.GetFiles(path, "foo", SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.GetFiles(path, "foo", new EnumerationOptions());
#endif
		yield return directory
			=> directory.GetFileSystemEntries(path);
		yield return directory
			=> directory.GetFileSystemEntries(path, "foo");
		yield return directory
			=> directory.GetFileSystemEntries(path, "foo", SearchOption.AllDirectories);
#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		yield return directory
			=> directory.GetFileSystemEntries(path, "foo", new EnumerationOptions());
#endif
		yield return directory
			=> directory.GetLastAccessTime(path);
		yield return directory
			=> directory.GetLastAccessTimeUtc(path);
		yield return directory
			=> directory.GetLastWriteTime(path);
		yield return directory
			=> directory.GetLastWriteTimeUtc(path);
		yield return directory
			=> directory.GetParent(path);
		yield return directory
			=> directory.SetCreationTime(path, DateTime.Now);
		yield return directory
			=> directory.SetCreationTimeUtc(path, DateTime.Now);
		yield return directory
			=> directory.SetLastAccessTime(path, DateTime.Now);
		yield return directory
			=> directory.SetLastAccessTimeUtc(path, DateTime.Now);
		yield return directory
			=> directory.SetLastWriteTime(path, DateTime.Now);
		yield return directory
			=> directory.SetLastWriteTimeUtc(path, DateTime.Now);
	}

	#endregion
}