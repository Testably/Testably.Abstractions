using System;
using System.IO.Abstractions;

namespace Testably.Abstractions.TestHelpers;

/// <summary>
///     If referencing this base class, the source generator will automatically create two classes implementing your class:
///     <br />
///     - one will provide a `RealFileSystem`<br />
///     - one will provide a `MockFileSystem`<br />
///     Thus your tests run on both systems identically.
/// </summary>
/// <remarks>
///     Important: You have to mark your class as ´partial`!
/// </remarks>
public abstract class FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public Test Test { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	// ReSharper disable once UnusedMember.Global
	protected FileSystemTestBase(
		Test test,
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		Test = test;
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	protected FileSystemTestBase()
	{
		throw new NotSupportedException(
			"The SourceGenerator didn't create the corresponding files!");
	}
}
