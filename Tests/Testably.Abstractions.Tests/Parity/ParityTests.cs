using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    public static FieldInfo?[] BlacklistedDirectoryFields = { };

    public static MethodInfo?[] BlacklistedDirectoryInfoMethods =
    {
        typeof(DirectoryInfo).GetMethod(nameof(DirectoryInfo.GetObjectData)),
        typeof(DirectoryInfo).GetMethod(nameof(DirectoryInfo.ToString))
    };

    public static PropertyInfo?[] BlacklistedDirectoryInfoProperties = { };

    public static MethodInfo?[] BlacklistedDirectoryMethods = { };

    public static FieldInfo?[] BlacklistedFileFields = { };

    public static MethodInfo?[] BlacklistedFileInfoMethods =
    {
        typeof(FileInfo).GetMethod(nameof(FileInfo.GetObjectData)),
        typeof(FileInfo).GetMethod(nameof(FileInfo.ToString))
    };

    public static PropertyInfo?[] BlacklistedFileInfoProperties = { };

    public static MethodInfo?[] BlacklistedFileSystemInfoMethods =
    {
        typeof(FileSystemInfo).GetMethod(nameof(FileSystemInfo.GetObjectData)),
        typeof(FileSystemInfo).GetMethod(nameof(FileSystemInfo.ToString))
    };

    public static PropertyInfo?[] BlacklistedFileSystemInfoProperties = { };

    public static MethodInfo?[] BlacklistedFileMethods =
    {
#if NET6_0_OR_GREATER
        typeof(File).GetMethod(nameof(File.OpenHandle))
#endif
    };

    public static FieldInfo?[] BlacklistedPathFields =
    {
#pragma warning disable CS0618
        typeof(Path).GetField(nameof(Path.InvalidPathChars))
#pragma warning restore CS0618
    };

    public static Type[] BlacklistedBaseTypes =
    {
        typeof(object),
        typeof(MarshalByRefObject)
    };

    public static MethodInfo?[] BlacklistedPathMethods = { };

    private readonly ITestOutputHelper _testOutputHelper;

    public ParityTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}