using System;
using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Helpers;

internal static class ChangeDescriptionExtensions
{
	/// <summary>
	///     Determines whether the <paramref name="changeDescription" /> matches the provided filter criteria:<br />
	///     - <paramref name="fileSystemType" /><br />
	///     - <paramref name="changeType" /><br />
	///     - <paramref name="path" /><br />
	///     - <paramref name="searchPattern" /><br />
	///     - custom <paramref name="predicate" />
	/// </summary>
	/// <param name="changeDescription">The change description.</param>
	/// <param name="execute">The execution engine simulation the underlying operating system.</param>
	/// <param name="fileSystemType">The <see cref="ChangeDescription.FileSystemType" /> must have any of the provided flags.</param>
	/// <param name="changeType">The <see cref="ChangeDescription.ChangeType" /> must match this type.</param>
	/// <param name="path">The <see cref="ChangeDescription.Path" /> must match this path.</param>
	/// <param name="searchPattern">The <see cref="ChangeDescription.Name" /> must match this search pattern.</param>
	/// <param name="predicate">(optional) If provided, additional filter criteria can be implemented via this predicate.</param>
	/// <returns>
	///     <see langword="true" /> if the
	///     <paramref name="changeDescription" /> matches all filter criteria, otherwise <see langword="false" />.
	/// </returns>
	internal static bool Matches(this ChangeDescription changeDescription,
		Execute execute,
		FileSystemTypes fileSystemType,
		WatcherChangeTypes changeType,
		string path,
		string searchPattern,
		Func<ChangeDescription, bool>? predicate = null)
	{
		if (changeDescription.ChangeType != changeType ||
		    !changeDescription.FileSystemType.HasFlag(fileSystemType))
		{
			return false;
		}

		if (!string.IsNullOrEmpty(path) &&
		    !changeDescription.Path.StartsWith(path,
			    execute.StringComparisonMode))
		{
			return false;
		}

		if (searchPattern != EnumerationOptionsHelper.DefaultSearchPattern &&
		    (changeDescription.Name == null ||
		     !EnumerationOptionsHelper.MatchesPattern(
			     execute,
			     EnumerationOptionsHelper.Compatible,
			     changeDescription.Name,
			     searchPattern)))
		{
			return false;
		}

		return predicate?.Invoke(changeDescription) ?? true;
	}
}
