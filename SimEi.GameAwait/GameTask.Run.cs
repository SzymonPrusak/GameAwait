using System;
using System.Threading;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    partial struct GameTask
    {
        public static GameTask Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var sm = GameTask.TaskCompletionSourceManager<ActionState>.Instance;
            ref var source = ref sm.AllocateAndActivate(out var token);
            source.State.StateMachine ??= new ActionState()
            {
                Token = token,
                Action = action
            };
            ThreadPool.UnsafeQueueUserWorkItem(source.State.StateMachine, true);

            return new GameTask(token, sm);
        }


        public static GameTask<TResult> Run<TResult>(Func<TResult> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var sm = GameTask<TResult>.TaskResultCompletionSourceManager<ResultActionState<TResult>>.Instance;
            ref var source = ref sm.AllocateAndActivate(out var token);
            source.State.StateMachine ??= new ResultActionState<TResult>()
            {
                Token = token,
                Action = action
            };
            ThreadPool.UnsafeQueueUserWorkItem(source.State.StateMachine, true);

            return new GameTask<TResult>(token, sm);
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
                GameTask.TaskCompletionSourceManager<ActionState>.Instance.Complete(Token, null);
            }

            private void SetException(Exception ex)
            {
                GameTask.TaskCompletionSourceManager<ActionState>.Instance.Complete(Token, ex);
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
                var manager = GameTask<TResult>.TaskResultCompletionSourceManager<ResultActionState<TResult>>.Instance;
                manager.SetResult(Token, result);
                manager.Complete(Token, null);
            }

            private void SetException(Exception ex)
            {
                GameTask<TResult>.TaskResultCompletionSourceManager<ResultActionState<TResult>>.Instance
                    .Complete(Token, ex);
            }
        }
    }
}
