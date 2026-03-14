using System;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
#if !NETFRAMEWORK
using System.Runtime.InteropServices;
#endif

namespace Testably.Abstractions.TestHelpers;

public abstract class FileSystemTestData
{
	private readonly string _testCase;

	protected FileSystemTestData(string testCase)
	{
		_testCase = testCase;
	}

	public abstract (IFileSystem fileSystem, ITimeSystem) GetAbstractions();
	public abstract IDirectoryCleaner GetDirectoryCleaner(IFileSystem fileSystem);
	public abstract Test GetTest();

	/// <inheritdoc />
	public override string ToString()
		=> _testCase;

	public class Mocked : FileSystemTestData
	{
#if !NETFRAMEWORK
		private readonly OSPlatform? _osPlatform;
		private readonly SimulationMode? _simulationMode;
#endif

		public Mocked(string testCase) : base(testCase)
		{
		}

#if !NETFRAMEWORK
		public Mocked(SimulationMode? simulationMode, OSPlatform? osPlatform, string testCase) :
			base(testCase)
		{
			_simulationMode = simulationMode;
			_osPlatform = osPlatform;
		}
#endif

		/// <inheritdoc />
		public override (IFileSystem fileSystem, ITimeSystem) GetAbstractions()
		{
#if NETFRAMEWORK
			MockFileSystem fileSystem = new();
#else
			MockFileSystem fileSystem = _simulationMode is null
				? new MockFileSystem()
				: new MockFileSystem(o => o.SimulatingOperatingSystem(_simulationMode.Value));
#endif
			ITimeSystem timeSystem = fileSystem.TimeSystem;
			return (fileSystem, timeSystem);
		}

		/// <inheritdoc />
		public override IDirectoryCleaner GetDirectoryCleaner(IFileSystem fileSystem)
		{
			return fileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory();
		}

#if !NETFRAMEWORK
		/// <inheritdoc />
		public override Test GetTest()
			=> _osPlatform is null ? new Test() : new Test(_osPlatform.Value);
#else
		/// <inheritdoc />
		public override Test GetTest()
			=> new();
#endif
	}

	public class Real : FileSystemTestData
	{
		private readonly DataGeneratorMetadata _dataGeneratorMetadata;

		public Real(DataGeneratorMetadata dataGeneratorMetadata,
			string testCase = "RealFileSystem") : base(testCase)
		{
			_dataGeneratorMetadata = dataGeneratorMetadata;
		}

		/// <inheritdoc />
		public override (IFileSystem fileSystem, ITimeSystem) GetAbstractions()
		{
			RealFileSystem fileSystem = new();
			RealTimeSystem timeSystem = new();
			return (fileSystem, timeSystem);
		}

		/// <inheritdoc />
		public override IDirectoryCleaner GetDirectoryCleaner(IFileSystem fileSystem)
			=> fileSystem.SetCurrentDirectoryToEmptyTemporaryDirectory(
				$"{_dataGeneratorMetadata.TestInformation?.Class.Name}-{_dataGeneratorMetadata.TestInformation?.Name ?? _dataGeneratorMetadata.TestSessionId}-",
				Console.WriteLine);

		/// <inheritdoc />
		public override Test GetTest()
			=> new();
	}
}
