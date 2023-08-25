using SimEi.Threading.GameAwait.Internal;
using SimEi.Threading.GameAwait.Internal.Source.Manager;
using SimEi.Threading.GameAwait.Internal.Source.State;

namespace SimEi.Threading.GameAwait
{
    /// <summary>
    /// Provides zero-allocation integration of <see langword="async"/>/<see langword="await"/> with game loop.
    /// </summary>
    public static partial class GameAwait
    {
        private static GameTask AllocateTaskWithTracking<T>(ref T state)
            where T : struct, ITrackedCompletionSourceState
        {
            var sm = CompletionSourceManagers.Void<T>.Instance;
            ref var source = ref sm.AllocateAndActivate(out var token);
            source.State = state;
            AwaitableTracker<T>.Register(token);
            return new(token, sm);
        }
    }
}
