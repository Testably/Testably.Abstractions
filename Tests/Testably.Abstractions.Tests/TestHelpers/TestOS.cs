namespace Testably.Abstractions.Tests.TestHelpers;

[Flags]
public enum TestOs
{
	Windows = 1,
	Linux = 2,
	Mac = 4,
	All = Windows | Linux | Mac,
	None = 0
}
