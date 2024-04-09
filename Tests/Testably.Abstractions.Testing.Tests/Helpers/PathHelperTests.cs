using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class PathHelperTests
{
	[Fact]
	public void
		EnsureValidFormat_WithWhiteSpaceAndIncludeIsEmptyCheck_ShouldThrowArgumentException()
	{
		string whiteSpace = " ";
		MockFileSystem fileSystem = new();
		Exception? exception = Record.Exception(() =>
		{
			whiteSpace.EnsureValidFormat(fileSystem, "foo", true);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024809);
	}

	[Theory]
	[AutoData]
	public void GetFullPathOrWhiteSpace_NormalPath_ShouldReturnFullPath(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize();
		string expectedPath = fileSystem.Path.GetFullPath(path);

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be(expectedPath);
	}

	[Fact]
	public void GetFullPathOrWhiteSpace_Null_ShouldReturnEmptyString()
	{
		MockFileSystem fileSystem = new();
		string? sut = null;

		string result = sut.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be("");
	}

	[Theory]
	[InlineData("  ")]
	[InlineData("\t")]
	public void GetFullPathOrWhiteSpace_WhiteSpace_ShouldReturnPath(string path)
	{
		MockFileSystem fileSystem = new();

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		result.Should().Be(path);
	}

	[Theory]
	[AutoData]
	public void IsUncPath_AltDirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.AltDirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void IsUncPath_DirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.DirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsUncPath_MixedDirectorySeparatorChars_ShouldReturnFalse(string path)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Mac and Linux don't have distinctive directory separator chars.");

		path = $"{Path.AltDirectorySeparatorChar}{Path.DirectorySeparatorChar}{path}";

		bool result = path.IsUncPath(new MockFileSystem());

		result.Should().BeFalse();
	}

	[Fact]
	public void IsUncPath_Null_ShouldReturnFalse()
	{
		string? path = null;

		bool result = path!.IsUncPath(new MockFileSystem());

		result.Should().BeFalse();
	}

	[Fact]
	public void
		ThrowCommonExceptionsIfPathIsInvalid_StartWithNull_ShouldThrowArgumentException()
	{
		string path = "\0foo";

		Exception? exception = Record.Exception(() =>
		{
			path.EnsureValidFormat(new MockFileSystem());
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.Message.Should().Contain($"'{path}'");
	}

	[SkippableTheory]
	[AutoData]
	public void ThrowCommonExceptionsIfPathIsInvalid_WithInvalidCharacters(
		char[] invalidChars)
	{
		// TODO: Enable this test again when the Execute method in MockFileSystem is writable
		Skip.If(true, "Check how to update this test");
		_ = new FileSystemMockForPath(invalidChars);
		string path = invalidChars[0] + "foo";

		Exception exception = Record.Exception(() =>
		{
			path.EnsureValidFormat(null!);
		});

#if NETFRAMEWORK
		exception.Should().BeOfType<ArgumentException>()
			.Which.Message.Should().Contain($"'{path}'");
#else
		exception.Should().BeOfType<IOException>()
			.Which.Message.Should().Contain($"'{path}'");
#endif
	}

	[Fact]
	public void
		ThrowCommonExceptionsIfPathToTargetIsInvalid_NullCharacter_ShouldThrowArgumentException()
	{
		MockFileSystem fileSystem = new();
		string path = "path-with\0 invalid character";

		Exception? exception = Record.Exception(() =>
		{
			path.ThrowCommonExceptionsIfPathToTargetIsInvalid(fileSystem);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.Message.Should().Contain($"'{path}'");
	}

	private sealed class FileSystemMockForPath(char[] invalidChars) : IFileSystem
	{
		#region IFileSystem Members

		public IDirectory Directory
			=> throw new NotSupportedException();

		public IDirectoryInfoFactory DirectoryInfo
			=> throw new NotSupportedException();

		public IDriveInfoFactory DriveInfo
			=> throw new NotSupportedException();

		public IFile File
			=> throw new NotSupportedException();

		public IFileInfoFactory FileInfo
			=> throw new NotSupportedException();

		public IFileStreamFactory FileStream
			=> throw new NotSupportedException();

		public IFileSystemWatcherFactory FileSystemWatcher
			=> throw new NotSupportedException();

		public IPath Path { get; } = new PathMockWithInvalidChars(invalidChars);

		#endregion

		private sealed class PathMockWithInvalidChars(char[] invalidChars) : IPath
		{
			#region IPath Members

			public char AltDirectorySeparatorChar
				=> throw new NotSupportedException();

			public char DirectorySeparatorChar
				=> throw new NotSupportedException();

			public IFileSystem FileSystem
				=> throw new NotSupportedException();

			public char PathSeparator
				=> throw new NotSupportedException();

			public char VolumeSeparatorChar
				=> throw new NotSupportedException();

			public string ChangeExtension(string? path, string? extension)
				=> throw new NotSupportedException();

			public string Combine(string path1, string path2)
				=> throw new NotSupportedException();

			public string Combine(string path1, string path2, string path3)
				=> throw new NotSupportedException();

			public string Combine(string path1, string path2, string path3, string path4)
				=> throw new NotSupportedException();

			public string Combine(params string[] paths)
				=> throw new NotSupportedException();

#if FEATURE_PATH_ADVANCED
			public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public bool EndsInDirectorySeparator(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_FILESYSTEM_NET7
			public bool Exists(string? path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_SPAN
			public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public string GetDirectoryName(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public string GetExtension(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public string GetFileName(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public string GetFileNameWithoutExtension(string? path)
				=> throw new NotSupportedException();

			public string GetFullPath(string path)
				=> path;

#if FEATURE_PATH_RELATIVE
			public string GetFullPath(string path, string basePath)
				=> throw new NotSupportedException();
#endif

			public char[] GetInvalidFileNameChars()
				=> throw new NotSupportedException();

			public char[] GetInvalidPathChars()
				=> invalidChars;

#if FEATURE_SPAN
			public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public string GetPathRoot(string? path)
				=> throw new NotSupportedException();

			public string GetRandomFileName()
				=> throw new NotSupportedException();

#if FEATURE_PATH_RELATIVE
			public string GetRelativePath(string relativeTo, string path)
				=> throw new NotSupportedException();
#endif

			public string GetTempFileName()
				=> throw new NotSupportedException();

			public string GetTempPath()
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			public bool HasExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public bool HasExtension(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			public bool IsPathFullyQualified(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_RELATIVE
			public bool IsPathFullyQualified(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_SPAN
			public bool IsPathRooted(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			public bool IsPathRooted(string? path)
				=> throw new NotSupportedException();

#if FEATURE_PATH_JOIN
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_JOIN
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string Join(string? path1, string? path2)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string Join(string? path1, string? path2, string? path3)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string Join(params string?[] paths)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string Join(string? path1, string? path2, string? path3, string? path4)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			public string TrimEndingDirectorySeparator(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_JOIN
			public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				Span<char> destination, out int charsWritten)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_JOIN
			public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3, Span<char> destination,
				out int charsWritten)
				=> throw new NotSupportedException();
#endif

			#endregion
		}
	}
}
