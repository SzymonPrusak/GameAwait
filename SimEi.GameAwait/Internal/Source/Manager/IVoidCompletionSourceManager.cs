namespace SimEi.Threading.GameAwait.Internal.Source.Manager
{
    internal interface IVoidCompletionSourceManager : ICompletionSourceManager
    {
        void GetResult(AwaitableToken token);
    }
}
