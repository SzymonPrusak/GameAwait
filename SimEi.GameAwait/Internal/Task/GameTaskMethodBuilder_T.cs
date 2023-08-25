using System;
using System.Runtime.CompilerServices;

namespace SimEi.Threading.GameAwait.Internal.Task
{
    public struct GameTaskMethodBuilder<TResult>
    // TODO: Don't allocate at all for operations completed synchronously.
    {
        private AwaitableToken _token;
        private GameTask<TResult>.ITaskCompletionSourcePool _completionSourcePool;


        internal readonly AwaitableToken Token => _token;
        internal readonly GameTask<TResult>.ITaskCompletionSourcePool CompletionSourcePool => _completionSourcePool;


        public readonly GameTask<TResult> Task => new(_token, _completionSourcePool);


        public static GameTaskMethodBuilder<TResult> Create()
        {
            return new();
        }


        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            // Execution context not supported.
            _completionSourcePool = GameTask<TResult>.TaskCompletionSourcePool<TStateMachine>.Instance;
            ref var source = ref CompletionSourcePool<GameTask<TResult>.CompletionSource<TStateMachine>>
                .Allocate(out _token);
            ref var state = ref source.State;
            state.ContinuationAction ??= GetContinuationAction<TStateMachine>(_token);
            state.StateMachine = stateMachine;
            state.StateMachine.MoveNext();
        }

        public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotSupportedException("Boxed state machine is not supported.");
        }


        public readonly void SetResult(TResult result) => _completionSourcePool.SetResult(_token, result);
        public readonly void SetException(Exception ex) => _completionSourcePool.SetException(_token, ex);


        public readonly void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine _)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            ref var source = ref CompletionSourcePool<GameTask<TResult>.CompletionSource<TStateMachine>>
                .UnvalidatedGetState(_token);
            awaiter.OnCompleted(source.ContinuationAction);
        }

        public readonly void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine _)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            ref var source = ref CompletionSourcePool<GameTask<TResult>.CompletionSource<TStateMachine>>
                .UnvalidatedGetState(_token);
            awaiter.UnsafeOnCompleted(source.ContinuationAction);
        }


        private static Action GetContinuationAction<TStateMachine>(AwaitableToken token)
            where TStateMachine : IAsyncStateMachine
        {
            return () =>
            {
                ref var state = ref CompletionSourcePool<GameTask<TResult>.CompletionSource<TStateMachine>>
                    .UnvalidatedGetState(token);
                state.StateMachine.MoveNext();
            };
        }
    }
}
