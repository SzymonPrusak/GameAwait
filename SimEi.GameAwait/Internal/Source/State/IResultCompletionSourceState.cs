namespace SimEi.Threading.GameAwait.Internal.Source.State
{
    public interface IResultCompletionSourceState<TRes>
    {
        TRes Result { get; }
    }
}
