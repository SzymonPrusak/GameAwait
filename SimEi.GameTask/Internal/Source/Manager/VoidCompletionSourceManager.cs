namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal class VoidCompletionSourceManager<T>
        : CompletionSourceManagerBase<T>, IVoidCompletionSourceManager
        where T : struct
    {
        public void GetResult(TaskToken token)
        {
            ref var source = ref CompletionSourcePool<T>.GetSource(token);
            var ex = source.Exception;

            source.GetResult();
            CompletionSourcePool<T>.Free(token);

            if (ex != null)
                throw ex;
        }
    }
}
