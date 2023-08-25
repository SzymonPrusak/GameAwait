using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Task;

namespace SimEi.Threading.GameAwait
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder<>))]
    public readonly struct GameTask<TResult>
    {
        private readonly AwaitableToken _token;
        private readonly ITaskCompletionSourcePool _completionSourcePool;

        internal GameTask(AwaitableToken token, ITaskCompletionSourcePool completionSourcePool)
        {
            _token = token;
            _completionSourcePool = completionSourcePool;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }



        public struct CompletionSource<TStateMachine>
        {
            internal Action ContinuationAction;
            internal TStateMachine StateMachine;
            internal TResult Result;
            internal volatile Exception? Exception;
        }


        public readonly struct TaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly GameTask<TResult> _task;

            internal TaskAwaiter(GameTask<TResult> task)
            {
                _task = task;
            }

            public bool IsCompleted => _task._completionSourcePool.IsCompleted(_task._token);

            public TResult GetResult() => _task._completionSourcePool.GetResult(_task._token);

            public void OnCompleted(Action continuation)
            {
                _task._completionSourcePool.OnCompleted(_task._token, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _task._completionSourcePool.UnsafeOnCompleted(_task._token, continuation);
            }
        }


        internal interface ITaskCompletionSourcePool
        {
            void SetResult(AwaitableToken token, TResult result);
            void SetException(AwaitableToken token, Exception exception);

            bool IsCompleted(AwaitableToken token);
            TResult GetResult(AwaitableToken token);

            void OnCompleted(AwaitableToken token, Action continuation);
            void UnsafeOnCompleted(AwaitableToken token, Action continuation);
        }

        internal class TaskCompletionSourcePool<TStateMachine> : ITaskCompletionSourcePool
        {
            public static readonly TaskCompletionSourcePool<TStateMachine> Instance = new();

            private TaskCompletionSourcePool() { }


            public void SetResult(AwaitableToken token, TResult result)
            {
                ref var source = ref CompletionSourcePool<CompletionSource<TStateMachine>>
                    .UnvalidatedGetSource(token);
                source.State.Result = result;
                source.Complete();
            }

            public void SetException(AwaitableToken token, Exception ex)
            {
                ref var source = ref CompletionSourcePool<CompletionSource<TStateMachine>>
                    .UnvalidatedGetSource(token);
                source.State.Exception = ex;
                source.Complete();
            }

            public bool IsCompleted(AwaitableToken token)
            {
                return CompletionSourcePool<CompletionSource<TStateMachine>>.UnvalidatedGetSource(token).HasCompleted;
            }

            public TResult GetResult(AwaitableToken token)
            {
                ref var source = ref CompletionSourcePool<CompletionSource<TStateMachine>>.GetSource(token);

                ref var state = ref source.State;
                var ex = state.Exception;
                var res = state.Result;
                state.Exception = null;
                state.Result = default!;

                source.GetResult();
                CompletionSourcePool<CompletionSource<TStateMachine>>.Free(token);

                if (ex != null)
                    throw ex;

                return res;
            }

            public void OnCompleted(AwaitableToken token, Action continuation)
            {
                throw new InvalidOperationException("Partial trust is not supported.");
            }

            public void UnsafeOnCompleted(AwaitableToken token, Action continuation)
            {
                CompletionSourcePool<CompletionSource<TStateMachine>>.UnvalidatedGetSource(token)
                    .UnsafeOnCompleted(continuation);
            }
        }
    }
}
