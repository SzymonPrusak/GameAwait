using SimEi.Threading.GameAwait.Internal.Source.State;

namespace SimEi.Threading.GameAwait.Internal.Source.Manager
{
    internal static class CompletionSourceManagers
    {
        public static class Void<T>
            where T : struct
        {
            public static readonly VoidCompletionSourceManager<T> Instance = new();
        }

        public static class Result<T, TResult>
            where T : struct, IResultCompletionSourceState<TResult>
        {
            public static readonly ResultCompletionSourceManager<T, TResult> Instance = new();
        }
    }
}
