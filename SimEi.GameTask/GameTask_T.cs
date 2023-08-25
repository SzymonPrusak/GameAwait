using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameTask.Internal;
using SimEi.Threading.GameTask.Internal.Source;
using SimEi.Threading.GameTask.Internal.Source.Manager;
using SimEi.Threading.GameTask.Internal.Source.State;
using SimEi.Threading.GameTask.Internal.Task;

namespace SimEi.Threading.GameTask
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder<>))]
    public readonly partial struct GameTask<TResult>
    {
        private readonly TaskToken _token;
        private readonly IResultCompletionSourceManager<TResult> _sourceManager;

        internal GameTask(TaskToken token, IResultCompletionSourceManager<TResult> sourceManager)
        {
            _token = token;
            _sourceManager = sourceManager;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }



        internal struct TaskCompletionSourceState<TStateMachine>
            : IResultCompletionSourceState<TResult>
        {
            internal Action ContinuationAction;
            internal TStateMachine StateMachine;
            internal TResult Result;

            TResult IResultCompletionSourceState<TResult>.Result => Result;
        }


        internal interface ITaskResultCompletionSourceManager
            : IResultCompletionSourceManager<TResult>
        {
            void SetResult(TaskToken token, TResult result);
        }

        internal class TaskResultCompletionSourceManager<TStateMachine>
            : ResultCompletionSourceManager<TaskCompletionSourceState<TStateMachine>, TResult>, ITaskResultCompletionSourceManager
        {
            public static readonly TaskResultCompletionSourceManager<TStateMachine> Instance = new();

            public void SetResult(TaskToken token, TResult result)
            {
                ref var source = ref CompletionSourcePool<TaskCompletionSourceState<TStateMachine>>.UnvalidatedGetSource(token);
                source.State.Result = result;
            }
        }
    }
}
