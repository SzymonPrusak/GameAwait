using SimEi.Threading.GameTask.Execution;
using SimEi.Threading.GameTask.Internal;
using SimEi.Threading.GameTask.Internal.Source.State;

namespace SimEi.Threading.GameTask
{
    partial struct GameTask
    {
        /// <summary>
        /// Delays execution until specified time passed on game loop execution thread for <typeparamref name="Timing"/>.
        /// <para>
        /// Call supports <see cref="System.Threading.SynchronizationContext"/>, but if not set
        ///  execution will be resumed from game loop execution thread.
        /// </para>
        /// </summary>
        public static GameTask Delay<Timing>(float seconds)
        {
            var state = new DelayState<Timing>
            {
                LeftTime = seconds
            };
            return AllocateTaskWithTracking(ref state);
        }


        internal struct DelayState<Timing> : ITrackedCompletionSourceState
        {
            internal float LeftTime;

            readonly bool ITrackedCompletionSourceState.IsCompleted => LeftTime <= 0;
        }


        internal static class DelayHandler<Timing>
        {
            private static readonly AwaitableTracker<DelayState<Timing>>.HandleCallback _delayHandler = HandleDelay;

            public static void Handle()
            {
                AwaitableTracker<DelayState<Timing>>.HandleActive(_delayHandler);
            }

            private static void HandleDelay(ref DelayState<Timing> state)
            {
                state.LeftTime -= Time<Timing>.LastTickDuration;
            }
        }
    }
}
