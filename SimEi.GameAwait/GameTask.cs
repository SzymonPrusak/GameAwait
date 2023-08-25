using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Task;

namespace SimEi.Threading.GameAwait
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder))]
    public readonly partial struct GameTask
    {
        private readonly AwaitableToken _token;
        private readonly GameTask<VoidResult>.ITaskCompletionSourcePool _completionSourcePool;

        internal GameTask(AwaitableToken token, GameTask<VoidResult>.ITaskCompletionSourcePool completionSourcePool)
        {
            _token = token;
            _completionSourcePool = completionSourcePool;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }


        public readonly struct TaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly GameTask _task;

            internal TaskAwaiter(GameTask task)
            {
                _task = task;
            }

            public bool IsCompleted => _task._completionSourcePool.IsCompleted(_task._token);

            public void GetResult() => _task._completionSourcePool.GetResult(_task._token);

            public void OnCompleted(Action continuation)
            {
                _task._completionSourcePool.OnCompleted(_task._token, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _task._completionSourcePool.UnsafeOnCompleted(_task._token, continuation);
            }
        }
    }
}
