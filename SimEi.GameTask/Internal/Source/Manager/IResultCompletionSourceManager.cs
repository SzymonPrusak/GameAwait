namespace SimEi.Threading.GameTask.Internal.Source.Manager
{
    internal interface IResultCompletionSourceManager<TResult> : ICompletionSourceManager
    {
        TResult GetResult(TaskToken token);
    }
}
