using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameTask.Collection;

namespace SimEi.Threading.GameTask.Internal.Source
{
    internal static partial class CompletionSourcePool<T>
        where T : struct
    {
        private const int MaxPoolArrayCount = 3;
        private static readonly int _maxPooledSourceCapacity =
            LogArrayCollection<T>.GetCapacityForArrayCount(MaxPoolArrayCount);

        private static readonly Stack<ushort> _freePoolIndices = new();
        private static readonly LogArrayCollection<CompletionSource<T>> _pool = new(MaxPoolArrayCount);


        public static ref CompletionSource<T> Allocate(out TaskToken token)
        {
            lock (_freePoolIndices)
            {
                if (_freePoolIndices.Count == 0)
                {
                    if (!EnlargePool())
                    {
                        ;
                        var box = new CompletionSourceBox();
                        token = new(box);
                        return ref box.Value;
                    }
                }

                ushort index = _freePoolIndices.Pop();
                ref var source = ref _pool.GetItem(index);
                token = new TaskToken(index, source.Generation);
                return ref source;
            }
        }


        public static void Free(TaskToken token)
        {
            if (token.Reference == null)
            {
                lock (_freePoolIndices)
                    _freePoolIndices.Push(token.Index);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T UnvalidatedGetState(TaskToken token)
        {
            return ref UnvalidatedGetSource(token).State;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref CompletionSource<T> UnvalidatedGetSource(TaskToken token)
        {
            if (token.Reference == null)
                return ref _pool.GetItem(token.Index);
            else
                return ref Unsafe.As<CompletionSourceBox>(token.Reference).Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref CompletionSource<T> GetSource(TaskToken token)
        {
            ref var source = ref UnvalidatedGetSource(token);
            if (token.Reference == null)
                ValidateGeneration(ref source, token.Generation);
            return ref source;
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
        private static void ValidateGeneration(ref CompletionSource<T> source, ushort generation)
        {
            if (source.Generation != generation)
                throw new InvalidOperationException("Tried to access awaitable after it has been marked for reusing.");
        }


        private class CompletionSourceBox
        {
            public CompletionSource<T> Value;
        }
    }
}
