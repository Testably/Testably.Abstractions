using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Tests.Testing;

public class RandomSystemExtensionsTests
{
    [Fact]
    [Trait(nameof(Testing), nameof(RandomSystemExtensions))]
    public void GenerateRandomFileExtension_ShouldNotStartWithDotOrReturnEmptyString()
    {
        RandomSystemMock randomSystem = new(
            RandomProvider.Generate(
                intGenerator: RandomProvider.Generator<int>.FromEnumerable(
                    Enumerable.Range(0, 1000))));

        List<string> fileExtensions = new();
        while (true)
        {
            string fileExtension = randomSystem.GenerateFileExtension();
            if (fileExtensions.Contains(fileExtension))
            {
                break;
            }

            fileExtensions.Add(fileExtension);
        }

        fileExtensions.Count.Should().BeGreaterThan(5);
        foreach (string fileExtension in fileExtensions)
        {
            fileExtension.Should().NotBeNullOrWhiteSpace();
            fileExtension.Should().NotStartWith(".");
        }
    }

    [Fact]
    [Trait(nameof(Testing), nameof(RandomSystemExtensions))]
    public void GenerateRandomFileName_ShouldGenerateEdgeCases()
    {
        RandomSystemMock randomSystem = new(
            RandomProvider.Generate(
                intGenerator: RandomProvider.Generator<int>.FromEnumerable(
                    Enumerable.Range(0, 1000))));

        List<string> fileNames = new();
        while (true)
        {
            string fileName = randomSystem.GenerateFileName();
            if (fileNames.Contains(fileName))
            {
                break;
            }

            fileNames.Add(fileName);
        }

        foreach (string fileName in fileNames)
        {
            fileName.Should().NotBeNullOrWhiteSpace();
        }

        // Check edge cases for directories
        fileNames.Should().Contain(d => d.Contains(' '));
        fileNames.Should().Contain(d => d.Length == 1);
        fileNames.Should().Contain(d => d.StartsWith("."));
    }
}