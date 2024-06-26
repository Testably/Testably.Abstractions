﻿using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class RandomSystemExtensionsTests
{
	[Fact]
	public void GenerateRandomFileExtension_ShouldNotStartWithDotOrReturnEmptyString()
	{
		MockRandomSystem randomSystem = new(
			RandomProvider.Generate(
				intGenerator: RandomProvider.Generator.FromEnumerable(
					Enumerable.Range(0, 1000))));

		List<string> fileExtensions = [];
		while (true)
		{
			string fileExtension = randomSystem.Random.Shared.GenerateFileExtension();
			if (fileExtensions.Contains(fileExtension, StringComparer.Ordinal))
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
	public void GenerateRandomFileName_ShouldGenerateEdgeCases()
	{
		MockRandomSystem randomSystem = new(
			RandomProvider.Generate(
				intGenerator: RandomProvider.Generator.FromEnumerable(
					Enumerable.Range(0, 1000))));

		List<string> fileNames = [];
		while (true)
		{
			string fileName = randomSystem.Random.Shared.GenerateFileName();
			if (fileNames.Contains(fileName, StringComparer.Ordinal))
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
		fileNames.Should().Contain(d => d.StartsWith('.'));
	}
}
