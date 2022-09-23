using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticDirectoryFields))]
    public void Directory_EnsureParityOfStaticFields(FieldInfo? field)
    {
        if (field == null)
        {
            _testOutputHelper.WriteLine("System.IO.Directory contains no fields!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {field.PrintField("Directory.")}");
        typeof(IFileSystem.IDirectory).ContainsEquivalentProperty(field).Should()
           .BeTrue($"System.IO.Directory contains {field.PrintField()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticDirectoryMethods))]
    public void Directory_EnsureParityOfStaticMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.Directory contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("Directory.")}");
        typeof(IFileSystem.IDirectory).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.Directory contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticDirectoryMethods()
    {
        return typeof(Directory)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedDirectoryMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticDirectoryFields()
    {
        return typeof(Directory)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedDirectoryFields.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}