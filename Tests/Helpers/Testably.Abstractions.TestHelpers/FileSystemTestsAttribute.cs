using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using TUnit.Core.Interfaces;

namespace Testably.Abstractions.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class FileSystemTestsAttribute : TypedDataSourceAttribute<FileSystemTestData>,
	ITestDiscoveryEventReceiver
{
	#region ITestDiscoveryEventReceiver Members

	/// <inheritdoc />
	public int Order { get; } = 0;

	/// <inheritdoc />
	public ValueTask OnTestDiscovered(DiscoveredTestContext context)
	{
		if (context.TestContext.Metadata.TestDetails.TestClassArguments.Length > 0 && context.TestContext.Metadata.TestDetails.TestClassArguments[0] is FileSystemTestData.Real)
		{
			context.AddParallelConstraint(new NotInParallelConstraint(["RealFileSystem"]));
		}
#if NET8_0_OR_GREATER
		return ValueTask.CompletedTask;
#else
		return new ValueTask(Task.CompletedTask);
#endif
	}

	#endregion

	public override async IAsyncEnumerable<Func<Task<FileSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		await Task.CompletedTask;

		yield return () => Task.FromResult<FileSystemTestData>(
			new FileSystemTestData.Mocked("MockFileSystem"));

#if !NETFRAMEWORK
		yield return () => Task.FromResult<FileSystemTestData>(
			new FileSystemTestData.Mocked(SimulationMode.Linux, OSPlatform.Linux,
				"LinuxFileSystem"));
		yield return () => Task.FromResult<FileSystemTestData>(
			new FileSystemTestData.Mocked(SimulationMode.MacOS, OSPlatform.OSX, "MacFileSystem"));
		yield return () => Task.FromResult<FileSystemTestData>(
			new FileSystemTestData.Mocked(SimulationMode.Windows, OSPlatform.Windows,
				"WindowsFileSystem"));
#endif

#if DEBUG
		if (Settings.RealFileSystemTests == Settings.TestSettingStatus.AlwaysEnabled)
#else
		if (Settings.RealFileSystemTests != Settings.TestSettingStatus.AlwaysDisabled)
#endif
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Real(dataGeneratorMetadata));
		}
	}
}
