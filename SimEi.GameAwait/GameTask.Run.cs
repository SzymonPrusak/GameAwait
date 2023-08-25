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
            ThreadPool.QueueUserWorkItem(ActionState.RunCallback, source.State.StateMachine);

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
            ThreadPool.QueueUserWorkItem(ResultActionState<TResult>.RunCallback, source.State.StateMachine);

            return new GameTask<TResult>(
                token,
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
            );
        }



        private class ActionState
        {
            public static readonly WaitCallback RunCallback = RunCore;

            public AwaitableToken Token;
            public Action Action = null!;

            public void SetResult()
            {
                GameTask<VoidResult>.TaskCompletionSourcePool<ActionState>.Instance
                    .SetResult(Token, default);
            }

            public void SetException(Exception ex)
            {
                GameTask<VoidResult>.TaskCompletionSourcePool<ActionState>.Instance
                    .SetException(Token, ex);
            }

            private static void RunCore(object? actionStateObj)
            {
                var actionState = (ActionState)actionStateObj!;
                try
                {
                    actionState.Action.Invoke();
                    actionState.SetResult();
                }
                catch (Exception ex)
                {
                    actionState.SetException(ex);
                }
            }
        }

        private class ResultActionState<TResult>
        {
            public static readonly WaitCallback RunCallback = RunCore;

            public AwaitableToken Token;
            public Func<TResult> Action = null!;

            public void SetResult(TResult result)
            {
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
                    .SetResult(Token, result);
            }

            public void SetException(Exception ex)
            {
                GameTask<TResult>.TaskCompletionSourcePool<ResultActionState<TResult>>.Instance
                    .SetException(Token, ex);
            }

            private static void RunCore(object? actionStateObj)
            {
                var actionState = (ResultActionState<TResult>)actionStateObj!;
                try
                {
                    var res = actionState.Action.Invoke();
                    actionState.SetResult(res);
                }
                catch (Exception ex)
                {
                    actionState.SetException(ex);
                }
            }
        }
    }
}
