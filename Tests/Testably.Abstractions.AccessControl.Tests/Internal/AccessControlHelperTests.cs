using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.AccessControl.Tests.Internal;

public sealed class AccessControlHelperTests
{
	[Theory]
	[AutoDomainData]
	public async Task GetExtensibilityOrThrow_CustomDirectoryInfo_ShouldThrowNotSupportedException(
		IDirectoryInfo sut)
	{
		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Should().Throw<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Theory]
	[AutoDomainData]
	public async Task GetExtensibilityOrThrow_CustomFileInfo_ShouldThrowNotSupportedException(
		IFileInfo sut)
	{
		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Should().Throw<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Fact]
	public async Task
		GetExtensibilityOrThrow_CustomFileSystemStream_ShouldThrowNotSupportedException()
	{
		FileSystemStream sut = new CustomFileSystemStream();

		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Should().Throw<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Fact]
	public async Task ThrowIfMissing_ExistingDirectoryInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");
		fileSystem.Directory.CreateDirectory("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Should().NotThrow();
	}

	[Fact]
	public async Task ThrowIfMissing_ExistingFileInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");
		fileSystem.File.WriteAllText("foo", "some content");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Should().NotThrow();
	}

	[Fact]
	public async Task ThrowIfMissing_MissingDirectoryInfo_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Should().Throw<DirectoryNotFoundException>()
			.WithHResult(-2147024893).And
			.WithMessage($"*'{sut.FullName}'*").AsWildcard();
	}

	[Fact]
	public async Task ThrowIfMissing_MissingFileInfo_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Should().Throw<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessage($"*'{sut.FullName}'*").AsWildcard();
	}

	private sealed class CustomFileSystemStream() : FileSystemStream(Null, ".", false);
}
