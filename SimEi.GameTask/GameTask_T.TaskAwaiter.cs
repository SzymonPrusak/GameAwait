using System;
using System.Runtime.CompilerServices;

namespace SimEi.Threading.GameTask
{
    partial struct GameTask<TResult>
    {
        public readonly struct TaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly GameTask<TResult> _task;

            internal TaskAwaiter(GameTask<TResult> task)
            {
                _task = task;
            }

            public bool IsCompleted => _task._sourceManager.IsCompleted(_task._token);

            public TResult GetResult() => _task._sourceManager.GetResult(_task._token);

            public void OnCompleted(Action continuation)
            {
                _task._sourceManager.OnCompleted(_task._token, continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _task._sourceManager.UnsafeOnCompleted(_task._token, continuation);
            }
        }
    }
}
