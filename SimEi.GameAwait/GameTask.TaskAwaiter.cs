using System;
using System.Runtime.CompilerServices;

namespace SimEi.Threading.GameAwait
{
    public readonly partial struct GameTask
    {
        public readonly struct TaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly GameTask _task;

            internal TaskAwaiter(GameTask task)
            {
                _task = task;
            }

            public bool IsCompleted => _task._sourceManager.IsCompleted(_task._token);

            public void GetResult() => _task._sourceManager.GetResult(_task._token);

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
