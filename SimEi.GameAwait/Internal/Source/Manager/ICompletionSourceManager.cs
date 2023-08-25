using System;

namespace SimEi.Threading.GameAwait.Internal.Source.Manager
{
    internal interface ICompletionSourceManager
    {
        bool IsCompleted(AwaitableToken token);

        void Complete(AwaitableToken token, Exception? exception);

        void OnCompleted(AwaitableToken token, Action continuation);
        void UnsafeOnCompleted(AwaitableToken token, Action continuation);
    }
}
