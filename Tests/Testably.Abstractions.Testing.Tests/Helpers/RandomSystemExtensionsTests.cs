using System;
using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class RandomSystemExtensionsTests
{
	[Fact]
	public async Task GenerateRandomFileExtension_ShouldNotStartWithDotOrReturnEmptyString()
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

		await That(fileExtensions.Count).IsGreaterThan(5);
		foreach (string fileExtension in fileExtensions)
		{
			await That(fileExtension).IsNotNullOrWhiteSpace();
			await That(fileExtension).DoesNotStartWith(".");
		}
	}

	[Fact]
	public async Task GenerateRandomFileName_ShouldGenerateEdgeCases()
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
			await That(fileName).IsNotNullOrWhiteSpace();
		}

		// Check edge cases for directories
		await That(fileNames).Contains(d => d.Contains(' ', StringComparison.Ordinal));
		await That(fileNames).Contains(d => d.Length == 1);
		await That(fileNames).Contains(d => d.StartsWith('.'));
	}
}
