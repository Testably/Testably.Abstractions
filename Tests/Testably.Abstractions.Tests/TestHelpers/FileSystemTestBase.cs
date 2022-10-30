namespace Testably.Abstractions.Tests.TestHelpers;

public abstract class FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemTestBase(TFileSystem fileSystem,
	                             ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	protected FileSystemTestBase()
	{
		throw new NotSupportedException("The SourceGenerator didn't create the corresponding files!");
	}
}