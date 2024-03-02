using AutoFixture;
using System.Linq;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.Tests.Statistics;

public sealed partial class StatisticsTests
{
	[Theory]
	[MemberData(nameof(GetSynchronousFileMethods))]
	public void File_SynchronousMethods_ShouldRegisterCall(string expectation,
		Action<MockFileSystem> synchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		try
		{
			synchronousCall(sut);
		}
		catch (Exception)
		{
			// Ignore any exception called here, as we only care about the statistics call registration.
		}

		sut.Statistics.Calls.Count.Should().Be(1, expectation);
		sut.Statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.SequenceEqual(parameters),
				expectation);
	}

#if FEATURE_FILESYSTEM_ASYNC
	[Theory]
	[MemberData(nameof(GetAsynchronousFileMethods))]
	public async Task File_AsynchronousMethods_ShouldRegisterCall(string expectation,
		Func<MockFileSystem, Task> asynchronousCall, string name, object?[] parameters)
	{
		MockFileSystem sut = new();

		try
		{
			await asynchronousCall(sut);
		}
		catch (Exception)
		{
			// Ignore any exception called here, as we only care about the statistics call registration.
		}

		sut.Statistics.Calls.Count.Should().Be(1, expectation);
		sut.Statistics.Calls.Should()
			.ContainSingle(c => c.Name == name &&
			                    c.Parameters.SequenceEqual(parameters),
				expectation);
	}
#endif

	public static TheoryData<string, Action<MockFileSystem>, string, object?[]>
		GetSynchronousFileMethods()
	{
		Fixture fixture = CreateFixture();
		TheoryData<string, Action<MockFileSystem>, string, object?[]> theoryData = new();
		foreach ((string Expectation,
			Action<IFile> Action,
			string Name,
			object?[] Parameters) item in EnumerateSynchronousMethods<IFile>(fixture))
		{
			theoryData.Add(item.Expectation,
				m => item.Action.Invoke(m.File),
				item.Name,
				item.Parameters);
		}

		return theoryData;
	}

	public static TheoryData<string, Func<MockFileSystem, Task>, string, object?[]>
		GetAsynchronousFileMethods()
	{
		Fixture fixture = CreateFixture();
		TheoryData<string, Func<MockFileSystem, Task>, string, object?[]> theoryData = new();
		foreach ((string Expectation,
			Func<IFile, Task> Action,
			string Name,
			object?[] Parameters) item in EnumerateAsynchronousMethods<IFile>(fixture))
		{
			theoryData.Add(item.Expectation,
				m => item.Action.Invoke(m.File),
				item.Name,
				item.Parameters);
		}

		return theoryData;
	}
}
