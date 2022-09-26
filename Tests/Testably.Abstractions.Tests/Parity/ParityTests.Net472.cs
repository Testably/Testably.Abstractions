#if NET472
using System.IO;
using System.Security.AccessControl;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
    public class Net472 : ParityTests
    {
        /// <summary>
        ///     Specifies which methods, properties and fields are defined in .NET Framework 4.7.2, but not in .NET Standard 2.0.
        ///     <para />
        ///     As we only support .NET Standard 2.0 these are blacklisted.
        /// </summary>
        public Net472(ITestOutputHelper testOutputHelper)
            : base(new ParityExclusions(), testOutputHelper)
        {
            #region Directory

            Blacklisted.DirectoryMethods.Add(typeof(Directory).GetMethod(
                nameof(Directory.CreateDirectory),
                new[] { typeof(string), typeof(DirectorySecurity) }));
            Blacklisted.DirectoryMethods.Add(typeof(Directory).GetMethod(
                nameof(Directory.GetAccessControl),
                new[] { typeof(string) }));
            Blacklisted.DirectoryMethods.Add(typeof(Directory).GetMethod(
                nameof(Directory.GetAccessControl),
                new[] { typeof(string), typeof(AccessControlSections) }));
            Blacklisted.DirectoryMethods.Add(typeof(Directory).GetMethod(
                nameof(Directory.SetAccessControl),
                new[] { typeof(string), typeof(DirectorySecurity) }));
            Blacklisted.DirectoryMethods.Add(typeof(Directory).GetMethod(
                nameof(Directory.GetParent),
                new[] { typeof(string) }));

            #endregion

            #region DirectoryInfo

            Blacklisted.DirectoryInfoMethods.Add(typeof(DirectoryInfo).GetMethod(
                nameof(DirectoryInfo.Create),
                new[] { typeof(DirectorySecurity) }));
            Blacklisted.DirectoryInfoMethods.Add(typeof(DirectoryInfo).GetMethod(
                nameof(DirectoryInfo.CreateSubdirectory),
                new[] { typeof(string), typeof(DirectorySecurity) }));
            Blacklisted.DirectoryInfoMethods.Add(typeof(DirectoryInfo).GetMethod(
                nameof(DirectoryInfo.GetAccessControl),
                Type.EmptyTypes));
            Blacklisted.DirectoryInfoMethods.Add(typeof(DirectoryInfo).GetMethod(
                nameof(DirectoryInfo.GetAccessControl),
                new[] { typeof(AccessControlSections) }));
            Blacklisted.DirectoryInfoMethods.Add(typeof(DirectoryInfo).GetMethod(
                nameof(DirectoryInfo.SetAccessControl),
                new[] { typeof(DirectorySecurity) }));

            #endregion

            #region File

            Blacklisted.FileMethods.Add(typeof(File).GetMethod(
                nameof(File.Create),
                new[]
                {
                    typeof(string), typeof(int), typeof(FileOptions),
                    typeof(FileSecurity)
                }));
            Blacklisted.FileMethods.Add(typeof(File).GetMethod(
                nameof(File.GetAccessControl),
                new[] { typeof(string) }));
            Blacklisted.FileMethods.Add(typeof(File).GetMethod(
                nameof(File.GetAccessControl),
                new[] { typeof(string), typeof(AccessControlSections) }));
            Blacklisted.FileMethods.Add(typeof(File).GetMethod(
                nameof(File.SetAccessControl),
                new[] { typeof(string), typeof(FileSecurity) }));

            #endregion

            #region FileInfo
            
            Blacklisted.FileInfoMethods.Add(typeof(FileInfo).GetMethod(
                nameof(FileInfo.GetAccessControl),
                Type.EmptyTypes));
            Blacklisted.FileInfoMethods.Add(typeof(FileInfo).GetMethod(
                nameof(FileInfo.GetAccessControl),
                new[] { typeof(AccessControlSections) }));
            Blacklisted.FileInfoMethods.Add(typeof(FileInfo).GetMethod(
                nameof(FileInfo.SetAccessControl),
                new[] { typeof(FileSecurity) }));

            #endregion

            #region Guid

            Blacklisted.GuidMethods.Add(typeof(Guid).GetMethod(
                nameof(Guid.Parse),
                new[]
                {
                    typeof(string)
                }));
            Blacklisted.GuidMethods.Add(typeof(Guid).GetMethod(
                nameof(Guid.ParseExact),
                new[]
                {
                    typeof(string),
                    typeof(string)
                }));
            Blacklisted.GuidMethods.Add(typeof(Guid).GetMethod(
                nameof(Guid.TryParse),
                new[]
                {
                    typeof(string),
                    typeof(Guid).MakeByRefType()
                }));
            Blacklisted.GuidMethods.Add(typeof(Guid).GetMethod(
                nameof(Guid.TryParseExact),
                new[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(Guid).MakeByRefType()
                }));

            #endregion

            #region Path

            Blacklisted.PathMethods.Add(typeof(Path).GetMethod(
                nameof(Path.ChangeExtension),
                new[] { typeof(string), typeof(string) }));
            Blacklisted.PathMethods.Add(typeof(Path).GetMethod(
                nameof(Path.GetExtension),
                new[] { typeof(string) }));
            Blacklisted.PathMethods.Add(typeof(Path).GetMethod(
                nameof(Path.GetFileName),
                new[] { typeof(string) }));
            Blacklisted.PathMethods.Add(typeof(Path).GetMethod(
                nameof(Path.GetFileNameWithoutExtension),
                new[] { typeof(string) }));

            #endregion
        }
    }
}
#endif