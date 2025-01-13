using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
internal static partial class SourceGenerationHelper
{
	public static string GenerateTimeSystemTestClasses(ClassModel model)
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.AppendLine($$"""
		                using Testably.Abstractions.TestHelpers;
		                using Testably.Abstractions.TestHelpers.Settings;
		                using Xunit.Abstractions;

		                namespace {{model.Namespace}}
		                {
		                	public abstract partial class {{model.Name}}<TTimeSystem>
		                	{
		                		protected {{model.Name}}(TTimeSystem timeSystem)
		                			: base(timeSystem)
		                		{
		                		}
		                	}
		                }

		                namespace {{model.Namespace}}.{{model.Name}}
		                {
		                	// ReSharper disable once UnusedMember.Global
		                	public sealed class MockTimeSystemTests : {{model.Name}}<MockTimeSystem>
		                	{
		                		public MockTimeSystemTests() : base(new MockTimeSystem(Testing.TimeProvider.Now()))
		                		{
		                		}
		                
		                		/// <inheritdoc cref="{{model.Name}}{TTimeSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                		{
		                			// Brittle tests are never skipped against the mock time system!
		                		}
		                	}
		                
		                	// ReSharper disable once UnusedMember.Global
		                	[Collection(nameof(RealTimeSystemTests))]
		                	public sealed class RealTimeSystemTests : {{model.Name}}<RealTimeSystem>
		                	{
		                		private readonly TestSettingsFixture _fixture;
		                
		                		public RealTimeSystemTests(TestSettingsFixture fixture) : base(new RealTimeSystem())
		                		{
		                			_fixture = fixture;
		                		}

		                #if DEBUG
		                		/// <inheritdoc cref="{{model.Name}}{TTimeSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			=> Xunit.Skip.If(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
		                				$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #else
		                		/// <inheritdoc cref="{{model.Name}}{TTimeSystem}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                		public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			=> Xunit.Skip.If(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
		                				$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #endif
		                	}
		                }
		                """);

		return sb.ToString();
	}
}
#pragma warning restore MA0051
