namespace SimEi.Threading.GameAwait.Internal
{
    public interface IResultCompletionSourceState<TRes>
    {
        TRes Result { get; }
    }
}
