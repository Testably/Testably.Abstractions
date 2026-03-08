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
	/// <summary>
	///     If set to a specific operating system, tests are only created for this operating system.
	/// </summary>
	/// <remarks>
	///     Use <see cref="SimulationMode.Native" /> (default value) to run the tests on all operating systems.
	/// </remarks>
	public SimulationMode RequiredOperatingSystem { get; set; } = SimulationMode.Native;

#if NET8_0_OR_GREATER
	private static readonly ValueTask _completedTask = ValueTask.CompletedTask;
#else
	private static readonly ValueTask _completedTask = new(Task.CompletedTask);
#endif

	#region ITestDiscoveryEventReceiver Members

	/// <inheritdoc />
	public int Order { get; } = 0;

	/// <inheritdoc />
	public ValueTask OnTestDiscovered(DiscoveredTestContext context)
	{
		if (context.TestContext.Metadata.TestDetails.TestClassArguments.Length > 0 &&
		    context.TestContext.Metadata.TestDetails.TestClassArguments[0] is FileSystemTestData
			    .Real)
		{
			context.AddParallelConstraint(new NotInParallelConstraint(["RealFileSystem"]));
		}

		return _completedTask;
	}

	#endregion

	public override async IAsyncEnumerable<Func<Task<FileSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		await Task.CompletedTask;
		bool isMatchingPlatform =
			RequiredOperatingSystem switch
			{
				SimulationMode.Linux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
				SimulationMode.MacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
				SimulationMode.Windows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
				_ => true,
			};

		if (isMatchingPlatform)
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Mocked("MockFileSystem"));
		}

#if !NETFRAMEWORK
		if (RequiredOperatingSystem is SimulationMode.Native or SimulationMode.Linux)
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Mocked(SimulationMode.Linux, OSPlatform.Linux,
					"LinuxFileSystem"));
		}

		if (RequiredOperatingSystem is SimulationMode.Native or SimulationMode.MacOS)
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Mocked(SimulationMode.MacOS, OSPlatform.OSX,
					"MacFileSystem"));
		}

		if (RequiredOperatingSystem is SimulationMode.Native or SimulationMode.Windows)
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Mocked(SimulationMode.Windows, OSPlatform.Windows,
					"WindowsFileSystem"));
		}
#endif

#if DEBUG
		if (isMatchingPlatform &&
		    Settings.RealFileSystemTests == Settings.TestSettingStatus.AlwaysEnabled)
#else
		if (isMatchingPlatform &&
		    Settings.RealFileSystemTests != Settings.TestSettingStatus.AlwaysDisabled)
#endif
		{
			yield return () => Task.FromResult<FileSystemTestData>(
				new FileSystemTestData.Real(dataGeneratorMetadata));
		}
	}
}
