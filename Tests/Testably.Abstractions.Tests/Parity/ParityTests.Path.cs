using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticPathFields))]
    public void Path_EnsureParityOfStaticFields(FieldInfo? field)
    {
        if (field == null)
        {
            _testOutputHelper.WriteLine("System.IO.Path contains no fields!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {field.PrintField("Path.")}");
        typeof(IFileSystem.IPath).ContainsEquivalentProperty(field).Should()
           .BeTrue($"System.IO.Path contains {field.PrintField()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticPathMethods))]
    public void Path_EnsureParityOfStaticMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.Path contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("Path.")}");
        typeof(IFileSystem.IPath).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.Path contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticPathMethods()
    {
        return typeof(Path)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedPathMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticPathFields()
    {
        return typeof(Path)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedPathFields.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}