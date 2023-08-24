using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Collection;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    internal static partial class CompletionSourcePool<T>
        where T : struct
    {
        private const int MaxPoolArrayCount = 5;
        private static readonly int _maxPooledSourceCapacity =
            LogArrayCollection<T>.GetCapacityForArrayCount(MaxPoolArrayCount);

        private static readonly Stack<ushort> _freePoolIndices = new();
        private static readonly LogArrayCollection<CompletionSource> _pool = new(MaxPoolArrayCount);


        public static AwaitableToken AllocateAndActivate(ref T state)
        {
            ref var source = ref AllocateCore(out var token);
            source.State = state;
            source.Activate();
            return token;
        }

        public static ref T Allocate(out AwaitableToken token)
        {
            return ref AllocateCore(out token).State;
        }

        public static void UnvalidatedActivate(AwaitableToken token)
        {
            UnvalidatedGetSource(token).Activate();
        }


        public static bool IsCompleted(AwaitableToken token)
        {
            ref var source = ref UnvalidatedGetSource(token);
            ValidateGeneration(ref source, token.Generation);

            return source.HasCompleted;
        }

        public static void GetResult(AwaitableToken token)
        {
            ref var source = ref UnvalidatedGetSource(token);
            ValidateGeneration(ref source, token.Generation);

            if (!source.HasCompleted)
                throw new InvalidOperationException("Calling GetResult() before completion is not supported.");
            source.Deactivate();

            lock (_freePoolIndices)
                _freePoolIndices.Push(token.Index);
        }

        public static void UnsafeOnCompleted(AwaitableToken token, Action continuation)
        {
            ref var source = ref UnvalidatedGetSource(token);
            ValidateGeneration(ref source, token.Generation);

            source.UnsafeOnCompleted(continuation);
        }


        private static ref CompletionSource AllocateCore(out AwaitableToken token)
        {
            lock (_freePoolIndices)
            {
                if (_freePoolIndices.Count == 0)
                {
                    if (!EnlargePool())
                    {
                        var box = new CompletionSourceBox();
                        token = new(box);
                        return ref box.Value;
                    }
                }

                ushort index = _freePoolIndices.Pop();
                ref var source = ref _pool.GetItem(index);
                token = new AwaitableToken(index, source.Generation);
                return ref source;
            }
        }

        private static bool EnlargePool()
        {
            int oldCapacity = _pool.TotalCapacity;
            if (oldCapacity == _maxPooledSourceCapacity)
                return false;

            _pool.AllocateNextArray();

            int newCapacity = _pool.TotalCapacity;
            if (newCapacity > ushort.MaxValue)
                throw new InvalidOperationException($"Maximum of {ushort.MaxValue} tasks exceeded");

            for (int i = oldCapacity; i < newCapacity; i++)
                _freePoolIndices.Push((ushort)i);

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T UnvalidatedGetState(AwaitableToken token)
        {
            return ref UnvalidatedGetSource(token).State;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref CompletionSource UnvalidatedGetSource(AwaitableToken token)
        {
            if (token.Reference == null)
                return ref _pool.GetItem(token.Index);
            else
                return ref Unsafe.As<CompletionSourceBox>(token.Reference).Value;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateGeneration(ref CompletionSource source, ushort generation)
        {
            if (source.Generation != generation)
                throw new InvalidOperationException("Tried to access awaitable after it has been marked for reusing.");
        }


        private class CompletionSourceBox
        {
            public CompletionSource Value;
        }
    }
}
