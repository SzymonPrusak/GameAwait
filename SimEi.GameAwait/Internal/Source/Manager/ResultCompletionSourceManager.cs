﻿using SimEi.Threading.GameAwait.Internal.Source.State;

namespace SimEi.Threading.GameAwait.Internal.Source.Manager
{
    internal class ResultCompletionSourceManager<T, TResult>
        : CompletionSourceManagerBase<T>, IResultCompletionSourceManager<TResult>
        where T : struct, IResultCompletionSourceState<TResult>
    {
        public TResult GetResult(AwaitableToken token)
        {
            ref var source = ref CompletionSourcePool<T>.GetSource(token);
            var ex = source.Exception;
            ref var state = ref source.State;
            var res = state.Result;

            source.GetResult();
            CompletionSourcePool<T>.Free(token);

            if (ex != null)
                throw ex;

            return res;
        }
    }
}
