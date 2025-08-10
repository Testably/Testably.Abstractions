using System;
using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Examples.UnixFileMode;

/// <summary>
///     Default implementation of an <see cref="IUnixFileModeStrategy" /> which uses a callback to determine if access
///     should be granted.
/// </summary>
public class DefaultUnixFileModeStrategy : IUnixFileModeStrategy
{
	private string _group = "";
	private string _user = "";

	#region IUnixFileModeStrategy Members

	/// <inheritdoc cref="IUnixFileModeStrategy.IsAccessGranted(string, IFileSystemExtensibility, UnixFileMode, FileAccess)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		System.IO.UnixFileMode mode, FileAccess requestedAccess)
	{
		UserGroup fileUserGroup = extensibility
			                          .RetrieveMetadata<UserGroup>(
				                          nameof(DefaultUnixFileModeStrategy))
		                          ?? new UserGroup(_user, _group);
		switch (requestedAccess)
		{
			case FileAccess.Read:
				return mode.HasFlag(System.IO.UnixFileMode.OtherRead) ||
				       (mode.HasFlag(System.IO.UnixFileMode.GroupRead) &&
				        string.Equals(fileUserGroup.Group, _group, StringComparison.Ordinal)) ||
				       (mode.HasFlag(System.IO.UnixFileMode.UserRead) &&
				        string.Equals(fileUserGroup.User, _user, StringComparison.Ordinal));
			default:
				return mode.HasFlag(System.IO.UnixFileMode.OtherWrite) ||
				       (mode.HasFlag(System.IO.UnixFileMode.GroupWrite) &&
				        string.Equals(fileUserGroup.Group, _group, StringComparison.Ordinal)) ||
				       (mode.HasFlag(System.IO.UnixFileMode.UserWrite) &&
				        string.Equals(fileUserGroup.User, _user, StringComparison.Ordinal));
		}
	}

	/// <inheritdoc cref="IUnixFileModeStrategy.OnSetUnixFileMode(string, IFileSystemExtensibility, UnixFileMode)" />
	public void OnSetUnixFileMode(string fullPath, IFileSystemExtensibility extensibility,
		System.IO.UnixFileMode mode)
	{
		extensibility.StoreMetadata(nameof(DefaultUnixFileModeStrategy),
			new UserGroup(_user, _group));
	}

	#endregion

	/// <summary>
	///     Simulate running under the given <paramref name="group" />.
	/// </summary>
	public DefaultUnixFileModeStrategy SimulateGroup(string group)
	{
		_group = group;
		return this;
	}

	/// <summary>
	///     Simulate running under the given <paramref name="user" />.
	/// </summary>
	public DefaultUnixFileModeStrategy SimulateUser(string user)
	{
		_user = user;
		return this;
	}

	private record UserGroup(string User, string Group);
}
