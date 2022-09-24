using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
    public ParityExclusions Blacklisted { get; }

    private readonly ITestOutputHelper _testOutputHelper;

    protected ParityTests(ParityExclusions blacklisted,
                          ITestOutputHelper testOutputHelper)
    {
        Blacklisted = blacklisted;
        _testOutputHelper = testOutputHelper;
    }

    public List<string> ParityErrors { get; } = new();

    [Fact]
    public void Directory_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(Directory)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.DirectoryMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("Directory.")}");
            if (!typeof(IFileSystem.IDirectory)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Directory_EnsureParityOfStaticFields()
    {
        foreach (FieldInfo field in typeof(Directory)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.DirectoryFields.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {field.PrintField("Directory.")}");
            if (!typeof(IFileSystem.IDirectory)
               .ContainsEquivalentProperty(field))
            {
                ParityErrors.Add(field.PrintField());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void DirectoryInfo_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(DirectoryInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.DirectoryInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("DirectoryInfo.")}");
            if (!typeof(IFileSystem.IDirectoryInfo)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void DirectoryInfo_EnsureParityOfStaticProperties()
    {
        foreach (PropertyInfo property in typeof(DirectoryInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => m.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(m.DeclaringType))
           .Where(m => !Blacklisted.DirectoryInfoProperties.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {property.PrintProperty("DirectoryInfo.")}");
            if (!typeof(IFileSystem.IDirectoryInfo)
               .ContainsEquivalentProperty(property))
            {
                ParityErrors.Add(property.PrintProperty());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void FileSystemInfo_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(FileSystemInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.FileSystemInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("FileSystemInfo.")}");
            if (!typeof(IFileSystem.IFileSystemInfo)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void FileSystemInfo_EnsureParityOfStaticProperties()
    {
        foreach (PropertyInfo property in typeof(FileSystemInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => m.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(m.DeclaringType))
           .Where(m => !Blacklisted.FileSystemInfoProperties.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {property.PrintProperty("FileSystemInfo.")}");
            if (!typeof(IFileSystem.IFileSystemInfo)
               .ContainsEquivalentProperty(property))
            {
                ParityErrors.Add(property.PrintProperty());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void FileInfo_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(FileInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.FileInfoMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("FileInfo.")}");
            if (!typeof(IFileSystem.IFileInfo)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void FileInfo_EnsureParityOfStaticProperties()
    {
        foreach (PropertyInfo property in typeof(FileInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => m.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(m.DeclaringType))
           .Where(m => !Blacklisted.FileInfoProperties.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {property.PrintProperty("FileInfo.")}");
            if (!typeof(IFileSystem.IFileInfo)
               .ContainsEquivalentProperty(property))
            {
                ParityErrors.Add(property.PrintProperty());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Path_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(Path)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.PathMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("Path.")}");
            if (!typeof(IFileSystem.IPath)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Path_EnsureParityOfStaticFields()
    {
        foreach (FieldInfo field in typeof(Path)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.PathFields.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {field.PrintField("Path.")}");
            if (!typeof(IFileSystem.IPath)
               .ContainsEquivalentProperty(field))
            {
                ParityErrors.Add(field.PrintField());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void File_EnsureParityOfStaticMethods()
    {
        foreach (MethodInfo method in typeof(File)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.FileMethods.Contains(m))
           .OrderBy(x => x.Name)
           .ThenBy(x => x.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("File.")}");
            if (!typeof(IFileSystem.IFile)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void File_EnsureParityOfStaticFields()
    {
        foreach (FieldInfo field in typeof(File)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.FileFields.Contains(m))
           .OrderBy(x => x.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {field.PrintField("File.")}");
            if (!typeof(IFileSystem.IFile)
               .ContainsEquivalentProperty(field))
            {
                ParityErrors.Add(field.PrintField());
            }
        }

        ParityErrors.Should().BeEmpty();
    }
}