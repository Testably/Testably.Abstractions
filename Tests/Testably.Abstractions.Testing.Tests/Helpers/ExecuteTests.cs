using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public sealed partial class ExecuteTests
{
	[Fact]
	public async Task Constructor_ForLinux_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.Linux);
		#pragma warning restore CS0618

		await That(sut.IsLinux).IsTrue();
		await That(sut.IsMac).IsFalse();
		await That(sut.IsNetFramework).IsFalse();
		await That(sut.IsWindows).IsFalse();
		await That(sut.StringComparisonMode).IsEqualTo(StringComparison.Ordinal);
	}

	[Fact]
	public async Task Constructor_ForMacOS_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.MacOS);
		#pragma warning restore CS0618

		await That(sut.IsLinux).IsFalse();
		await That(sut.IsMac).IsTrue();
		await That(sut.IsNetFramework).IsFalse();
		await That(sut.IsWindows).IsFalse();
		await That(sut.StringComparisonMode).IsEqualTo(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task Constructor_ForWindows_ShouldInitializeAccordingly()
	{
		#pragma warning disable CS0618
		Execute sut = new(new MockFileSystem(), SimulationMode.Windows);
		#pragma warning restore CS0618

		await That(sut.IsLinux).IsFalse();
		await That(sut.IsMac).IsFalse();
		await That(sut.IsNetFramework).IsFalse();
		await That(sut.IsWindows).IsTrue();
		await That(sut.StringComparisonMode).IsEqualTo(StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task Constructor_UnsupportedSimulationMode_ShouldThrowNotSupportedException()
	{
		void Act()
		{
			#pragma warning disable CS0618
			_ = new Execute(new MockFileSystem(), (SimulationMode)42);
			#pragma warning restore CS0618
		}

		await That(Act).ThrowsExactly<NotSupportedException>()
			.Whose(x => x.Message, it => it
				.Contains(nameof(SimulationMode.Linux)).And
				.Contains(nameof(SimulationMode.MacOS)).And
				.Contains(nameof(SimulationMode.Windows)));
	}
}
