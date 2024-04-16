namespace Testably.Abstractions.Tests.TestHelpers;

[Flags]
public enum TestOS
{
	Windows = 1,
	Linux = 2,
	Mac = 4,
	All = Windows | Linux | Mac,
	None = 0
}
