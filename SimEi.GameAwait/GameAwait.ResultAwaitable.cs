using System.Runtime.CompilerServices;
using System;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    public static partial class GameAwait
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


                public bool IsCompleted => CompletionSourcePool<T>.IsCompleted(_token);


                public TRes GetResult()
                {
                    var res = CompletionSourcePool<T>.UnvalidatedGetState(_token).Result;
                    CompletionSourcePool<T>.GetResult(_token);
                    return res;
                }

                public void OnCompleted(Action continuation)
                {
                    throw new InvalidOperationException("Partial trust is not supported.");
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    CompletionSourcePool<T>.UnsafeOnCompleted(_token, continuation);
                }
            }
        }
    }
}
