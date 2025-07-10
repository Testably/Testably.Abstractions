using System.IO;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class MoveTests
{
	[Theory]
	[AutoData]
	public async Task Move_CaseOnlyChange_ShouldMoveDirectoryWithContent(string path)
	{
		Skip.If(Test.IsNetFramework);

		string source = path.ToLowerInvariant();
		string destination = path.ToUpperInvariant();
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));

		FileSystem.Directory.Move(source, destination);

		await That(FileSystem.Directory.Exists(source)).IsEqualTo(!Test.RunsOnLinux);
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		await That(FileSystem.Directory.GetDirectories(".")).HasSingle().Matching(d => d.Contains(destination, StringComparison.Ordinal));
		await That(FileSystem.Directory.GetFiles(destination, initialized[1].Name)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[2].Name)).HasSingle();
		await That(FileSystem.Directory.GetFiles(destination, initialized[3].Name,	SearchOption.AllDirectories)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Move_CaseOnlyChange_ShouldThrowIOException_OnNetFramework(string path)
	{
		Skip.IfNot(Test.IsNetFramework);

		string source = path.ToLowerInvariant();
		string destination = path.ToUpperInvariant();
		FileSystem.Initialize()
			.WithSubdirectory(source);

		void Act()
		{
			FileSystem.Directory.Move(source, destination);
		}

		await That(Act).Throws<IOException>().WithHResult(-2146232800);
	}

	[Theory]
	[AutoData]
	public async Task Move_DestinationDoesNotExist_ShouldThrowDirectoryNotFoundException(
		string source)
	{
		FileSystem.InitializeIn(source)
			.WithAFile();
		string destination = FileTestHelper.RootDrive(Test, "not-existing/path");

		void Act()
		{
			FileSystem.Directory.Move(source, destination);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Move_ShouldMoveAttributes(string source, string destination)
	{
		FileSystem.Initialize()
			.WithSubdirectory(source);
		FileSystem.DirectoryInfo.New(source).Attributes |= FileAttributes.System;
		FileAttributes expectedAttributes =
			FileSystem.DirectoryInfo.New(source).Attributes;

		FileSystem.Directory.Move(source, destination);

		await That(FileSystem.DirectoryInfo.New(destination).Attributes).IsEqualTo(expectedAttributes);
	}

	[Theory]
	[AutoData]
	public async Task Move_ShouldMoveDirectoryWithContent(string source, string destination)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));

		FileSystem.Directory.Move(source, destination);

		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		await That(FileSystem.Directory.GetFiles(destination, initialized[1].Name)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[2].Name)).HasSingle();
		await That(FileSystem.Directory.GetFiles(destination, initialized[3].Name, SearchOption.AllDirectories)).HasSingle();
		await That(FileSystem.Directory.GetDirectories(destination, initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Move_ShouldNotAdjustTimes(string source, string destination)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Initialize()
			.WithSubdirectory(source).Initialized(s => s
				.WithAFile()
				.WithASubdirectory().Initialized(t => t
					.WithAFile()
					.WithASubdirectory()));
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.Directory.Move(source, destination);

		DateTime creationTime = FileSystem.Directory.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(destination);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task Move_SourceAndDestinationIdentical_ShouldThrowIOException(string path)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);

		void Act()
		{
			FileSystem.Directory.Move(path, path);
		}

		await That(Act).Throws<IOException>().WithHResult(-2146232800);
	}

	[Theory]
	[AutoData]
	public async Task Move_WithLockedFile_ShouldStillMoveDirectory_NotOnWindows(
		string source, string destination)
	{
		Skip.If(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		void Act()
		{
			FileSystem.Directory.Move(source, destination);
		}

		await That(Act).DoesNotThrow();
		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		await That(destinationDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Move_WithLockedFile_ShouldThrowIOException_AndNotMoveDirectoryAtAll_OnWindows(
		string source, string destination)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		using FileSystemStream stream = FileSystem.File.Open(initialized[3].FullName,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read);

		void Act()
		{
			FileSystem.Directory.Move(source, destination);
		}

		await That(Act).Throws<IOException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(source)).IsTrue();
		await That(FileSystem.Directory.Exists(destination)).IsFalse();
		IDirectoryInfo sourceDirectory =
			FileSystem.DirectoryInfo.New(source);
		await That(sourceDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(sourceDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(sourceDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle();
		await That(sourceDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}

	[Theory]
	[AutoData]
	public async Task Move_WithReadOnlyFile_ShouldMoveDirectoryWithContent(
		string source, string destination)
	{
		IFileSystemDirectoryInitializer<IFileSystem> initialized =
			FileSystem.Initialize()
				.WithSubdirectory(source).Initialized(s => s
					.WithAFile()
					.WithASubdirectory().Initialized(t => t
						.WithAFile()
						.WithASubdirectory()));
		initialized[3].Attributes = FileAttributes.ReadOnly;

		FileSystem.Directory.Move(source, destination);

		await That(FileSystem.Directory.Exists(source)).IsFalse();
		await That(FileSystem.Directory.Exists(destination)).IsTrue();
		IDirectoryInfo destinationDirectory =
			FileSystem.DirectoryInfo.New(destination);
		await That(destinationDirectory.GetFiles(initialized[1].Name)).HasSingle();
		await That(destinationDirectory.GetDirectories(initialized[2].Name)).HasSingle();
		await That(destinationDirectory.GetFiles(initialized[3].Name, SearchOption.AllDirectories)).HasSingle().Which.For(x => x.Attributes, a => a.HasFlag(FileAttributes.ReadOnly));
		await That(destinationDirectory.GetDirectories(initialized[4].Name, SearchOption.AllDirectories)).HasSingle();
	}
}
