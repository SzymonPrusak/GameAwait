using System;
using System.Runtime.CompilerServices;
using SimEi.Threading.GameTask.Internal.Source;

namespace SimEi.Threading.GameTask.Internal.Task
{
    public struct GameTaskMethodBuilder<TResult>
    {
        private TaskToken _token;
        private GameTask<TResult>.ITaskResultCompletionSourceManager _sourceManager;


        public readonly GameTask<TResult> Task => new(_token, _sourceManager);


        public static GameTaskMethodBuilder<TResult> Create()
        {
            return new();
        }


        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            // Execution context not supported.
            // Cannot skip allocation even if completed synchronously, because CompletionSource
            //  holds result and exception.
            var sm = GameTask<TResult>.TaskResultCompletionSourceManager<TStateMachine>.Instance;
            ref var source = ref sm.AllocateAndActivate(out _token);
            _sourceManager = sm;

            ref var state = ref source.State;
            state.ContinuationAction ??= GetContinuationAction<TStateMachine>(_token);
            state.StateMachine = stateMachine;
            state.StateMachine.MoveNext();
        }

        public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException("Boxed state machine is not supported.");
        }


        public readonly void SetResult(TResult result)
        {
            _sourceManager.SetResult(_token, result);
            _sourceManager.Complete(_token, null);
        }

        public readonly void SetException(Exception ex) => _sourceManager.Complete(_token, ex);


        public readonly void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine _)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            ref var source = ref CompletionSourcePool<GameTask<TResult>.TaskCompletionSourceState<TStateMachine>>
                .UnvalidatedGetState(_token);
            awaiter.OnCompleted(source.ContinuationAction);
        }

        public readonly void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine _)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            ref var source = ref CompletionSourcePool<GameTask<TResult>.TaskCompletionSourceState<TStateMachine>>
                .UnvalidatedGetState(_token);
            awaiter.UnsafeOnCompleted(source.ContinuationAction);
        }


        private static Action GetContinuationAction<TStateMachine>(TaskToken token)
            where TStateMachine : IAsyncStateMachine
        {
            return () =>
            {
                ref var state = ref CompletionSourcePool<GameTask<TResult>.TaskCompletionSourceState<TStateMachine>>
                    .UnvalidatedGetState(token);
                state.StateMachine.MoveNext();
            };
        }
    }
}
