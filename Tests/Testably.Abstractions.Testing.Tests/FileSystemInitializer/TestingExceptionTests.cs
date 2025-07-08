#if !NET8_0_OR_GREATER
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class TestingExceptionTests
{
	[Theory]
	[AutoData]
	public async Task
		TestingException_SerializationAndDeserialization_ShouldKeepMessageAndInnerException(
			string message, Exception innerException)
	{
		TestingException originalException =
			new(message, innerException);

		byte[] buffer = new byte[4096];
		using MemoryStream ms = new(buffer);
		using MemoryStream ms2 = new(buffer);
		#pragma warning disable SYSLIB0011 //BinaryFormatter serialization is obsolete - only used in unit test
		BinaryFormatter formatter = new();
		formatter.Serialize(ms, originalException);
		TestingException deserializedException =
			(TestingException)formatter.Deserialize(ms2);
#pragma warning restore SYSLIB0011

		await That(deserializedException.InnerException?.Message).IsEqualTo(originalException.InnerException?.Message);
		await That(deserializedException.Message).IsEqualTo(originalException.Message);
	}
}
#endif
