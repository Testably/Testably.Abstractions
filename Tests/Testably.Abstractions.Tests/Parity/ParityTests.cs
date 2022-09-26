using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
    #region Test Setup

    public ParityExclusions Blacklisted { get; }

    public List<string> ParityErrors { get; } = new();

    private readonly ITestOutputHelper _testOutputHelper;

    protected ParityTests(ParityExclusions blacklisted,
                          ITestOutputHelper testOutputHelper)
    {
        Blacklisted = blacklisted;
        _testOutputHelper = testOutputHelper;
    }

    #endregion

    [Fact]
    public void Directory_EnsureParityOfStaticFieldsWith_IDirectory()
    {
        foreach (FieldInfo field in typeof(Directory)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(f => !Blacklisted.DirectoryFields.Contains(f))
           .Where(f => !f.IsSpecialName)
           .OrderBy(f => f.Name))
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
    public void Directory_EnsureParityOfStaticMethodsWith_IDirectory()
    {
        foreach (MethodInfo method in typeof(Directory)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.DirectoryMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void DirectoryInfo_EnsureParityOfConstructorsWith_IDirectoryInfoFactory()
    {
        foreach (ConstructorInfo constructor in typeof(DirectoryInfo)
           .GetConstructors()
           .OrderBy(p => p.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {constructor.PrintConstructor()}");
            if (!typeof(IFileSystem.IDirectoryInfoFactory)
               .ContainsEquivalentMethod(constructor))
            {
                ParityErrors.Add(constructor.PrintConstructor());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void DirectoryInfo_EnsureParityOfMethodsWith_IDirectoryInfo()
    {
        foreach (MethodInfo method in typeof(DirectoryInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(m => !Blacklisted.DirectoryInfoMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void DirectoryInfo_EnsureParityOfPropertiesWith_IDirectoryInfo()
    {
        foreach (PropertyInfo property in typeof(DirectoryInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(p => !Blacklisted.DirectoryInfoProperties.Contains(p))
           .Where(p => !p.IsSpecialName)
           .OrderBy(p => p.Name))
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
    public void File_EnsureParityOfStaticFieldsWith_IFile()
    {
        foreach (FieldInfo field in typeof(File)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(f => !Blacklisted.FileFields.Contains(f))
           .Where(f => !f.IsSpecialName)
           .OrderBy(f => f.Name))
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

    [Fact]
    public void File_EnsureParityOfStaticMethodsWith_IFile()
    {
        foreach (MethodInfo method in typeof(File)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.FileMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void FileInfo_EnsureParityOfConstructorsWith_IFileInfoFactory()
    {
        foreach (ConstructorInfo constructor in typeof(FileInfo)
           .GetConstructors()
           .OrderBy(p => p.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {constructor.PrintConstructor()}");
            if (!typeof(IFileSystem.IFileInfoFactory)
               .ContainsEquivalentMethod(constructor))
            {
                ParityErrors.Add(constructor.PrintConstructor());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void FileInfo_EnsureParityOfMethodsWith_IFileInfo()
    {
        foreach (MethodInfo method in typeof(FileInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(m => !Blacklisted.FileInfoMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void FileInfo_EnsureParityOfPropertiesWith_IFileInfo()
    {
        foreach (PropertyInfo property in typeof(FileInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(p => !Blacklisted.FileInfoProperties.Contains(p))
           .Where(p => !p.IsSpecialName)
           .OrderBy(p => p.Name))
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
    public void FileSystemInfo_EnsureParityOfMethodsWith_IFileSystemInfo()
    {
        foreach (MethodInfo method in typeof(FileSystemInfo)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(m => !Blacklisted.FileSystemInfoMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void FileSystemInfo_EnsureParityOfPropertiesWith_IFileSystemInfo()
    {
        foreach (PropertyInfo property in typeof(FileSystemInfo)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(p => !Blacklisted.FileSystemInfoProperties.Contains(p))
           .Where(p => !p.IsSpecialName)
           .OrderBy(p => p.Name))
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
    public void Guid_EnsureParityOfStaticFieldsWith_IGuid()
    {
        foreach (FieldInfo field in typeof(Guid)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(f => !Blacklisted.GuidFields.Contains(f))
           .Where(f => !f.IsSpecialName)
           .OrderBy(f => f.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {field.PrintField("Guid.")}");
            if (!typeof(IRandomSystem.IGuid)
               .ContainsEquivalentProperty(field))
            {
                ParityErrors.Add(field.PrintField());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Guid_EnsureParityOfStaticMethodsWith_IGuid()
    {
        foreach (MethodInfo method in typeof(Guid)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.GuidMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("Guid.")}");
            if (!typeof(IRandomSystem.IGuid)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Path_EnsureParityOfStaticFieldsWith_IPath()
    {
        foreach (FieldInfo field in typeof(Path)
           .GetFields(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(f => !Blacklisted.PathFields.Contains(f))
           .Where(f => !f.IsSpecialName)
           .OrderBy(f => f.Name))
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
    public void Path_EnsureParityOfStaticMethodsWith_IPath()
    {
        foreach (MethodInfo method in typeof(Path)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(m => !Blacklisted.PathMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
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
    public void Random_EnsureParityOfMethodsWith_IRandom()
    {
        foreach (MethodInfo method in typeof(Random)
           .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
           .Where(p => p.DeclaringType == null ||
                       !Blacklisted.BaseTypes.Contains(p.DeclaringType))
           .Where(m => !Blacklisted.RandomMethods.Contains(m))
           .Where(m => !m.IsSpecialName)
           .OrderBy(m => m.Name)
           .ThenBy(m => m.GetParameters().Length))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {method.PrintMethod("Random.")}");
            if (!typeof(IRandomSystem.IRandom)
               .ContainsEquivalentMethod(method))
            {
                ParityErrors.Add(method.PrintMethod());
            }
        }

        ParityErrors.Should().BeEmpty();
    }

    [Fact]
    public void Random_EnsureParityOfStaticPropertiesWith_IRandomFactory()
    {
        foreach (PropertyInfo property in typeof(Random)
           .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static)
           .Where(p => !Blacklisted.RandomProperties.Contains(p))
           .Where(p => !p.IsSpecialName)
           .OrderBy(p => p.Name))
        {
            _testOutputHelper.WriteLine(
                $"Check parity property for {property.PrintProperty("Random.")}");
            if (!typeof(IRandomSystem.IRandomFactory)
               .ContainsEquivalentProperty(property))
            {
                ParityErrors.Add(property.PrintProperty());
            }
        }

        ParityErrors.Should().BeEmpty();
    }
}