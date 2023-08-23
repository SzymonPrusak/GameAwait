namespace SimEi.Threading.GameAwait.Internal
{
    public interface IResultCompletionSourceState<TRes> : ICompletionSourceState
    {
        TRes Result { get; }
    }
}
