using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

#pragma warning disable MA0051
#pragma warning disable MA0028
internal static partial class SourceGenerationHelper
{
	public static string GenerateTimeSystemTestClasses(ClassModel model)
	{
		StringBuilder? sb = GetSourceBuilder();
		sb.AppendLine($$"""
		                using Testably.Abstractions.TestHelpers;
		                using Testably.Abstractions.TestHelpers.Settings;

		                namespace {{model.Namespace}}
		                {
		                	public abstract partial class {{model.Name}}
		                	{
		                		/// <summary>
		                		///     The delay in milliseconds when wanting to ensure a timeout in the test.
		                		/// </summary>
		                		public const int EnsureTimeout = 500;
		                
		                		/// <summary>
		                		///     The delay in milliseconds when expecting a success in the test.
		                		/// </summary>
		                		public const int ExpectSuccess = 30000;
		                
		                		/// <summary>
		                		///     The delay in milliseconds when expecting a timeout in the test.
		                		/// </summary>
		                		public const int ExpectTimeout = 30;
		                
		                		public ITimeSystem TimeSystem { get; }
		                
		                		protected {{model.Name}}(ITimeSystem timeSystem)
		                		{
		                			TimeSystem = timeSystem;
		                		}
		                
		                		/// <summary>
		                		///     Specifies, if brittle tests should be skipped on the real time system.
		                		/// </summary>
		                		/// <param name="condition">
		                		///     (optional) A condition that must be <see langword="true" /> for the test to be skipped on the
		                		///     real time system.
		                		/// </param>
		                		public abstract void SkipIfBrittleTestsShouldBeSkipped(bool condition = true);
		                
		                		// ReSharper disable once UnusedMember.Global
		                		public sealed class MockTimeSystemTests : {{model.Name}}
		                		{
		                			public MockTimeSystemTests() : base(new MockTimeSystem(Testing.TimeProvider.Now()))
		                			{
		                			}
		                
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                			{
		                				// Brittle tests are never skipped against the mock time system!
		                			}
		                		}
		                
		                		// ReSharper disable once UnusedMember.Global
		                		[Collection("RealTimeSystemTests")]
		                		public sealed class RealTimeSystemTests : {{model.Name}}
		                		{
		                			private readonly TestSettingsFixture _fixture;
		                
		                			public RealTimeSystemTests(TestSettingsFixture fixture) : base(new RealTimeSystem())
		                			{
		                				_fixture = fixture;
		                			}

		                #if DEBUG
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                				=> aweXpect.Skip.When(condition && _fixture.BrittleTests != TestSettingStatus.AlwaysEnabled,
		                					$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #else
		                			/// <inheritdoc cref="{{model.Name}}.SkipIfBrittleTestsShouldBeSkipped(bool)" />
		                			public override void SkipIfBrittleTestsShouldBeSkipped(bool condition = true)
		                				=> aweXpect.Skip.When(condition && _fixture.BrittleTests == TestSettingStatus.AlwaysDisabled,
		                					$"Brittle tests are {_fixture.BrittleTests}. You can enable them by executing the corresponding tests in Testably.Abstractions.TestSettings.BrittleTests.");
		                #endif
		                		}
		                	}
		                }
		                """);

		return sb.ToString();
	}
}
#pragma warning restore MA0028
#pragma warning restore MA0051
