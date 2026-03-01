using Mockolate;
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.AccessControl.Tests.Internal;

public sealed class AccessControlHelperTests
{
	[Test]
	public async Task GetExtensibilityOrThrow_CustomDirectoryInfo_ShouldThrowNotSupportedException()
	{
		var sut = Mock.Create<IDirectoryInfo>();
		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Throws<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Test]
	public async Task GetExtensibilityOrThrow_CustomFileInfo_ShouldThrowNotSupportedException()
	{
		var sut = Mock.Create<IFileInfo>();
		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Throws<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Test]
	public async Task
		GetExtensibilityOrThrow_CustomFileSystemStream_ShouldThrowNotSupportedException()
	{
		FileSystemStream sut = new CustomFileSystemStream();

		void Act()
		{
			sut.GetExtensibilityOrThrow();
		}

		await That(Act).Throws<NotSupportedException>()
			.WithMessage($"*{sut.GetType().Name}*{nameof(IFileSystemExtensibility)}*").AsWildcard();
	}

	[Test]
	public async Task ThrowIfMissing_ExistingDirectoryInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");
		fileSystem.Directory.CreateDirectory("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task ThrowIfMissing_ExistingFileInfo_ShouldNotThrow()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");
		fileSystem.File.WriteAllText("foo", "some content");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task ThrowIfMissing_MissingDirectoryInfo_ShouldThrowDirectoryNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.DirectoryInfo.New("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessage($"*'{sut.FullName}'*").AsWildcard().And
			.WithHResult(-2147024893);
	}

	[Test]
	public async Task ThrowIfMissing_MissingFileInfo_ShouldThrowFileNotFoundException()
	{
		MockFileSystem fileSystem = new();
		IFileInfo sut = fileSystem.FileInfo.New("foo");

		void Act()
		{
			sut.ThrowIfMissing();
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage($"*'{sut.FullName}'*").AsWildcard().And
			.WithHResult(-2147024894);
	}

	private sealed class CustomFileSystemStream() : FileSystemStream(Null, ".", false);
}
