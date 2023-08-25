namespace SimEi.Threading.GameTask.Internal.Source.State
{
    internal interface IResultCompletionSourceState<TRes>
    {
        TRes Result { get; }
    }
}
