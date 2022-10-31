using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests;

public partial class RandomProviderTests
{
	public class GeneratorTests
	{
		[Fact]
		public void Dispose_ShouldThrowObjectDisposedExceptionOnGetNext()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromEnumerable(enumerable);
			sut.GetNext();
			sut.GetNext();

			sut.Dispose();
			Exception? exception = Record.Exception(() =>
			{
				sut.GetNext();
			});

			exception.Should().BeOfType<ObjectDisposedException>();
		}

		[Theory]
		[AutoData]
		public void FromArray_Overflow_ShouldStartAgain(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut =
				RandomProvider.Generator<Guid>.FromArray(values);

			Guid[] results = new Guid[values.Length * 2];
			for (int i = 0; i < values.Length * 2; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().BeEquivalentTo(values.Concat(values));
		}

		[Theory]
		[AutoData]
		public void FromArray_ShouldIterateThroughArrayValue(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut =
				RandomProvider.Generator<Guid>.FromArray(values);

			Guid[] results = new Guid[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().BeEquivalentTo(values);
		}

		[Fact]
		public void FromCallback_ShouldExecuteCallback()
		{
			int iterations = 30;
			int startValue = 10;
			int executionCount = 0;
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromCallback(
					() => startValue + executionCount++);

			int[] results = new int[iterations];
			for (int i = 0; i < iterations; i++)
			{
				results[i] = sut.GetNext();
			}

			executionCount.Should().Be(iterations);
			results.Should().BeEquivalentTo(Enumerable.Range(startValue, iterations));
		}

		[Fact]
		public void FromEnumerable_Dispose_ShouldDisposeEnumerator()
		{
			Mock<IEnumerable<int>> mock = new();
			Mock<IEnumerator<int>> enumeratorMock = new();
			mock.Setup(m => m.GetEnumerator()).Returns(enumeratorMock.Object);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromEnumerable(mock.Object);

			sut.Dispose();

			enumeratorMock.Verify(m => m.Dispose());
		}

		[Fact]
		public void FromEnumerable_Overflow_List_ShouldResetEnumerator()
		{
			int maxRange = 100;
			// A list as enumerable does support `Reset`
			List<int> values = Enumerable.Range(0, maxRange).ToList();
			IEnumerable<int> enumerable = values;
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromEnumerable(enumerable);

			int[] results = new int[maxRange * 2];
			for (int i = 0; i < maxRange * 2; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().BeEquivalentTo(values.Concat(values));
		}

		[Fact]
		public void FromEnumerable_Overflow_ShouldResetEnumerator()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromEnumerable(enumerable);

			for (int i = 0; i < maxRange; i++)
			{
				_ = sut.GetNext();
			}

			Exception? exception = Record.Exception(() =>
			{
				sut.GetNext();
			});

			// Enumerable.Range does not support `Reset`
			exception.Should().BeOfType<NotSupportedException>();
		}

		[Fact]
		public void FromEnumerable_ShouldReturnEnumerableValues()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator<int>.FromEnumerable(enumerable);

			int[] results = new int[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().BeEquivalentTo(Enumerable.Range(0, maxRange));
		}

		[Theory]
		[AutoData]
		public void FromValue_ShouldReturnFixedValue(Guid value)
		{
			int maxRange = 100;
			RandomProvider.Generator<Guid> sut =
				RandomProvider.Generator<Guid>.FromValue(value);

			Guid[] results = new Guid[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().AllBeEquivalentTo(value);
		}

		[Theory]
		[AutoData]
		public void Operator_FromArray(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut = values;

			Guid[] results = new Guid[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().BeEquivalentTo(values);
		}

		[Theory]
		[AutoData]
		public void Operator_FromCallback(Guid value)
		{
			int maxRange = 100;
			// ReSharper disable once ConvertToLocalFunction
			Func<Guid> castFrom = () => value;
			RandomProvider.Generator<Guid> sut = castFrom;

			Guid[] results = new Guid[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().AllBeEquivalentTo(value);
		}

		[Theory]
		[AutoData]
		public void Operator_FromValue(Guid value)
		{
			int maxRange = 100;
			RandomProvider.Generator<Guid> sut = value;

			Guid[] results = new Guid[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			results.Should().AllBeEquivalentTo(value);
		}
	}
}