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
	#pragma warning disable MA0051 // Method is too long
	public Net48ParityTests(ITestOutputHelper testOutputHelper)
		: base(new TestHelpers.Parity(), testOutputHelper)
	{
		#region Directory

		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.CreateDirectory),
			[
				typeof(string), typeof(DirectorySecurity)
			]));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetAccessControl),
			[
				typeof(string)
			]));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetAccessControl),
			[
				typeof(string), typeof(AccessControlSections)
			]));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.SetAccessControl),
			[
				typeof(string), typeof(DirectorySecurity)
			]));
		Parity.Directory.MissingMethods.Add(typeof(Directory).GetMethod(
			nameof(Directory.GetParent),
			[
				typeof(string)
			]));

		#endregion

		#region DirectoryInfo

		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.Create),
			[
				typeof(DirectorySecurity)
			]));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.CreateSubdirectory),
			[
				typeof(string), typeof(DirectorySecurity)
			]));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.GetAccessControl),
			Type.EmptyTypes));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.GetAccessControl),
			[
				typeof(AccessControlSections)
			]));
		Parity.DirectoryInfo.MissingMethods.Add(typeof(DirectoryInfo).GetMethod(
			nameof(DirectoryInfo.SetAccessControl),
			[
				typeof(DirectorySecurity)
			]));

		#endregion

		#region File

		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.Create),
			[
				typeof(string),
				typeof(int),
				typeof(FileOptions),
				typeof(FileSecurity)
			]));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.GetAccessControl),
			[
				typeof(string)
			]));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.GetAccessControl),
			[
				typeof(string), typeof(AccessControlSections)
			]));
		Parity.File.MissingMethods.Add(typeof(File).GetMethod(
			nameof(File.SetAccessControl),
			[
				typeof(string), typeof(FileSecurity)
			]));

		#endregion

		#region FileInfo

		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.GetAccessControl),
			Type.EmptyTypes));
		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.GetAccessControl),
			[
				typeof(AccessControlSections)
			]));
		Parity.FileInfo.MissingMethods.Add(typeof(FileInfo).GetMethod(
			nameof(FileInfo.SetAccessControl),
			[
				typeof(FileSecurity)
			]));

		#endregion

		#region Guid

		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.Parse),
			[
				typeof(string)
			]));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.ParseExact),
			[
				typeof(string), typeof(string)
			]));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.TryParse),
			[
				typeof(string), typeof(Guid).MakeByRefType()
			]));
		Parity.Guid.MissingMethods.Add(typeof(Guid).GetMethod(
			nameof(Guid.TryParseExact),
			[
				typeof(string), typeof(string), typeof(Guid).MakeByRefType()
			]));

		#endregion

		#region Path

		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.ChangeExtension),
			[
				typeof(string), typeof(string)
			]));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetExtension),
			[
				typeof(string)
			]));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetFileName),
			[
				typeof(string)
			]));
		Parity.Path.MissingMethods.Add(typeof(Path).GetMethod(
			nameof(Path.GetFileNameWithoutExtension),
			[
				typeof(string)
			]));

		#endregion

		#region ZipArchiveEntry

		Parity.ZipArchiveEntry.MissingProperties.Add(typeof(ZipArchiveEntry).GetProperty(
			nameof(ZipArchiveEntry.ExternalAttributes)));

		#endregion
	}
	#pragma warning restore MA0051 // Method is too long
}
#endif
