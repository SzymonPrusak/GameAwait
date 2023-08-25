using System;
using System.Collections.Generic;
using SimEi.Collections;
using SimEi.Threading.GameTask.Internal.Source;
using SimEi.Threading.GameTask.Internal.Source.State;

namespace SimEi.Threading.GameTask.Internal
{
    internal class TaskTracker<T>
        where T : struct, ITrackedCompletionSourceState
    {
        private static readonly PooledLinkedList<TaskToken> _activeTasks = new(16);
        private static readonly List<TaskToken> _tasksToAdd = new(8);


        public delegate void HandleCallback(ref T state);


        public static void Register(TaskToken token)
        {
            lock (_tasksToAdd)
                _tasksToAdd.Add(token);
        }

        public static void HandleActive(HandleCallback callback)
        {
            var handleN = _activeTasks.First;
            while (handleN.HasValue)
            {
                var handle = handleN.Value;
                handleN = handle.Next;
                if (Handle(callback, handle.Value))
                    _activeTasks.Remove(handle);
            }

            var prevLast = _activeTasks.Last;
            lock (_tasksToAdd)
            {
                for (int i = 0; i < _tasksToAdd.Count; i++)
                    _activeTasks.AddLast(_tasksToAdd[i]);
                _tasksToAdd.Clear();
            }

            handleN = _activeTasks.Last;
            while (!Nullable.Equals(handleN, prevLast))
            {
                // Null forgiven - if handleN is null, then prevLast also has to be null,
                //  so loop condition check fails.
                var handle = handleN!.Value;
                handleN = handle.Prev;
                if (Handle(callback, handle.Value))
                    _activeTasks.Remove(handle);
            }
        }

        private static bool Handle(HandleCallback callback, TaskToken token)
        {
            ref var source = ref CompletionSourcePool<T>.UnvalidatedGetSource(token);
            callback.Invoke(ref source.State);

            if (!source.State.IsCompleted)
                return false;
            source.Complete(null);
            return true;
        }
    }
}
