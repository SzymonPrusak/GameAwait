using System;
using System.Threading;
using System.Threading.Tasks;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    public static partial class GameAwait
    {
        // TODO: .tt generator for multiple different counts.

        /// <summary>
        /// Delays execution until all specified tasks are completed.
        /// </summary>
        public static ResultAwaitable<WhenAllCompletionSourceState<T1, T2>, (T1, T2)> WhenAll<T1, T2>(ValueTask<T1> t1, ValueTask<T2> t2)
        {
            ref var state = ref CompletionSourcePool<WhenAllCompletionSourceState<T1, T2>>.Allocate(out var token);
            if (state.TaskContinuation == null)
            {
                state.TaskContinuation = GetContinuation<T1, T2>(token);
                state.Token = token;
            }
            state.CompletedCount = 0;
            state.Task1 = t1;
            state.Task2 = t2;
            state.Result1 = default!;
            state.Result2 = default!;
            CompletionSourcePool<WhenAllCompletionSourceState<T1, T2>>.UnvalidatedActivate(token);

            t1.GetAwaiter().UnsafeOnCompleted(state.TaskContinuation);
            t2.GetAwaiter().UnsafeOnCompleted(state.TaskContinuation);

            return new(token);
        }
        

        private static Action GetContinuation<T1, T2>(AwaitableToken token)
        {
            return () =>
            {
                CompletionSourcePool<WhenAllCompletionSourceState<T1, T2>>.UnvalidatedGetState(token)
                    .IncrementAndTryComplete();
            };
        }


        public struct WhenAllCompletionSourceState<T1, T2> : IResultCompletionSourceState<(T1, T2)>
        {
            internal Action TaskContinuation;
            internal AwaitableToken Token;
            internal int CompletedCount;
            internal ValueTask<T1> Task1;
            internal ValueTask<T2> Task2;
            internal T1 Result1;
            internal T2 Result2;


            (T1, T2) IResultCompletionSourceState<(T1, T2)>.Result => (Result1, Result2);


            public void IncrementAndTryComplete()
            {
                if (Interlocked.Increment(ref CompletedCount) == 2)
                {
                    Result1 = Task1.GetAwaiter().GetResult();
                    Result2 = Task2.GetAwaiter().GetResult();
                    CompletionSourcePool<WhenAllCompletionSourceState<T1, T2>>.UnvalidatedGetSource(Token).Complete();
                }
            }
        }
    }
}
