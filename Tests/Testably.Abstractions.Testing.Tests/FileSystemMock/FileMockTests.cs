﻿namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class FileMockTests
{
	[Theory]
	[AutoData]
	public void SetCreationTime(string path, DateTime creationTime)
	{
		Testing.FileSystemMock fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTime(path, creationTime);

		fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
	}

	[Theory]
	[AutoData]
	public void SetCreationTimeUtc(string path, DateTime creationTime)
	{
		Testing.FileSystemMock fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTimeUtc(path, creationTime);

		fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTime);
	}
}