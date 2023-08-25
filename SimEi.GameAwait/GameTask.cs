using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Source.Manager;
using SimEi.Threading.GameAwait.Internal.Task;

namespace SimEi.Threading.GameAwait
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder))]
    public readonly partial struct GameTask
    {
        private readonly AwaitableToken _token;
        private readonly IVoidCompletionSourceManager _sourceManager;

        internal GameTask(AwaitableToken token, IVoidCompletionSourceManager sourceManager)
        {
            _token = token;
            _sourceManager = sourceManager;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }


        public struct TaskCompletionSourceState<TStateMachine>
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
