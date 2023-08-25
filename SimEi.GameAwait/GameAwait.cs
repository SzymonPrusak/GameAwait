using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    /// <summary>
    /// Provides zero-allocation integration of <see langword="async"/>/<see langword="await"/> with game loop.
    /// </summary>
    public static partial class GameAwait
    {
        private static Awaitable<T> AllocateAwaitableWithTracking<T>(ref T state)
            where T : struct, ITrackedCompletionSourceState
        {
            ref var source = ref CompletionSourcePool<T>.Allocate(out var token);
            source.State = state;
            AwaitableTracker<T>.Register(token);
            return new(token);
        }
    }
}
