using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    private static IEnumerable<object[]> GetEncodingDifference()
    {
        yield return new object[]
        {
            SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8
        };
    }
}