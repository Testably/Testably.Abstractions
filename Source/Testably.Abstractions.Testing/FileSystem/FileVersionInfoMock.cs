using System;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Mocked instance of a <see cref="IFileVersionInfo" />
/// </summary>
internal sealed class FileVersionInfoMock : IFileVersionInfo
{
	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	private readonly MockFileSystem _fileSystem;
	private readonly string _path;

	private FileVersionInfoMock(IStorageLocation location, MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
		_path = location.FullPath;
	}

	#region IFileVersionInfo Members

	/// <inheritdoc cref="IFileVersionInfo.Comments" />
	public string? Comments
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(Comments), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.CompanyName" />
	public string? CompanyName
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(CompanyName), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileBuildPart" />
	public int FileBuildPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileBuildPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileDescription" />
	public string? FileDescription
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileDescription), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileMajorPart" />
	public int FileMajorPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileMajorPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileMinorPart" />
	public int FileMinorPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileMinorPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileName" />
	public string FileName
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileName), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FilePrivatePart" />
	public int FilePrivatePart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FilePrivatePart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.FileVersion" />
	public string? FileVersion
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(FileVersion), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.InternalName" />
	public string? InternalName
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(InternalName), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.IsDebug" />
	public bool IsDebug
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(IsDebug), PropertyAccess.Get);

			return false;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.IsPatched" />
	public bool IsPatched
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(IsPatched), PropertyAccess.Get);

			return false;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.IsPreRelease" />
	public bool IsPreRelease
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(IsPreRelease), PropertyAccess.Get);

			return false;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.IsPrivateBuild" />
	public bool IsPrivateBuild
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(IsPrivateBuild), PropertyAccess.Get);

			return false;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.IsSpecialBuild" />
	public bool IsSpecialBuild
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(IsSpecialBuild), PropertyAccess.Get);

			return false;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.Language" />
	public string? Language
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(Language), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.LegalCopyright" />
	public string? LegalCopyright
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(LegalCopyright), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.LegalTrademarks" />
	public string? LegalTrademarks
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(LegalTrademarks), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.OriginalFilename" />
	public string? OriginalFilename
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(OriginalFilename), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.PrivateBuild" />
	public string? PrivateBuild
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(PrivateBuild), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductBuildPart" />
	public int ProductBuildPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductBuildPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductMajorPart" />
	public int ProductMajorPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductMajorPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductMinorPart" />
	public int ProductMinorPart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductMinorPart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductName" />
	public string? ProductName
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductName), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductPrivatePart" />
	public int ProductPrivatePart
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductPrivatePart), PropertyAccess.Get);

			return 0;
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.ProductVersion" />
	public string? ProductVersion
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(ProductVersion), PropertyAccess.Get);

			return "";
		}
	}

	/// <inheritdoc cref="IFileVersionInfo.SpecialBuild" />
	public string? SpecialBuild
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileVersionInfo.RegisterPathProperty(_path,
					nameof(SpecialBuild), PropertyAccess.Get);

			return "";
		}
	}

	#endregion

	internal static FileVersionInfoMock New(IStorageLocation location, MockFileSystem fileSystem)
		=> new(location, fileSystem);
}
