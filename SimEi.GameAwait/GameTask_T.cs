using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Source;
using SimEi.Threading.GameAwait.Internal.Source.Manager;
using SimEi.Threading.GameAwait.Internal.Source.State;
using SimEi.Threading.GameAwait.Internal.Task;

namespace SimEi.Threading.GameAwait
{
    [AsyncMethodBuilder(typeof(GameTaskMethodBuilder<>))]
    public readonly partial struct GameTask<TResult>
    {
        private readonly AwaitableToken _token;
        private readonly IResultCompletionSourceManager<TResult> _sourceManager;

        internal GameTask(AwaitableToken token, IResultCompletionSourceManager<TResult> sourceManager)
        {
            _token = token;
            _sourceManager = sourceManager;
        }


        public TaskAwaiter GetAwaiter()
        {
            return new(this);
        }



        public struct TaskCompletionSourceState<TStateMachine>
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
            void SetResult(AwaitableToken token, TResult result);
        }

        internal class TaskResultCompletionSourceManager<TStateMachine>
            : ResultCompletionSourceManager<TaskCompletionSourceState<TStateMachine>, TResult>, ITaskResultCompletionSourceManager
        {
            public static readonly TaskResultCompletionSourceManager<TStateMachine> Instance = new();

            public void SetResult(AwaitableToken token, TResult result)
            {
                ref var source = ref CompletionSourcePool<TaskCompletionSourceState<TStateMachine>>.UnvalidatedGetSource(token);
                source.State.Result = result;
            }
        }
    }
}
