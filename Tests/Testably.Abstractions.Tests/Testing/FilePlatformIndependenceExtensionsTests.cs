using System.IO;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests.Testing;

public class FilePlatformIndependenceExtensionsTests
{
    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void NormalizePath_Unix_RootedPath_ShouldRemoveDriveInfo(string part1)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string path = "C:/" + part1;
        string expectedPath = part1.PrefixRoot();
        path = path.NormalizePath();

        path.Should().Be(expectedPath);
    }

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void NormalizePath_Unix_ShouldReplaceAltDirectorySeparatorChar(
        string part1, string part2)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        char[] separatorChars =
        {
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
        };
        foreach (char separatorChar in separatorChars)
        {
            string path = part1 + separatorChar + part2;
            string expectedPath = part1 + Path.DirectorySeparatorChar + part2;
            path = path.NormalizePath();

            path.Should().Be(expectedPath);
        }
    }

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void NormalizePath_Windows_ShouldAlsoKeepAltDirectorySeparatorChar(
        string part1, string part2)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        char[] separatorChars =
        {
            Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
        };
        foreach (char separatorChar in separatorChars)
        {
            string path = part1 + separatorChar + part2;
            path = path.NormalizePath();

            path.Should().Be(path);
        }
    }

    [Fact]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void PrefixRoot_Null_ShouldReturnNull()
    {
        string? path = null;

        string? result = path.PrefixRoot();

        result.Should().BeNull();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void PrefixRoot_RootedPath_ShouldReturnPath(string path)
    {
        path = path.PrefixRoot();

        string result = path.PrefixRoot();

        result.Should().Be(path);
    }

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FilePlatformIndependenceExtensions))]
    public void PrefixRoot_UnRootedPath_ShouldPrefixRoot(string path)
    {
        string result = path.PrefixRoot();

        result.Should().NotBe(path);
        result.Should().EndWith(path);
    }
}