using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests
{
    #region Test Setup

    public IFileSystem FileSystem { get; }

    protected FileSystemPathTests(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    #endregion

    [Fact]
    public void AltDirectorySeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.AltDirectorySeparatorChar;

        result.Should().Be(Path.AltDirectorySeparatorChar);
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_EmptyPath_ShouldReturnEmptyString(string extension)
    {
        string result = FileSystem.Path.ChangeExtension(string.Empty, extension);

        result.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_NullPath_ShouldReturnNull(string extension)
    {
        string? result = FileSystem.Path.ChangeExtension(null, extension);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_WithDirectory_ShouldIncludeDirectory(
        string directory, string fileName, string extension)
    {
        string path = FileSystem.Path.Combine(directory, fileName + ".foo");
        string expectedResult =
            FileSystem.Path.Combine(directory, fileName + "." + extension);

        string result = FileSystem.Path.ChangeExtension(path, extension);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void ChangeExtension_WithLeadingDotInExtension_ShouldNotIncludeTwoDots(
        string fileName, string extension)
    {
        string path = fileName + ".foo";
        string expectedResult = fileName + "." + extension;

        string result = FileSystem.Path.ChangeExtension(path, "." + extension);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void Combine_2Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
        string path)
    {
        string result1 = FileSystem.Path.Combine(path, string.Empty);
        string result2 = FileSystem.Path.Combine(string.Empty, path);

        result1.Should().Be(path);
        result2.Should().Be(path);
    }

    [Theory]
    [AutoData]
    public void Combine_2Paths_OneNull_ShouldThrowArgumentNullException(string path)
    {
        Exception? exception1 = Record.Exception(() =>
            FileSystem.Path.Combine(path, null!));
        Exception? exception2 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, path));

        exception1.Should().BeAssignableTo<ArgumentNullException>();
        exception2.Should().BeAssignableTo<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public void Combine_2Paths_Rooted_ShouldReturnLastRootedPath(
        string path1, string path2)
    {
        path1 = Path.DirectorySeparatorChar + path1;
        path2 = Path.DirectorySeparatorChar + path2;

        string result = FileSystem.Path.Combine(path1, path2);

        result.Should().Be(path2);
    }

    [Theory]
    [AutoData]
    public void Combine_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2)
    {
        string expectedPath = path1
                              + FileSystem.Path.DirectorySeparatorChar + path2;

        string result = FileSystem.Path.Combine(path1, path2);

        result.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Combine_3Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
        string path1, string path2)
    {
        string expectedPath = Path.Combine(path1, path2);

        string result1 = FileSystem.Path.Combine(string.Empty, path1, path2);
        string result2 = FileSystem.Path.Combine(path1, string.Empty, path2);
        string result3 = FileSystem.Path.Combine(path1, path2, string.Empty);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Combine_3Paths_OneNull_ShouldThrowArgumentNullException(string path)
    {
        Exception? exception1 = Record.Exception(() =>
            FileSystem.Path.Combine(path, null!, null!));
        Exception? exception2 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, path, null!));
        Exception? exception3 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, path));

        exception1.Should().BeAssignableTo<ArgumentNullException>();
        exception2.Should().BeAssignableTo<ArgumentNullException>();
        exception3.Should().BeAssignableTo<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public void Combine_3Paths_Rooted_ShouldReturnLastRootedPath(
        string path1, string path2, string path3)
    {
        path1 = Path.AltDirectorySeparatorChar + path1;
        path2 = Path.AltDirectorySeparatorChar + path2;
        path3 = Path.AltDirectorySeparatorChar + path3;

        string result = FileSystem.Path.Combine(path1, path2, path3);

        result.Should().Be(path3);
    }

    [Theory]
    [AutoData]
    public void Combine_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3)
    {
        string expectedPath = path1
                              + FileSystem.Path.DirectorySeparatorChar + path2
                              + FileSystem.Path.DirectorySeparatorChar + path3;

        string result = FileSystem.Path.Combine(path1, path2, path3);

        result.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Combine_4Paths_OneEmpty_ShouldReturnCombinationOfOtherParts(
        string path1, string path2, string path3)
    {
        string expectedPath = Path.Combine(path1, path2, path3);

        string result1 = FileSystem.Path.Combine(string.Empty, path1, path2, path3);
        string result2 = FileSystem.Path.Combine(path1, string.Empty, path2, path3);
        string result3 = FileSystem.Path.Combine(path1, path2, string.Empty, path3);
        string result4 = FileSystem.Path.Combine(path1, path2, path3, string.Empty);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
        result4.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Combine_4Paths_OneNull_ShouldThrowArgumentNullException(string path)
    {
        Exception? exception1 = Record.Exception(() =>
            FileSystem.Path.Combine(path, null!, null!, null!));
        Exception? exception2 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, path, null!, null!));
        Exception? exception3 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, path, null!));
        Exception? exception4 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, null!, path));

        exception1.Should().BeAssignableTo<ArgumentNullException>();
        exception2.Should().BeAssignableTo<ArgumentNullException>();
        exception3.Should().BeAssignableTo<ArgumentNullException>();
        exception4.Should().BeAssignableTo<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public void Combine_4Paths_Rooted_ShouldReturnLastRootedPath(
        string path1, string path2, string path3, string path4)
    {
        path1 = Path.DirectorySeparatorChar + path1;
        path2 = Path.DirectorySeparatorChar + path2;
        path3 = Path.DirectorySeparatorChar + path3;
        path4 = Path.DirectorySeparatorChar + path4;

        string result = FileSystem.Path.Combine(path1, path2, path3, path4);

        result.Should().Be(path4);
    }

    [Theory]
    [AutoData]
    public void Combine_4Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3, string path4)
    {
        string expectedPath = path1
                              + FileSystem.Path.DirectorySeparatorChar + path2
                              + FileSystem.Path.DirectorySeparatorChar + path3
                              + FileSystem.Path.DirectorySeparatorChar + path4;

        string result = FileSystem.Path.Combine(path1, path2, path3, path4);

        result.Should().Be(expectedPath);
    }

    [Fact]
    public void Combine_ParamPaths_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
            FileSystem.Path.Combine(null!));

        exception.Should().BeAssignableTo<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public void Combine_ParamPaths_OneEmpty_ShouldReturnCombinationOfOtherParts(
        string path1, string path2, string path3, string path4)
    {
        string expectedPath = Path.Combine(path1, path2, path3, path4);

        string result1 =
            FileSystem.Path.Combine(string.Empty, path1, path2, path3, path4);
        string result2 =
            FileSystem.Path.Combine(path1, string.Empty, path2, path3, path4);
        string result3 =
            FileSystem.Path.Combine(path1, path2, string.Empty, path3, path4);
        string result4 =
            FileSystem.Path.Combine(path1, path2, path3, string.Empty, path4);
        string result5 =
            FileSystem.Path.Combine(path1, path2, path3, path4, string.Empty);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
        result4.Should().Be(expectedPath);
        result5.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Combine_ParamPaths_OneNull_ShouldThrowArgumentNullException(
        string path)
    {
        Exception? exception1 = Record.Exception(() =>
            FileSystem.Path.Combine(path, null!, null!, null!, null!));
        Exception? exception2 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, path, null!, null!, null!));
        Exception? exception3 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, path, null!, null!));
        Exception? exception4 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, null!, path, null!));
        Exception? exception5 = Record.Exception(() =>
            FileSystem.Path.Combine(null!, null!, null!, null!, path));

        exception1.Should().BeAssignableTo<ArgumentNullException>();
        exception2.Should().BeAssignableTo<ArgumentNullException>();
        exception3.Should().BeAssignableTo<ArgumentNullException>();
        exception4.Should().BeAssignableTo<ArgumentNullException>();
        exception5.Should().BeAssignableTo<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    public void Combine_ParamPaths_Rooted_ShouldReturnLastRootedPath(
        string path1, string path2, string path3, string path4, string path5)
    {
        path1 = Path.DirectorySeparatorChar + path1;
        path2 = Path.DirectorySeparatorChar + path2;
        path3 = Path.DirectorySeparatorChar + path3;
        path4 = Path.DirectorySeparatorChar + path4;
        path5 = Path.DirectorySeparatorChar + path5;

        string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

        result.Should().Be(path5);
    }

    [Theory]
    [AutoData]
    public void Combine_ParamPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3, string path4, string path5)
    {
        string expectedPath = path1
                              + FileSystem.Path.DirectorySeparatorChar + path2
                              + FileSystem.Path.DirectorySeparatorChar + path3
                              + FileSystem.Path.DirectorySeparatorChar + path4
                              + FileSystem.Path.DirectorySeparatorChar + path5;

        string result = FileSystem.Path.Combine(path1, path2, path3, path4, path5);

        result.Should().Be(expectedPath);
    }

    [Fact]
    public void DirectorySeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.DirectorySeparatorChar;

        result.Should().Be(Path.DirectorySeparatorChar);
    }

    [Fact]
    public void GetDirectoryName_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetDirectoryName(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetDirectoryName_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string? result = FileSystem.Path.GetDirectoryName(path);

        result.Should().Be(directory);
    }

    [Fact]
    public void GetExtension_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetExtension(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetExtension_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetExtension(path);

        result.Should().Be("." + extension);
    }

    [Fact]
    public void GetFileName_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetFileName(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetFileName_ShouldReturnDirectory(string directory, string filename,
                                                  string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetFileName(path);

        result.Should().Be(filename + "." + extension);
    }

    [Fact]
    public void GetFileNameWithoutExtension_Null_ShouldReturnNull()
    {
        string? result = FileSystem.Path.GetFileNameWithoutExtension(null);

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public void GetFileNameWithoutExtension_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        string result = FileSystem.Path.GetFileNameWithoutExtension(path);

        result.Should().Be(filename);
    }

    [Fact]
    public void GetRandomFileName_ShouldReturnRandomFileNameWithExtension()
    {
        string result = FileSystem.Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
        FileSystem.Path.IsPathFullyQualified(result)
           .Should().BeFalse();
#endif
        FileSystem.Path.GetExtension(result)
           .Should().NotBeEmpty();
        FileSystem.Path.GetFileNameWithoutExtension(result)
           .Should().NotBeEmpty();
    }

    [Fact]
    public void GetRandomFileName_ShouldReturnRandomStrings()
    {
        ConcurrentBag<string> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(FileSystem.Path.GetRandomFileName());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HasExtension_Null_ShouldReturnFalse()
    {
        bool result = FileSystem.Path.HasExtension(null);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineAutoData(".foo", true)]
    [InlineAutoData(".abc.xyz", true)]
    [InlineAutoData("foo", false)]
    [InlineAutoData(".", false)]
    public void HasExtension_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void PathSeparator_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.PathSeparator;

        result.Should().Be(Path.PathSeparator);
    }

    [Fact]
    public void VolumeSeparatorChar_ShouldReturnDefaultValue()
    {
        char result = FileSystem.Path.VolumeSeparatorChar;

        result.Should().Be(Path.VolumeSeparatorChar);
    }
#if FEATURE_SPAN
    [Theory]
    [AutoData]
    public void GetDirectoryName_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetDirectoryName(path.AsSpan());

        result.ToString().Should().Be(directory);
    }

    [Theory]
    [AutoData]
    public void GetExtension_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetExtension(path.AsSpan());

        result.ToString().Should().Be("." + extension);
    }

    [Theory]
    [AutoData]
    public void GetFileName_Span_ShouldReturnDirectory(
        string directory, string filename,
        string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result = FileSystem.Path.GetFileName(path.AsSpan());

        result.ToString().Should().Be(filename + "." + extension);
    }

    [Theory]
    [AutoData]
    public void GetFileNameWithoutExtension_Span_ShouldReturnDirectory(
        string directory, string filename, string extension)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar + filename +
                      "." + extension;

        ReadOnlySpan<char> result =
            FileSystem.Path.GetFileNameWithoutExtension(path.AsSpan());

        result.ToString().Should().Be(filename);
    }

    [Theory]
    [InlineAutoData(".foo", true)]
    [InlineAutoData(".abc.xyz", true)]
    [InlineAutoData("foo", false)]
    [InlineAutoData(".", false)]
    public void HasExtension_Span_ShouldReturnExpectedResult(
        string suffix, bool expectedResult, string filename)
    {
        string path = filename + suffix;

        bool result = FileSystem.Path.HasExtension(path.AsSpan());

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_Span_PrefixedRoot_ShouldReturnTrue(
        string directory)
    {
        string path = directory.PrefixRoot();
        bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_Span_WithoutPrefixedRoot_ShouldReturnFalse(
        string path)
    {
        bool result = FileSystem.Path.IsPathFullyQualified(path.AsSpan());

        result.Should().BeFalse();
    }
#endif
#if FEATURE_PATH_RELATIVE
    [Theory]
    [AutoData]
    public void GetRelativePath_CommonParentDirectory_ShouldReturnRelativePath(
        string baseDirectory, string directory1, string directory2)
    {
        string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
        string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
        string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
        string result = FileSystem.Path.GetRelativePath(path1, path2);

        result.Should().Be(expectedRelativePath);
    }

    [Theory]
    [AutoData]
    public void GetRelativePath_RootedPath_ShouldReturnAbsolutePath(
        string baseDirectory, string directory1, string directory2)
    {
        baseDirectory = baseDirectory.PrefixRoot();
        string path1 = FileSystem.Path.Combine(baseDirectory, directory1);
        string path2 = FileSystem.Path.Combine(baseDirectory, directory2);
        string expectedRelativePath = FileSystem.Path.Combine("..", directory2);
        string result = FileSystem.Path.GetRelativePath(path1, path2);

        result.Should().Be(expectedRelativePath);
    }

    [Theory]
    [AutoData]
    public void GetRelativePath_DifferentDrives_ShouldReturnAbsolutePath(
        string path1, string path2)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Different drives are only supported on Windows
            return;
        }

        path1 = path1.PrefixRoot('A');
        path2 = path2.PrefixRoot('B');
        string result = FileSystem.Path.GetRelativePath(path1, path2);

        result.Should().Be(path2);
    }

    [Theory]
    [AutoData]
    public void GetRelativePath_ToItself_ShouldReturnDot(string path)
    {
        string expectedResult = ".";

        string result = FileSystem.Path.GetRelativePath(path, path);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_PrefixedRoot_ShouldReturnTrue(
        string directory)
    {
        string path = directory.PrefixRoot();
        bool result = FileSystem.Path.IsPathFullyQualified(path);

        result.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    public void IsPathFullyQualified_WithoutPrefixedRoot_ShouldReturnFalse(
        string path)
    {
        bool result = FileSystem.Path.IsPathFullyQualified(path);

        result.Should().BeFalse();
    }
#endif

#if FEATURE_PATH_ADVANCED
    [Fact]
    public void EndsInDirectorySeparator_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void EndsInDirectorySeparator_Null_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(null!);

        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void
        EndsInDirectorySeparator_WithTrailingDirectorySeparator_ShouldReturnTrue(
            string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    public void
        EndsInDirectorySeparator_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path);

        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void TrimEndingDirectorySeparator_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(directory);
    }

    [Theory]
    [AutoData]
    public void
        TrimEndingDirectorySeparator_WithoutDirectoryChar_ShouldReturnUnchanged(
            string path)
    {
        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }

    [Fact]
    public void TrimEndingDirectorySeparator_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        string result = FileSystem.Path.TrimEndingDirectorySeparator(path);

        result.Should().Be(path);
    }

    [Theory]
    [AutoData]
    public void TrimEndingDirectorySeparator_Span_DirectoryChar_ShouldTrim(
        string directory)
    {
        string path = directory + FileSystem.Path.DirectorySeparatorChar;

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(directory);
    }

    [Theory]
    [AutoData]
    public void
        TrimEndingDirectorySeparator_Span_WithoutDirectoryChar_ShouldReturnUnchanged(
            string path)
    {
        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(path);
    }

    [Fact]
    public void TrimEndingDirectorySeparator_Span_Root_ShouldReturnUnchanged()
    {
        string path = string.Empty.PrefixRoot();

        ReadOnlySpan<char> result =
            FileSystem.Path.TrimEndingDirectorySeparator(path.AsSpan());

        result.ToString().Should().Be(path);
    }

    [Fact]
    public void EndsInDirectorySeparator_Span_Empty_ShouldReturnExpectedResult()
    {
        bool result = FileSystem.Path.EndsInDirectorySeparator(string.Empty.AsSpan());

        result.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void
        EndsInDirectorySeparator_Span_WithTrailingDirectorySeparator_ShouldReturnTrue(
            string path)
    {
        path += FileSystem.Path.DirectorySeparatorChar;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeTrue();
    }

    [Theory]
    [InlineAutoData('.')]
    [InlineAutoData('a')]
    public void
        EndsInDirectorySeparator_Span_WithoutTrailingDirectorySeparator_ShouldReturnFalse(
            char lastCharacter, string path)
    {
        path += lastCharacter;

        bool result = FileSystem.Path.EndsInDirectorySeparator(path.AsSpan());

        result.Should().BeFalse();
    }
#endif

#if FEATURE_PATH_JOIN
    [Theory]
    [InlineAutoData((string?)null)]
    [InlineAutoData("")]
    public void Join_2Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
        string? missingPath, string? path)
    {
        string result1 = FileSystem.Path.Join(path, missingPath);
        string result2 = FileSystem.Path.Join(missingPath, path);

        result1.Should().Be(path);
        result2.Should().Be(path);
    }

    [Theory]
    [AutoData]
    public void Join_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2;

        string result = FileSystem.Path.Join(path1, path2);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void Join_2Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2;

        string result = FileSystem.Path.Join(
            path1.AsSpan(),
            path2.AsSpan());

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineAutoData((string?)null)]
    [InlineAutoData("")]
    public void Join_3Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
        string? missingPath, string path1, string path2)
    {
        string expectedPath = Path.Join(path1, path2);

        string result1 = FileSystem.Path.Join(missingPath, path1, path2);
        string result2 = FileSystem.Path.Join(path1, missingPath, path2);
        string result3 = FileSystem.Path.Join(path1, path2, missingPath);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Join_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3;

        string result = FileSystem.Path.Join(path1, path2, path3);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void Join_3Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3;

        string result = FileSystem.Path.Join(
            path1.AsSpan(),
            path2.AsSpan(),
            path3.AsSpan());

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineAutoData((string?)null)]
    [InlineAutoData("")]
    public void Join_4Paths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
        string? missingPath, string path1, string path2, string path3)
    {
        string expectedPath = Path.Join(path1, path2, path3);

        string result1 = FileSystem.Path.Join(missingPath, path1, path2, path3);
        string result2 = FileSystem.Path.Join(path1, missingPath, path2, path3);
        string result3 = FileSystem.Path.Join(path1, path2, missingPath, path3);
        string result4 = FileSystem.Path.Join(path1, path2, path3, missingPath);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
        result4.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Join_4Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3, string path4)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3
                                + FileSystem.Path.DirectorySeparatorChar + path4;

        string result = FileSystem.Path.Join(path1, path2, path3, path4);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void Join_4Paths_Span_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3, string path4)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3
                                + FileSystem.Path.DirectorySeparatorChar + path4;

        string result = FileSystem.Path.Join(
            path1.AsSpan(),
            path2.AsSpan(),
            path3.AsSpan(),
            path4.AsSpan());

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineAutoData((string?)null)]
    [InlineAutoData("")]
    public void Join_ParamPaths_OneNullOrEmpty_ShouldReturnCombinationOfOtherParts(
        string? missingPath, string path1, string path2, string path3, string path4)
    {
        string expectedPath = Path.Join(path1, path2, path3, path4);

        string result1 =
            FileSystem.Path.Join(missingPath, path1, path2, path3, path4);
        string result2 =
            FileSystem.Path.Join(path1, missingPath, path2, path3, path4);
        string result3 =
            FileSystem.Path.Join(path1, path2, missingPath, path3, path4);
        string result4 =
            FileSystem.Path.Join(path1, path2, path3, missingPath, path4);
        string result5 =
            FileSystem.Path.Join(path1, path2, path3, path4, missingPath);

        result1.Should().Be(expectedPath);
        result2.Should().Be(expectedPath);
        result3.Should().Be(expectedPath);
        result4.Should().Be(expectedPath);
        result5.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    public void Join_ParamPaths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3, string path4, string path5)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3
                                + FileSystem.Path.DirectorySeparatorChar + path4
                                + FileSystem.Path.DirectorySeparatorChar + path5;

        string result = FileSystem.Path.Join(path1, path2, path3, path4, path5);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void TryJoin_2Paths_BufferTooLittle_ShouldReturnFalse(
        string path1, string path2)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2;

        char[] buffer = new char[expectedResult.Length - 1];
        Span<char> destination = new(buffer);

        bool result = FileSystem.Path.TryJoin(
            path1.AsSpan(),
            path2.AsSpan(),
            destination,
            out int charsWritten);

        result.Should().BeFalse();
        charsWritten.Should().Be(0);
    }

    [Theory]
    [AutoData]
    public void TryJoin_2Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2;

        char[] buffer = new char[expectedResult.Length + 10];
        Span<char> destination = new(buffer);

        bool result = FileSystem.Path.TryJoin(
            path1.AsSpan(),
            path2.AsSpan(),
            destination,
            out int charsWritten);

        result.Should().BeTrue();
        charsWritten.Should().Be(expectedResult.Length);
        destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
    }

    [Theory]
    [AutoData]
    public void TryJoin_3Paths_BufferTooLittle_ShouldReturnFalse(
        string path1, string path2, string path3)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3;

        char[] buffer = new char[expectedResult.Length - 1];
        Span<char> destination = new(buffer);

        bool result = FileSystem.Path.TryJoin(
            path1.AsSpan(),
            path2.AsSpan(),
            path3.AsSpan(),
            destination,
            out int charsWritten);

        result.Should().BeFalse();
        charsWritten.Should().Be(0);
    }

    [Theory]
    [AutoData]
    public void TryJoin_3Paths_ShouldReturnPathsCombinedByDirectorySeparatorChar(
        string path1, string path2, string path3)
    {
        string expectedResult = path1
                                + FileSystem.Path.DirectorySeparatorChar + path2
                                + FileSystem.Path.DirectorySeparatorChar + path3;

        char[] buffer = new char[expectedResult.Length + 10];
        Span<char> destination = new(buffer);

        bool result = FileSystem.Path.TryJoin(
            path1.AsSpan(),
            path2.AsSpan(),
            path3.AsSpan(),
            destination,
            out int charsWritten);

        result.Should().BeTrue();
        charsWritten.Should().Be(expectedResult.Length);
        destination.Slice(0, charsWritten).ToString().Should().Be(expectedResult);
    }
#endif
}