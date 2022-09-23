using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticDirectoryInfoProperties))]
    public void DirectoryInfo_EnsureParityOfFields(PropertyInfo? property)
    {
        if (property == null)
        {
            _testOutputHelper.WriteLine(
                "System.IO.DirectoryInfo contains no properties!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {property.PrintProperty("DirectoryInfo.")}");
        typeof(IFileSystem.IDirectoryInfo).ContainsEquivalentProperty(property).Should()
           .BeTrue($"System.IO.DirectoryInfo contains {property.PrintProperty()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticDirectoryInfoMethods))]
    public void DirectoryInfo_EnsureParityOfMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.DirectoryInfo contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("DirectoryInfo.")}");
        typeof(IFileSystem.IDirectoryInfo).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.DirectoryInfo contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticDirectoryInfoMethods()
    {
        return typeof(DirectoryInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedDirectoryInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticDirectoryInfoProperties()
    {
        return typeof(DirectoryInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedDirectoryInfoProperties.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}