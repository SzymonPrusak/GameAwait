using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    partial class GameAwait
    {
        public readonly struct ResultAwaitable<T, TRes>
            where T : struct, IResultCompletionSourceState<TRes>
        {
            private readonly AwaitableToken _token;

            internal ResultAwaitable(AwaitableToken token)
            {
                _token = token;
            } 

            public ResultAwaiter GetAwaiter() => new(_token);


            public readonly struct ResultAwaiter : ICriticalNotifyCompletion
            {
                private readonly AwaitableToken _token;

                internal ResultAwaiter(AwaitableToken token)
                {
                    _token = token;
                }


                public bool IsCompleted => CompletionSourcePool<T>.GetSource(_token).HasCompleted;


                public TRes GetResult()
                {
                    ref var source = ref CompletionSourcePool<T>.GetSource(_token);
                    var res = source.State.Result;
                    source.GetResult();
                    CompletionSourcePool<T>.Free(_token);
                    return res;
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
