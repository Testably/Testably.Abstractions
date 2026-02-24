using Testably.Abstractions.TestHelpers.Settings;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

[CollectionDefinition("RealFileSystemTests")]
public class FileSystemTestSettingsFixture : ICollectionFixture<TestSettingsFixture>
{
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}
