using System;

namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal interface ICompletionSourceManager
    {
        bool IsCompleted(TaskToken token);

        void Complete(TaskToken token, Exception? exception);

        void OnCompleted(TaskToken token, Action continuation);
        void UnsafeOnCompleted(TaskToken token, Action continuation);
    }
}
