using System;
using System.IO;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static partial class FileSystemInitializer
{
	private sealed class DirectoryCleaner : IDirectoryCleaner
	{
		private readonly IFileSystem _fileSystem;
		private readonly Action<string>? _logger;

		public DirectoryCleaner(IFileSystem fileSystem, Action<string>? logger)
		{
			_fileSystem = fileSystem;
			_logger = logger;
			BasePath = InitializeBasePath();
		}

		#region IDirectoryCleaner Members

		/// <inheritdoc cref="IDirectoryCleaner.BasePath" />
		public string BasePath { get; }

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
			ITimeSystem timeSystem = _fileSystem is FileSystemMock fileSystemMock
				? fileSystemMock.TimeSystem
				: new TimeSystem();
			try
			{
				// It is important to reset the current directory, as otherwise deleting the BasePath
				// results in a IOException, because the process cannot access the file.
				_fileSystem.Directory.SetCurrentDirectory(_fileSystem.Path.GetTempPath());

				_logger?.Invoke($"Cleaning up '{BasePath}'...");
				for (int i = 10; i >= 0; i--)
				{
					try
					{
						ForceDeleteDirectory(BasePath);
						break;
					}
					catch (Exception)
					{
						if (i == 0)
						{
							throw;
						}

						_logger?.Invoke(
							$"  Force delete failed! Retry again {i} times in 100ms...");
						timeSystem.Thread.Sleep(100);
					}
				}

				_logger?.Invoke("Cleanup was successful :-)");
			}
			catch (Exception ex)
			{
				_logger?.Invoke($"Could not clean up '{BasePath}' because: {ex}");
			}
		}

		#endregion

		/// <summary>
		///     Force deletes the directory at the given <paramref name="path" />.<br />
		///     Removes the <see cref="FileAttributes.ReadOnly" /> flag, if necessary.
		///     <para />
		///     If <paramref name="recursive" /> is set (default <see langword="true" />), the sub directories are force deleted as
		///     well.
		/// </summary>
		private void ForceDeleteDirectory(string path, bool recursive = true)
		{
			if (!_fileSystem.Directory.Exists(path))
			{
				return;
			}

			IFileSystem.IDirectoryInfo directory = _fileSystem.DirectoryInfo.New(path);
			directory.Attributes = FileAttributes.Normal;

			foreach (IFileSystem.IFileInfo info in directory.EnumerateFiles("*",
				SearchOption.TopDirectoryOnly))
			{
				info.Attributes = FileAttributes.Normal;
				info.Delete();
			}

			if (recursive)
			{
				foreach (IFileSystem.IDirectoryInfo info in
					directory.EnumerateDirectories("*",
						SearchOption.TopDirectoryOnly))
				{
					ForceDeleteDirectory(info.FullName, recursive);
				}
			}

			_fileSystem.Directory.Delete(path);
		}

		private string InitializeBasePath()
		{
			string basePath;

			do
			{
				string localBasePath = _fileSystem.Path.Combine(
					_fileSystem.Path.GetTempPath(),
					_fileSystem.Path.GetFileNameWithoutExtension(_fileSystem.Path
					   .GetRandomFileName()));
				Execute.OnMac(() => localBasePath = "/private" + localBasePath);
				basePath = localBasePath;
			} while (_fileSystem.Directory.Exists(basePath));

			_fileSystem.Directory.CreateDirectory(basePath);

			_logger?.Invoke($"Use '{basePath}' as current directory.");
			_fileSystem.Directory.SetCurrentDirectory(basePath);
			return basePath;
		}
	}
}