using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;


public class ParityExclusions
{
    public List<FieldInfo?> DirectoryFields = new();

    public List<MethodInfo?> DirectoryInfoMethods = new()
    {
        typeof(DirectoryInfo).GetMethod(nameof(DirectoryInfo.GetObjectData)),
        typeof(DirectoryInfo).GetMethod(nameof(DirectoryInfo.ToString))
    };

    public List<PropertyInfo?> DirectoryInfoProperties = new();

    public List<MethodInfo?> DirectoryMethods = new();

    public List<FieldInfo?> FileFields = new();

    public List<MethodInfo?> FileInfoMethods = new()
    {
        typeof(FileInfo).GetMethod(nameof(FileInfo.GetObjectData)),
        typeof(FileInfo).GetMethod(nameof(FileInfo.ToString))
    };

    public List<PropertyInfo?> FileInfoProperties = new();

    public List<MethodInfo?> FileSystemInfoMethods = new()
    {
        typeof(FileSystemInfo).GetMethod(nameof(FileSystemInfo.GetObjectData)),
        typeof(FileSystemInfo).GetMethod(nameof(FileSystemInfo.ToString))
    };

    public List<PropertyInfo?> FileSystemInfoProperties = new();

    public List<MethodInfo?> FileMethods = new()
    {
#if NET6_0_OR_GREATER
        typeof(File).GetMethod(nameof(File.OpenHandle))
#endif
    };

    public List<FieldInfo?> PathFields = new()
    {
#pragma warning disable CS0618
        typeof(Path).GetField(nameof(Path.InvalidPathChars))
#pragma warning restore CS0618
    };

    public List<Type> BaseTypes = new()
    {
        typeof(object), typeof(MarshalByRefObject)
    };

    public List<MethodInfo?> PathMethods = new();
}