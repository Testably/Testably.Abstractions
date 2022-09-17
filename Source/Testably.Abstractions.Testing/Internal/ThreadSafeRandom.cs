using System;

namespace Testably.Abstractions.Testing.Internal;

/// <summary>
///     <see href="https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/" />
/// </summary>
internal static class ThreadSafeRandom
{
    [ThreadStatic] private static Random? _local;

    private static readonly Random Global = new();

    private static Random Instance
    {
        get
        {
            if (_local is null)
            {
                int seed;
                lock (Global)
                {
                    seed = Global.Next();
                }

                _local = new Random(seed);
            }

            return _local;
        }
    }

    public static int Next() => Instance.Next();
}