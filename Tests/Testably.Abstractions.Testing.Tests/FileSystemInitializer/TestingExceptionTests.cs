using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class TestingExceptionTests
{
	[Theory]
	[AutoData]
	public void
		TestingException_SerializationAndDeserialization_ShouldKeepMessageAndInnerException(
			string message, Exception innerException)
	{
		Testing.FileSystemInitializer.TestingException originalException =
			new(message, innerException);

		byte[] buffer = new byte[4096];
		using MemoryStream ms = new(buffer);
		using MemoryStream ms2 = new(buffer);
		BinaryFormatter formatter = new();
#pragma warning disable SYSLIB0011 //BinaryFormatter serialization is obsolete - only used in unit test
		formatter.Serialize(ms, originalException);
		Testing.FileSystemInitializer.TestingException deserializedException =
			(Testing.FileSystemInitializer.TestingException)formatter.Deserialize(ms2);
#pragma warning restore SYSLIB0011

		Assert.Equal(originalException.InnerException?.Message,
			deserializedException.InnerException?.Message);
		Assert.Equal(originalException.Message, deserializedException.Message);
	}
}