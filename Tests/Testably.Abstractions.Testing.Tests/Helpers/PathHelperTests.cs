using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class PathHelperTests
{
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

		bool result = path.IsUncPath();

		result.Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void IsUncPath_DirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.DirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath();

		result.Should().BeTrue();
	}

	[SkippableTheory]
	[AutoData]
	public void IsUncPath_MixedDirectorySeparatorChars_ShouldReturnFalse(string path)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Mac and Linux don't have distinctive directory separator chars.");

		path = $"{Path.AltDirectorySeparatorChar}{Path.DirectorySeparatorChar}{path}";

		bool result = path.IsUncPath();

		result.Should().BeFalse();
	}

	[Fact]
	public void IsUncPath_Null_ShouldReturnFalse()
	{
		string? path = null;

		bool result = path!.IsUncPath();

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

	[Theory]
	[AutoData]
	public void ThrowCommonExceptionsIfPathIsInvalid_WithInvalidCharacters(
		char[] invalidChars)
	{
		FileSystemMockForPath mockFileSystem = new(invalidChars);
		string path = invalidChars[0] + "foo";

		Exception? exception = Record.Exception(() =>
		{
			path.EnsureValidFormat(mockFileSystem);
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
		string sut = "path-with\0 invalid character";

		Exception? exception = Record.Exception(() =>
		{
			sut.ThrowCommonExceptionsIfPathToTargetIsInvalid(fileSystem);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.HResult.Should().Be(-2147024713);
	}

	private sealed class FileSystemMockForPath : IFileSystem
	{
		public FileSystemMockForPath(char[] invalidChars)
		{
			Path = new PathMockWithInvalidChars(invalidChars);
		}

		#region IFileSystem Members

		/// <inheritdoc />
		public IDirectory Directory
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IDirectoryInfoFactory DirectoryInfo
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IDriveInfoFactory DriveInfo
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IFile File
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IFileInfoFactory FileInfo
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IFileStreamFactory FileStream
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IFileSystemWatcherFactory FileSystemWatcher
			=> throw new NotSupportedException();

		/// <inheritdoc />
		public IPath Path { get; }

		#endregion

		private sealed class PathMockWithInvalidChars : IPath
		{
			private readonly char[] _invalidChars;

			public PathMockWithInvalidChars(char[] invalidChars)
			{
				_invalidChars = invalidChars;
			}

			#region IPath Members

			/// <inheritdoc />
			public char AltDirectorySeparatorChar
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public char DirectorySeparatorChar
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public IFileSystem FileSystem
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public char PathSeparator
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public char VolumeSeparatorChar
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string ChangeExtension(string? path, string? extension)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Combine(string path1, string path2)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Combine(string path1, string path2, string path3)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Combine(string path1, string path2, string path3, string path4)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Combine(params string[] paths)
				=> throw new NotSupportedException();

#if FEATURE_PATH_ADVANCED
			/// <inheritdoc />
			public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public bool EndsInDirectorySeparator(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_FILESYSTEM_NET7
			/// <inheritdoc />
			public bool Exists(string? path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_SPAN
			/// <inheritdoc />
			public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetDirectoryName(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			/// <inheritdoc />
			public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetExtension(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			/// <inheritdoc />
			public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetFileName(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			/// <inheritdoc />
			public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetFileNameWithoutExtension(string? path)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string GetFullPath(string path)
				=> path;

#if FEATURE_PATH_RELATIVE
			/// <inheritdoc />
			public string GetFullPath(string path, string basePath)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public char[] GetInvalidFileNameChars()
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public char[] GetInvalidPathChars()
				=> _invalidChars;

#if FEATURE_SPAN
			/// <inheritdoc />
			public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetPathRoot(string? path)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string GetRandomFileName()
				=> throw new NotSupportedException();

#if FEATURE_PATH_RELATIVE
			/// <inheritdoc />
			public string GetRelativePath(string relativeTo, string path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public string GetTempFileName()
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string GetTempPath()
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			/// <inheritdoc />
			public bool HasExtension(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public bool HasExtension(string? path)
				=> throw new NotSupportedException();

#if FEATURE_SPAN
			/// <inheritdoc />
			public bool IsPathFullyQualified(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_RELATIVE
			/// <inheritdoc />
			public bool IsPathFullyQualified(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_SPAN
			/// <inheritdoc />
			public bool IsPathRooted(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();
#endif

			/// <inheritdoc />
			public bool IsPathRooted(string? path)
				=> throw new NotSupportedException();

#if FEATURE_PATH_JOIN
			/// <inheritdoc />
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			/// <inheritdoc />
			public string Join(string? path1, string? path2)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Join(string? path1, string? path2, string? path3)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Join(params string?[] paths)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string Join(string? path1, string? path2, string? path3, string? path4)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_ADVANCED
			/// <inheritdoc />
			public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public string TrimEndingDirectorySeparator(string path)
				=> throw new NotSupportedException();
#endif

#if FEATURE_PATH_JOIN
			/// <inheritdoc />
			public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				Span<char> destination, out int charsWritten)
				=> throw new NotSupportedException();

			/// <inheritdoc />
			public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2,
				ReadOnlySpan<char> path3, Span<char> destination,
				out int charsWritten)
				=> throw new NotSupportedException();
#endif

			#endregion
		}
	}
}
