using System;

namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal class CompletionSourceManagerBase<T> : ICompletionSourceManager
        where T : struct
    {
        public ref CompletionSource<T> AllocateAndActivate(out TaskToken token)
        {
            ref var source = ref CompletionSourcePool<T>.Allocate(out token);
            source.Activate();
            return ref source;
        }


        public bool IsCompleted(TaskToken token)
        {
            return CompletionSourcePool<T>.UnvalidatedGetSource(token).HasCompleted;
        }

        public void Complete(TaskToken token, Exception? ex)
        {
            CompletionSourcePool<T>.UnvalidatedGetSource(token).Complete(ex);
        }

        public void OnCompleted(TaskToken token, Action continuation)
        {
            throw new InvalidOperationException("Partial trust is not supported.");
        }

        public void UnsafeOnCompleted(TaskToken token, Action continuation)
        {
            CompletionSourcePool<T>.UnvalidatedGetSource(token).UnsafeOnCompleted(continuation);
        }
    }
}
