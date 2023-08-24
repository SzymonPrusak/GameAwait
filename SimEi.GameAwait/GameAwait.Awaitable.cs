using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    public static partial class GameAwait
    {
        public readonly struct Awaitable<T>
            where T : struct, ITrackedCompletionSourceState
        {
            private readonly AwaitableToken _token;

            internal Awaitable(AwaitableToken token)
            {
                _token = token;
            }

            public Awaiter GetAwaiter() => new(_token);


            public readonly struct Awaiter : ICriticalNotifyCompletion
            {
                private readonly AwaitableToken _token;

                internal Awaiter(AwaitableToken token)
                {
                    _token = token;
                }


                public bool IsCompleted => CompletionSourcePool<T>.GetSource(_token).HasCompleted;


                public void GetResult()
                {
                    ref var source = ref CompletionSourcePool<T>.GetSource(_token);
                    source.GetResult();
                    CompletionSourcePool<T>.Free(_token);
                }

                public void OnCompleted(Action continuation)
                {
                    throw new InvalidOperationException("Partial trust is not supported.");
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    CompletionSourcePool<T>.GetSource(_token).UnsafeOnCompleted(continuation);
                }
            }
        }
    }
}
