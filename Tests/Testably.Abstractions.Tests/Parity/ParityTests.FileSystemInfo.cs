using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticFileSystemInfoProperties))]
    public void FileSystemInfo_EnsureParityOfFields(PropertyInfo? property)
    {
        if (property == null)
        {
            _testOutputHelper.WriteLine("System.IO.FileSystemInfo contains no properties!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {property.PrintProperty("FileSystemInfo.")}");
        typeof(IFileSystem.IFileSystemInfo).ContainsEquivalentProperty(property).Should()
           .BeTrue($"System.IO.FileSystemInfo contains {property.PrintProperty()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticFileSystemInfoMethods))]
    public void FileSystemInfo_EnsureParityOfMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.FileSystemInfo contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("FileSystemInfo.")}");
        typeof(IFileSystem.IFileSystemInfo).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.FileSystemInfo contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticFileSystemInfoMethods()
    {
        return typeof(FileSystemInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedFileSystemInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticFileSystemInfoProperties()
    {
        return typeof(FileSystemInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedFileSystemInfoProperties.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}