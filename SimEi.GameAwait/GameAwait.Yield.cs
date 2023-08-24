
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    public static partial class GameAwait
    {
        /// <summary>
        /// Delays execution to next <paramref name="tickCount"/> ticks of specified <typeparamref name="Timing"/>.
        /// <para>
        /// Call supports <see cref="System.Threading.SynchronizationContext"/>, but if not set
        ///  execution will be resumed from game loop execution thread.
        /// </para>
        /// </summary>
        public static Awaitable<YieldState<Timing>> Yield<Timing>(int tickCount = 1)
        {
            var state = new YieldState<Timing>
            {
                LeftTicks = tickCount
            };
            return AllocateAwaitableWithTracking(ref state);
        }


        public struct YieldState<Timing> : ITrackedCompletionSourceState
        {
            internal int LeftTicks;

            bool ITrackedCompletionSourceState.IsCompleted => LeftTicks == 0;
        }


        internal static class YieldHandler<Timing>
        {
            private static readonly TaskTracker<YieldState<Timing>>.HandleCallback _yieldHandler = HandleYield;

            public static void Handle()
            {
                TaskTracker<YieldState<Timing>>.HandleActive(_yieldHandler);
            }

            private static void HandleYield(ref YieldState<Timing> core)
            {
                core.LeftTicks--;
            }
        }
    }
}
