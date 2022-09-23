using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Testably.Abstractions.Tests.Parity;

public partial class ParityTests
{
    [Theory]
    [MemberData(nameof(GetPublicStaticFileFields))]
    public void File_EnsureParityOfStaticFields(FieldInfo? field)
    {
        if (field == null)
        {
            _testOutputHelper.WriteLine("System.IO.File contains no fields!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity property for {field.PrintField("File.")}");
        typeof(IFileSystem.IFile).ContainsEquivalentProperty(field).Should()
           .BeTrue($"System.IO.File contains {field.PrintField()}");
    }

    [Theory]
    [MemberData(nameof(GetPublicStaticFileMethods))]
    public void File_EnsureParityOfStaticMethods(MethodInfo? method)
    {
        if (method == null)
        {
            _testOutputHelper.WriteLine("System.IO.File contains no methods!");
            return;
        }

        _testOutputHelper.WriteLine(
            $"Check parity method for {method.PrintMethod("File.")}");
        typeof(IFileSystem.IFile).ContainsEquivalentMethod(method).Should()
           .BeTrue($"System.IO.File contains {method.PrintMethod()}");
    }

    #region Helpers

    public static IEnumerable<object?[]> GetPublicStaticFileMethods()
    {
        return typeof(File)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedFileMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    public static IEnumerable<object?[]> GetPublicStaticFileFields()
    {
        return typeof(File)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !BlacklistedFileFields.Contains(m))
           .OrderBy(x => x.Name)
           .DefaultIfEmpty()
           .Select(m => new[] { m });
    }

    #endregion
}