﻿namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal interface IVoidCompletionSourceManager : ICompletionSourceManager
    {
        void GetResult(TaskToken token);
    }
}
