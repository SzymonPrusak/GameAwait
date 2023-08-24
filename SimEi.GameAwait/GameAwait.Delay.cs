using SimEi.Threading.GameAwait.Execution;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    public static partial class GameAwait
    {
        /// <summary>
        /// Delays execution until specified time passed on game loop execution thread for <typeparamref name="Timing"/>.
        /// <para>
        /// Call supports <see cref="System.Threading.SynchronizationContext"/>, but if not set
        ///  execution will be resumed from game loop execution thread.
        /// </para>
        /// </summary>
        public static Awaitable<DelayState<Timing>> Delay<Timing>(float seconds)
        {
            var source = new DelayState<Timing>
            {
                LeftTime = seconds
            };
            return AllocateAwaitableWithTracking(ref source);
        }


        public struct DelayState<Timing> : ITrackedCompletionSourceState
        {
            internal float LeftTime;

            bool ITrackedCompletionSourceState.IsCompleted => LeftTime <= 0;
        }


        internal static class DelayHandler<Timing>
        {
            private static readonly TaskTracker<DelayState<Timing>>.HandleCallback _delayHandler = HandleDelay;

            public static void Handle()
            {
                TaskTracker<DelayState<Timing>>.HandleActive(_delayHandler);
            }

            private static void HandleDelay(ref DelayState<Timing> state)
            {
                state.LeftTime -= Time<Timing>.LastTickDuration;
            }
        }
    }
}
