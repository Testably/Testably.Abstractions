#if NET48
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using Xunit.Abstractions;

namespace Testably.Abstractions.Parity.Tests;

public class Net48ParityTests : ParityTests
{
	/// <summary>
	///     Specifies which methods, properties and fields are defined in .NET Framework 4.8, but not in .NET Standard 2.0.
	///     <para />
	///     As we only support .NET Standard 2.0 these are blacklisted.
	/// </summary>
	public Net48ParityTests(ITestOutputHelper testOutputHelper)
		: base(new TestHelpers.Parity(), testOutputHelper)
	{
		#region Directory

		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.CreateDirectory),
			new[]
			{
				typeof(string), typeof(DirectorySecurity)
			}));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetAccessControl),
			new[]
			{
				typeof(string)
			}));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetAccessControl),
			new[]
			{
				typeof(string), typeof(AccessControlSections)
			}));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.SetAccessControl),
			new[]
			{
				typeof(string), typeof(DirectorySecurity)
			}));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetParent),
			new[]
			{
				typeof(string)
			}));

		#endregion

		#region DirectoryInfo

		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.Create),
			new[]
			{
				typeof(DirectorySecurity)
			}));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.CreateSubdirectory),
			new[]
			{
				typeof(string), typeof(DirectorySecurity)
			}));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.GetAccessControl),
			Type.EmptyTypes));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.GetAccessControl),
			new[]
			{
				typeof(AccessControlSections)
			}));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.SetAccessControl),
			new[]
			{
				typeof(DirectorySecurity)
			}));

		#endregion

		#region File

		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.Create),
			new[]
			{
				typeof(string),
				typeof(int),
				typeof(FileOptions),
				typeof(FileSecurity)
			}));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.GetAccessControl),
			new[]
			{
				typeof(string)
			}));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.GetAccessControl),
			new[]
			{
				typeof(string), typeof(AccessControlSections)
			}));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.SetAccessControl),
			new[]
			{
				typeof(string), typeof(FileSecurity)
			}));

		#endregion

		#region FileInfo

		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.GetAccessControl),
			Type.EmptyTypes));
		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.GetAccessControl),
			new[]
			{
				typeof(AccessControlSections)
			}));
		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.SetAccessControl),
			new[]
			{
				typeof(FileSecurity)
			}));

		#endregion

		#region Guid

		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.Parse),
			new[]
			{
				typeof(string)
			}));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.ParseExact),
			new[]
			{
				typeof(string), typeof(string)
			}));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.TryParse),
			new[]
			{
				typeof(string), typeof(Guid).MakeByRefType()
			}));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.TryParseExact),
			new[]
			{
				typeof(string), typeof(string), typeof(Guid).MakeByRefType()
			}));

		#endregion

		#region Path

		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.ChangeExtension),
			new[]
			{
				typeof(string), typeof(string)
			}));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetExtension),
			new[]
			{
				typeof(string)
			}));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetFileName),
			new[]
			{
				typeof(string)
			}));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetFileNameWithoutExtension),
			new[]
			{
				typeof(string)
			}));

		#endregion

		#region ZipArchiveEntry

		Parity.ZipArchiveEntry.MissingProperties.Add(typeof(ZipArchiveEntry).GetProperty(
			nameof(ZipArchiveEntry.ExternalAttributes)));

		#endregion
	}
}
#endif
