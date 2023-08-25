using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameTask.Internal;
using SimEi.Threading.GameTask.Internal.Source.Manager;
using SimEi.Threading.GameTask.Internal.Source.State;
using SimEi.Threading.GameTask.Internal.Task;

namespace SimEi.Threading.GameTask
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder))]
    public readonly partial struct GameTask
    {
        private readonly TaskToken _token;
        private readonly IVoidCompletionSourceManager _sourceManager;

        internal GameTask(TaskToken token, IVoidCompletionSourceManager sourceManager)
        {
            _token = token;
            _sourceManager = sourceManager;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }


        private static GameTask AllocateTaskWithTracking<T>(ref T state)
            where T : struct, ITrackedCompletionSourceState
        {
            var sm = CompletionSourceManagers.Void<T>.Instance;
            ref var source = ref sm.AllocateAndActivate(out var token);
            source.State = state;
            TaskTracker<T>.Register(token);
            return new(token, sm);
        }



        internal struct TaskCompletionSourceState<TStateMachine>
        {
            internal Action ContinuationAction;
            internal TStateMachine StateMachine;
        }


        internal static class TaskCompletionSourceManager<TStateMachine>
        {
            public static VoidCompletionSourceManager<TaskCompletionSourceState<TStateMachine>> Instance =>
                CompletionSourceManagers.Void<TaskCompletionSourceState<TStateMachine>>.Instance;
        }
    }
}
