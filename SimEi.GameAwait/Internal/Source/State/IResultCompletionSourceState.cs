namespace SimEi.Threading.GameAwait.Internal.Source.State
{
    internal interface IResultCompletionSourceState<TRes>
    {
        TRes Result { get; }
    }
}
