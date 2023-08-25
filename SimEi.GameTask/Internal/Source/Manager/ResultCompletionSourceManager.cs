using SimEi.Threading.GameTask.Internal.Source.State;

namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal class ResultCompletionSourceManager<T, TResult>
        : CompletionSourceManagerBase<T>, IResultCompletionSourceManager<TResult>
        where T : struct, IResultCompletionSourceState<TResult>
    {
        public TResult GetResult(TaskToken token)
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
