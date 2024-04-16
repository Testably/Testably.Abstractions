namespace Testably.Abstractions.Tests.TestHelpers;

[Flags]
public enum TestOs
{
	Linux = 1,
	Mac = 2,
	Windows = 4,
	Framework = 8,
	All = Linux | Mac | Windows | Framework,
	None = 0
}
