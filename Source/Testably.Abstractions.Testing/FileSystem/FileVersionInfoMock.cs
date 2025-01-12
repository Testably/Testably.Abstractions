using System;
using System.Text;
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

	private readonly FileVersionInfoContainer _container;
	private readonly MockFileSystem _fileSystem;
	private readonly string _path;

	private FileVersionInfoMock(
		IStorageLocation location,
		FileVersionInfoContainer container,
		MockFileSystem fileSystem)
	{
		_container = container;
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

			return _container.Comments;
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

			return _container.CompanyName;
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

			return _container.FileBuildPart;
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

			return _container.FileDescription;
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

			return _container.FileMajorPart;
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

			return _container.FileMinorPart;
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

			return _path;
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

			return _container.FilePrivatePart;
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

			return _container.FileVersion;
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

			return _container.InternalName;
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

			return _container.IsDebug;
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

			return _container.IsPatched;
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

			return _container.IsPreRelease;
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

			return _container.IsPrivateBuild;
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

			return _container.IsSpecialBuild;
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

			return _container.Language;
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

			return _container.LegalCopyright;
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

			return _container.LegalTrademarks;
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

			return _container.OriginalFilename;
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

			return _container.PrivateBuild;
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

			return _container.ProductBuildPart;
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

			return _container.ProductMajorPart;
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

			return _container.ProductMinorPart;
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

			return _container.ProductName;
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

			return _container.ProductPrivatePart;
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

			return _container.ProductVersion;
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

			return _container.SpecialBuild;
		}
	}

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		// An initial capacity of 512 was chosen because it is large enough to cover
		// the size of the static strings with enough capacity left over to cover
		// average length property values.
		StringBuilder sb = new(512);
		sb.Append("File:             ").AppendLine(FileName);
		sb.Append("InternalName:     ").AppendLine(InternalName);
		sb.Append("OriginalFilename: ").AppendLine(OriginalFilename);
		sb.Append("FileVersion:      ").AppendLine(FileVersion);
		sb.Append("FileDescription:  ").AppendLine(FileDescription);
		sb.Append("Product:          ").AppendLine(ProductName);
		sb.Append("ProductVersion:   ").AppendLine(ProductVersion);
		sb.Append("Debug:            ").Append(IsDebug).AppendLine();
		sb.Append("Patched:          ").Append(IsPatched).AppendLine();
		sb.Append("PreRelease:       ").Append(IsPreRelease).AppendLine();
		sb.Append("PrivateBuild:     ").Append(IsPrivateBuild).AppendLine();
		sb.Append("SpecialBuild:     ").Append(IsSpecialBuild).AppendLine();
		sb.Append("Language:         ").AppendLine(Language);
		return sb.ToString();
	}

	internal static FileVersionInfoMock New(
		IStorageLocation location,
		FileVersionInfoContainer container,
		MockFileSystem fileSystem)
		=> new(location, container, fileSystem);
}
