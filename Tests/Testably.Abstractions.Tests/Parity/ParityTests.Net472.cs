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
		///	 Specifies which methods, properties and fields are defined in .NET Framework 4.7.2, but not in .NET Standard 2.0.
		///	 <para />
		///	 As we only support .NET Standard 2.0 these are blacklisted.
		/// </summary>
		public Net472(ITestOutputHelper testOutputHelper)
			: base(new Parity(), testOutputHelper)
		{
			#region Directory

			Parity.Directory.ExcludedMethods.Add(typeof(Directory).GetMethod(
				nameof(Directory.CreateDirectory),
				new[] { typeof(string), typeof(DirectorySecurity) }));
			Parity.Directory.ExcludedMethods.Add(typeof(Directory).GetMethod(
				nameof(Directory.GetAccessControl),
				new[] { typeof(string) }));
			Parity.Directory.ExcludedMethods.Add(typeof(Directory).GetMethod(
				nameof(Directory.GetAccessControl),
				new[] { typeof(string), typeof(AccessControlSections) }));
			Parity.Directory.ExcludedMethods.Add(typeof(Directory).GetMethod(
				nameof(Directory.SetAccessControl),
				new[] { typeof(string), typeof(DirectorySecurity) }));
			Parity.Directory.ExcludedMethods.Add(typeof(Directory).GetMethod(
				nameof(Directory.GetParent),
				new[] { typeof(string) }));

			#endregion

			#region DirectoryInfo

			Parity.DirectoryInfo.ExcludedMethods.Add(typeof(DirectoryInfo).GetMethod(
				nameof(DirectoryInfo.Create),
				new[] { typeof(DirectorySecurity) }));
			Parity.DirectoryInfo.ExcludedMethods.Add(typeof(DirectoryInfo).GetMethod(
				nameof(DirectoryInfo.CreateSubdirectory),
				new[] { typeof(string), typeof(DirectorySecurity) }));
			Parity.DirectoryInfo.ExcludedMethods.Add(typeof(DirectoryInfo).GetMethod(
				nameof(DirectoryInfo.GetAccessControl),
				Type.EmptyTypes));
			Parity.DirectoryInfo.ExcludedMethods.Add(typeof(DirectoryInfo).GetMethod(
				nameof(DirectoryInfo.GetAccessControl),
				new[] { typeof(AccessControlSections) }));
			Parity.DirectoryInfo.ExcludedMethods.Add(typeof(DirectoryInfo).GetMethod(
				nameof(DirectoryInfo.SetAccessControl),
				new[] { typeof(DirectorySecurity) }));

			#endregion

			#region File

			Parity.File.ExcludedMethods.Add(typeof(File).GetMethod(
				nameof(File.Create),
				new[]
				{
					typeof(string), typeof(int), typeof(FileOptions),
					typeof(FileSecurity)
				}));
			Parity.File.ExcludedMethods.Add(typeof(File).GetMethod(
				nameof(File.GetAccessControl),
				new[] { typeof(string) }));
			Parity.File.ExcludedMethods.Add(typeof(File).GetMethod(
				nameof(File.GetAccessControl),
				new[] { typeof(string), typeof(AccessControlSections) }));
			Parity.File.ExcludedMethods.Add(typeof(File).GetMethod(
				nameof(File.SetAccessControl),
				new[] { typeof(string), typeof(FileSecurity) }));

			#endregion

			#region FileInfo
			
			Parity.FileInfo.ExcludedMethods.Add(typeof(FileInfo).GetMethod(
				nameof(FileInfo.GetAccessControl),
				Type.EmptyTypes));
			Parity.FileInfo.ExcludedMethods.Add(typeof(FileInfo).GetMethod(
				nameof(FileInfo.GetAccessControl),
				new[] { typeof(AccessControlSections) }));
			Parity.FileInfo.ExcludedMethods.Add(typeof(FileInfo).GetMethod(
				nameof(FileInfo.SetAccessControl),
				new[] { typeof(FileSecurity) }));

			#endregion

			#region Guid

			Parity.Guid.ExcludedMethods.Add(typeof(Guid).GetMethod(
				nameof(Guid.Parse),
				new[]
				{
					typeof(string)
				}));
			Parity.Guid.ExcludedMethods.Add(typeof(Guid).GetMethod(
				nameof(Guid.ParseExact),
				new[]
				{
					typeof(string),
					typeof(string)
				}));
			Parity.Guid.ExcludedMethods.Add(typeof(Guid).GetMethod(
				nameof(Guid.TryParse),
				new[]
				{
					typeof(string),
					typeof(Guid).MakeByRefType()
				}));
			Parity.Guid.ExcludedMethods.Add(typeof(Guid).GetMethod(
				nameof(Guid.TryParseExact),
				new[]
				{
					typeof(string),
					typeof(string),
					typeof(Guid).MakeByRefType()
				}));

			#endregion

			#region Path

			Parity.Path.ExcludedMethods.Add(typeof(Path).GetMethod(
				nameof(Path.ChangeExtension),
				new[] { typeof(string), typeof(string) }));
			Parity.Path.ExcludedMethods.Add(typeof(Path).GetMethod(
				nameof(Path.GetExtension),
				new[] { typeof(string) }));
			Parity.Path.ExcludedMethods.Add(typeof(Path).GetMethod(
				nameof(Path.GetFileName),
				new[] { typeof(string) }));
			Parity.Path.ExcludedMethods.Add(typeof(Path).GetMethod(
				nameof(Path.GetFileNameWithoutExtension),
				new[] { typeof(string) }));

			#endregion
		}
	}
}
#endif