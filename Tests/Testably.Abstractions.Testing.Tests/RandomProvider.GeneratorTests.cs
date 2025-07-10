using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests;

public partial class RandomProviderTests
{
	public class GeneratorTests
	{
		[Fact]
		public async Task Dispose_ShouldThrowObjectDisposedExceptionOnGetNext()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut = RandomProvider.Generator.FromEnumerable(enumerable);
			sut.GetNext();
			sut.GetNext();

			sut.Dispose();
			Exception? exception = Record.Exception(() =>
			{
				sut.GetNext();
			});

			await That(exception).IsExactly<ObjectDisposedException>();
		}

		[Theory]
		[AutoData]
		public async Task FromArray_Overflow_ShouldStartAgain(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut = RandomProvider.Generator.FromArray(values);

			Guid[] results = new Guid[values.Length * 2];
			for (int i = 0; i < values.Length * 2; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).IsEqualTo([.. values, .. values]).InAnyOrder();
		}

		[Theory]
		[AutoData]
		public async Task FromArray_ShouldIterateThroughArrayValue(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut = RandomProvider.Generator.FromArray(values);

			Guid[] results = new Guid[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).IsEqualTo(values).InAnyOrder();
		}

		[Fact]
		public async Task FromCallback_ShouldExecuteCallback()
		{
			int iterations = 30;
			int startValue = 10;
			int executionCount = 0;
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator.FromCallback(() => startValue + executionCount++);

			int[] results = new int[iterations];
			for (int i = 0; i < iterations; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(executionCount).IsEqualTo(iterations);
			await That(results).IsEqualTo(Enumerable.Range(startValue, iterations)).InAnyOrder();
		}

		[Fact]
		public async Task FromEnumerable_Dispose_ShouldDisposeEnumerator()
		{
			EnumerableMock enumerable = new();
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator.FromEnumerable(enumerable);

			sut.Dispose();

			await That(enumerable.Enumerator.IsDisposed).IsTrue();
		}

		[Fact]
		public async Task FromEnumerable_Overflow_List_ShouldResetEnumerator()
		{
			int maxRange = 100;
			// A list as enumerable does support `Reset`
			List<int> values = Enumerable.Range(0, maxRange).ToList();
			IEnumerable<int> enumerable = values;
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator.FromEnumerable(enumerable);

			int[] results = new int[maxRange * 2];
			for (int i = 0; i < maxRange * 2; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).IsEqualTo([.. values, .. values]).InAnyOrder();
		}

		[Fact]
		public async Task FromEnumerable_Overflow_ShouldResetEnumerator()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator.FromEnumerable(enumerable);

			for (int i = 0; i < maxRange; i++)
			{
				_ = sut.GetNext();
			}

			Exception? exception = Record.Exception(() =>
			{
				sut.GetNext();
			});

			// Enumerable.Range does not support `Reset`
			await That(exception).IsExactly<NotSupportedException>();
		}

		[Fact]
		public async Task FromEnumerable_ShouldReturnEnumerableValues()
		{
			int maxRange = 100;
			IEnumerable<int> enumerable = Enumerable.Range(0, maxRange);
			RandomProvider.Generator<int> sut =
				RandomProvider.Generator.FromEnumerable(enumerable);

			int[] results = new int[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).IsEqualTo(Enumerable.Range(0, maxRange)).InAnyOrder();
		}

		[Theory]
		[AutoData]
		public async Task FromValue_ShouldReturnFixedValue(Guid value)
		{
			int maxRange = 100;
			RandomProvider.Generator<Guid> sut = RandomProvider.Generator.FromValue(value);

			Guid[] results = new Guid[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).All().AreEquivalentTo(value);
		}

		[Theory]
		[AutoData]
		public async Task Operator_FromArray(Guid[] values)
		{
			RandomProvider.Generator<Guid> sut = values;

			Guid[] results = new Guid[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).IsEqualTo(values).InAnyOrder();
		}

		[Theory]
		[AutoData]
		public async Task Operator_FromCallback(Guid value)
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

			await That(results).All().AreEquivalentTo(value);
		}

		[Theory]
		[AutoData]
		public async Task Operator_FromValue(Guid value)
		{
			int maxRange = 100;
			RandomProvider.Generator<Guid> sut = value;

			Guid[] results = new Guid[maxRange];
			for (int i = 0; i < maxRange; i++)
			{
				results[i] = sut.GetNext();
			}

			await That(results).All().AreEquivalentTo(value);
		}

		private sealed class EnumerableMock : IEnumerable<int>
		{
			public EnumeratorMock Enumerator { get; } = new();

			#region IEnumerable<int> Members

			/// <inheritdoc cref="IEnumerable{T}.GetEnumerator()" />
			public IEnumerator<int> GetEnumerator()
				=> Enumerator;

			/// <inheritdoc cref="IEnumerable.GetEnumerator()" />
			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

			#endregion
		}

		private sealed class EnumeratorMock : IEnumerator<int>
		{
			public bool IsDisposed { get; private set; }

			#region IEnumerator<int> Members

			/// <inheritdoc cref="IEnumerator{T}.Current" />
			public int Current
				=> 0;

			/// <inheritdoc cref="IEnumerator.Current" />
			object IEnumerator.Current
				=> Current;

			/// <inheritdoc cref="IDisposable.Dispose()" />
			public void Dispose()
				=> IsDisposed = true;

			/// <inheritdoc cref="IEnumerator{T}.MoveNext()" />
			public bool MoveNext()
				=> throw new NotSupportedException();

			/// <inheritdoc cref="IEnumerator{T}.Reset()" />
			public void Reset()
				=> throw new NotSupportedException();

			#endregion
		}
	}
}
