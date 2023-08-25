namespace SimEi.Threading.GameAwait.Internal.Source.Manager
{
    internal interface IResultCompletionSourceManager<TResult> : ICompletionSourceManager
    {
        TResult GetResult(AwaitableToken token);
    }
}
