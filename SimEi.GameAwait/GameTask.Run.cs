using System;
using System.Threading;
using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Task;

namespace SimEi.Threading.GameAwait
{
    partial struct GameTask
    {
        public static GameTask Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ref var source = ref CompletionSourcePool<GameTask<VoidResult>.CompletionSource<ActionState>>
                .Allocate(out var token);
            source.State.StateMachine ??= new ActionState()
            {
                Token = token,
                Action = action
            };
            ThreadPool.UnsafeQueueUserWorkItem(source.State.StateMachine, false);

            return new GameTask(
                token,
                GameTask<VoidResult>.TaskCompletionSourcePool<ActionState>.Instance
            );
        }


        public static GameTask<TResult> Run<TResult>(Func<TResult> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ref var source = ref CompletionSourcePool<GameTask<TResult>.CompletionSource<ResultActionState<TResult>>>
                .Allocate(out var token);
            source.State.StateMachine ??= new ResultActionState<TResult>()
            {
                Token = token,
                Action = action
            };
            ThreadPool.UnsafeQueueUserWorkItem(source.State.StateMachine, false);

            return new GameTask<TResult>(
                token,
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
            );
        }


        // TODO: Run(Func<GameTask>)



        private class ActionState : IThreadPoolWorkItem
        {
            public AwaitableToken Token;
            public Action Action = null!;

            void IThreadPoolWorkItem.Execute()
            {
                try
                {
                    Action.Invoke();
                    SetResult();
                }
                catch (Exception ex)
                {
                    SetException(ex);
                }
            }

            private void SetResult()
            {
                GameTask<VoidResult>.TaskCompletionSourcePool<ActionState>.Instance
                    .SetResult(Token, default);
            }

            private void SetException(Exception ex)
            {
                GameTask<VoidResult>.TaskCompletionSourcePool<ActionState>.Instance
                    .SetException(Token, ex);
            }
        }

        private class ResultActionState<TResult> : IThreadPoolWorkItem
        {
            public AwaitableToken Token;
            public Func<TResult> Action = null!;

            void IThreadPoolWorkItem.Execute()
            {
                try
                {
                    var res = Action.Invoke();
                    SetResult(res);
                }
                catch (Exception ex)
                {
                    SetException(ex);
                }
            }

            private void SetResult(TResult result)
            {
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
                    .SetResult(Token, result);
            }

            private void SetException(Exception ex)
            {
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
                    .SetException(Token, ex);
            }
        }
    }
}
