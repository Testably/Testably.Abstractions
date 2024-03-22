using System;
using System.IO;
using System.Reflection;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static class FileSystemInitializerExtensions
{
	/// <summary>
	///     Initializes the <see cref="IFileSystem" /> in the working directory with test data.
	/// </summary>
	public static IFileSystemInitializer<TFileSystem> Initialize<TFileSystem>(
		this TFileSystem fileSystem,
		Action<FileSystemInitializerOptions>? options = null)
		where TFileSystem : IFileSystem
		=> fileSystem.InitializeIn(".", options);

	/// <summary>
	/// </summary>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="assembly">The assembly in which the embedded resource files are located.</param>
	/// <param name="directoryPath">The directory path in which the found resource files are created.</param>
	/// <param name="relativePath">The relative path of the embedded resources in the <paramref name="assembly" />.</param>
	/// <param name="searchPattern">
	///     The search string to match against the names of embedded resources in the <paramref name="assembly" /> under
	///     <paramref name="relativePath" />.<br />
	///     This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't
	///     support regular expressions.
	/// </param>
	/// <param name="searchOption">
	///     One of the enumeration values that specifies whether the search operation should include only the
	///     <paramref name="relativePath" /> or should include all subdirectories.<br />
	///     The default value is <see cref="SearchOption.AllDirectories" />.
	/// </param>
	#pragma warning disable MA0051 // Method is too long
	public static void InitializeEmbeddedResourcesFromAssembly(this IFileSystem fileSystem,
		string directoryPath,
		Assembly assembly,
		string? relativePath = null,
		string searchPattern = "*",
		SearchOption searchOption = SearchOption.AllDirectories)
	{
		using IDisposable release = fileSystem.IgnoreStatistics();
		EnumerationOptions enumerationOptions =
			EnumerationOptionsHelper.FromSearchOption(searchOption);

		string[] resourcePaths = assembly.GetManifestResourceNames();
		string assemblyNamePrefix = $"{assembly.GetName().Name ?? ""}.";

		if (relativePath != null)
		{
			relativePath = relativePath.Replace(
				Path.AltDirectorySeparatorChar,
				Path.DirectorySeparatorChar);
			relativePath = relativePath.TrimEnd(Path.DirectorySeparatorChar);
			relativePath += Path.DirectorySeparatorChar;
		}

		foreach (string resourcePath in resourcePaths)
		{
			string fileName = resourcePath;
			if (fileName.StartsWith(assemblyNamePrefix, StringComparison.Ordinal))
			{
				fileName = fileName.Substring(assemblyNamePrefix.Length);
			}

			fileName = fileName.Replace('.', Path.DirectorySeparatorChar);
			int lastSeparator = fileName.LastIndexOf(Path.DirectorySeparatorChar);
			if (lastSeparator > 0)
			{
				#pragma warning disable CA1845
				fileName = fileName.Substring(0, lastSeparator) + "." +
				           fileName.Substring(lastSeparator + 1);
				#pragma warning restore CA1845
			}

			if (relativePath != null)
			{
				if (!fileName.StartsWith(relativePath, StringComparison.Ordinal))
				{
					continue;
				}

				fileName = fileName.Substring(relativePath.Length);
			}

			#pragma warning disable CA2249 // string.Contains with char is not supported on netstandard2.0
			if (!enumerationOptions.RecurseSubdirectories &&
			    fileName.IndexOf(Path.DirectorySeparatorChar) >= 0)
			{
				continue;
			}
			#pragma warning restore CA2249

			if (EnumerationOptionsHelper.MatchesPattern(
				fileSystem.ExecuteOrDefault(),
				enumerationOptions,
				fileName,
				searchPattern))
			{
				string filePath = fileSystem.Path.Combine(directoryPath, fileName);
				fileSystem.InitializeFileFromEmbeddedResource(filePath, assembly, resourcePath);
			}
		}
	}
	#pragma warning restore MA0051 // Method is too long

	/// <summary>
	///     Initializes the <see cref="IFileSystem" /> in the <paramref name="basePath" /> with test data.
	/// </summary>
	public static IFileSystemInitializer<TFileSystem> InitializeIn<TFileSystem>(
		this TFileSystem fileSystem,
		string basePath,
		Action<FileSystemInitializerOptions>? options = null)
		where TFileSystem : IFileSystem
	{
		using IDisposable release = fileSystem.IgnoreStatistics();
		if (fileSystem.Path.IsPathRooted(basePath) &&
		    fileSystem is MockFileSystem mockFileSystem)
		{
			string? drive = fileSystem.Path.GetPathRoot(basePath);
			mockFileSystem.WithDrive(drive);
		}

		fileSystem.Directory.CreateDirectory(basePath);
		fileSystem.Directory.SetCurrentDirectory(basePath);
		FileSystemInitializerOptions optionsValue = new();
		options?.Invoke(optionsValue);
		if (optionsValue.InitializeTempDirectory)
		{
			fileSystem.Directory.CreateDirectory(Path.GetTempPath());
		}

		return new FileSystemInitializer<TFileSystem>(fileSystem, ".");
	}

	/// <summary>
	///     Sets the current directory to a new temporary directory.<br />
	///     <see cref="IDirectory.GetCurrentDirectory()" /> and all relative paths will use this directory.
	/// </summary>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="prefix">
	///     A prefix to use for the temporary directory.<br />
	///     This simplifies matching directories to tests.
	/// </param>
	/// <param name="logger">(optional) A callback to log the cleanup process.</param>
	/// <returns>
	///     A <see cref="IDirectoryCleaner" /> that will
	///     force delete all content in the temporary directory on dispose.
	/// </returns>
	public static IDirectoryCleaner SetCurrentDirectoryToEmptyTemporaryDirectory(
		this IFileSystem fileSystem, string? prefix = null, Action<string>? logger = null)
	{
		using IDisposable release = fileSystem.IgnoreStatistics();
		return new DirectoryCleaner(fileSystem, prefix, logger);
	}

	private static void InitializeFileFromEmbeddedResource(this IFileSystem fileSystem,
		string path,
		Assembly assembly,
		string embeddedResourcePath)
	{
		using IDisposable release = fileSystem.IgnoreStatistics();
		using (Stream? embeddedResourceStream = assembly
			.GetManifestResourceStream(embeddedResourcePath))
		{
			if (embeddedResourceStream == null)
			{
				throw new ArgumentException(
					$"Resource '{embeddedResourcePath}' not found in assembly '{assembly.FullName}'",
					nameof(embeddedResourcePath));
			}

			using (BinaryReader streamReader = new(embeddedResourceStream))
			{
				byte[] fileData = streamReader.ReadBytes((int)embeddedResourceStream.Length);
				string? directoryPath = fileSystem.Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryPath))
				{
					fileSystem.Directory.CreateDirectory(directoryPath);
				}

				fileSystem.File.WriteAllBytes(path, fileData);
			}
		}
	}
}
