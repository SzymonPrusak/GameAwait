using System;
using System.Runtime.CompilerServices;

namespace SimEi.Threading.GameAwait.Internal.Task
{
    public struct GameTaskMethodBuilder
    // TODO: Don't allocate at all for operations completed synchronously.
    {
        private GameTaskMethodBuilder<VoidResult> _core;


        public readonly GameTask Task => new(_core.Token, _core.CompletionSourcePool);


        public static GameTaskMethodBuilder Create()
        {
            return new();
        }


        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => _core.Start(ref stateMachine);

        public readonly void SetStateMachine(IAsyncStateMachine stateMachine)
            => _core.SetStateMachine(stateMachine);


        public readonly void SetResult() => _core.SetResult(default);
        public readonly void SetException(Exception ex) => _core.SetException(ex);


        public readonly void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _core.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public readonly void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            => _core.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }
}
