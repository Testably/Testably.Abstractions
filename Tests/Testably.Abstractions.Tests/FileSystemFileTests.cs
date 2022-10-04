using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    private const string SpecialCharactersContent = "_€_Ä_Ö_Ü";

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
    }

    #endregion

    #region Helpers

    private static IEnumerable<object[]> GetEncodingDifference()
    {
        yield return new object[]
        {
            SpecialCharactersContent, Encoding.ASCII, Encoding.UTF8
        };
    }

    #endregion
}