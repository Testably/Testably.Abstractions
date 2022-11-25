﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

[ExcludeFromCodeCoverage]
internal sealed class DirectoryCleaner : IDirectoryCleaner
{
	private readonly IFileSystem _fileSystem;
	private readonly Action<string>? _logger;

	public DirectoryCleaner(IFileSystem fileSystem, string? prefix, Action<string>? logger)
	{
		_fileSystem = fileSystem;
		_logger = logger;
		BasePath = InitializeBasePath(prefix ?? "");
	}

	#region IDirectoryCleaner Members

	/// <inheritdoc cref="IDirectoryCleaner.BasePath" />
	public string BasePath { get; }

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		ITimeSystem? timeSystem = (_fileSystem as MockFileSystem)?.TimeSystem;
		try
		{
			// It is important to reset the current directory, as otherwise deleting the BasePath
			// results in a IOException, because the process cannot access the file.
			if (_fileSystem.Directory.GetCurrentDirectory() == BasePath)
			{
				_fileSystem.Directory.SetCurrentDirectory(_fileSystem.Path.GetTempPath());
			}

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
					if (timeSystem == null)
					{
						Thread.Sleep(100);
					}
					else
					{
						timeSystem.Thread.Sleep(100);
					}
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

		IDirectoryInfo directory = _fileSystem.DirectoryInfo.New(path);
		directory.Attributes = FileAttributes.Normal;

		foreach (IFileInfo info in directory.EnumerateFiles(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly))
		{
			info.Attributes = FileAttributes.Normal;
			info.Delete();
		}

		if (recursive)
		{
			foreach (IDirectoryInfo info in
				directory.EnumerateDirectories(
					EnumerationOptionsHelper.DefaultSearchPattern,
					SearchOption.TopDirectoryOnly))
			{
				ForceDeleteDirectory(info.FullName, recursive);
			}
		}

		_fileSystem.Directory.Delete(path);
	}

	private string InitializeBasePath(string prefix)
	{
		string basePath;

		do
		{
			string localBasePath = _fileSystem.Path.Combine(
				_fileSystem.Path.GetTempPath(),
				prefix + _fileSystem.Path.GetFileNameWithoutExtension(
					_fileSystem.Path.GetRandomFileName()));
			Execute.OnMac(() => localBasePath = "/private" + localBasePath);
			basePath = localBasePath;
		} while (_fileSystem.Directory.Exists(basePath));

		for (int i = 0; i <= 2; i++)
		{
			try
			{
				_fileSystem.Directory.CreateDirectory(basePath);
				break;
			}
			catch (Exception)
			{
				// Give a transient condition like antivirus/indexing a chance to go away
				Thread.Sleep(10);
			}
		}

		if (!_fileSystem.Directory.Exists(basePath))
		{
			throw new TestingException($"Could not create current directory '{basePath}' for tests");
		}

		_logger?.Invoke($"Use '{basePath}' as current directory.");
		_fileSystem.Directory.SetCurrentDirectory(basePath);
		for (int i = 0; i <= 10 && _fileSystem.Directory.GetCurrentDirectory() != basePath; i++)
		{
			Thread.Sleep(5);
		}

		if (_fileSystem.Directory.GetCurrentDirectory() != basePath)
		{
			throw new TestingException($"Could not set current directory to '{basePath}' for tests");
		}

		return basePath;
	}
}
