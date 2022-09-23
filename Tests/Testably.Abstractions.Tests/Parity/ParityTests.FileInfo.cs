using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticFileInfoProperties))]
    public void FileInfo_EnsureParityOfFields(PropertyInfo? property)
    {
        if (property == null)
        {
            _testOutputHelper.WriteLine("System.IO.FileInfo contains no properties!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {property.PrintProperty("FileInfo.")}");
        typeof(IFileSystem.IFileInfo).ContainsEquivalentProperty(property).Should()
           .BeTrue($"System.IO.FileInfo contains {property.PrintProperty()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticFileInfoMethods))]
    public void FileInfo_EnsureParityOfMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.FileInfo contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("FileInfo.")}");
        typeof(IFileSystem.IFileInfo).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.FileInfo contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticFileInfoMethods()
    {
        return typeof(FileInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedFileInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticFileInfoProperties()
    {
        return typeof(FileInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(m => !BlacklistedBaseTypes.Contains(m.DeclaringType))
           .Where(m => !BlacklistedFileInfoProperties.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}