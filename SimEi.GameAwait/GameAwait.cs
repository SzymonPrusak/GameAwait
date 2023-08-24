using System.Runtime.CompilerServices;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    /// <summary>
    /// Provides zero-allocation integration of <see langword="async"/>/<see langword="await"/> with game loop.
    /// </summary>
    public static partial class GameAwait
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Awaitable<T> AllocateAwaitableWithTracking<T>(ref T core)
            where T : struct, ITrackedCompletionSourceState
        {
            var token = CompletionSourcePool<T>.AllocateAndActivate(ref core);
            TaskTracker<T>.Register(token);
            return new(token);
        }
    }
}
