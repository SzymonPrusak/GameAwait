using System;
using System.Diagnostics;
using System.Threading;
using SimEi.Threading.GameTask.Execution;

namespace SimEi.Threading.GameTask.Internal.Source
{
    internal struct CompletionSource<T>
    {
        private static readonly SendOrPostCallback _completeCallback = CompleteCore;

        public T State;

        private ushort _generation;
        private SpinLock _completionLock;
        private bool _hasCompleted;
        private SynchronizationContext? _syncContext;
        private volatile Action? _continuation;
        private Exception? _exception;


        public readonly ushort Generation => _generation;
        public readonly bool HasCompleted => _hasCompleted;
        public readonly Exception? Exception => _exception;


        public void Activate()
        {
            Debug.Assert(!_hasCompleted);
            _syncContext = SynchronizationContext.Current;
        }


        public void GetResult()
        {
            if (!_hasCompleted)
                throw new InvalidOperationException("Calling GetResult() before completion is not supported.");
            Deactivate();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (_continuation != null)
                throw new InvalidOperationException("Multiple continuations are not supported.");

            bool lockTaken = false;
            _completionLock.Enter(ref lockTaken);

            _continuation = continuation;
            bool invokeImmediately = Volatile.Read(ref _hasCompleted);

            _completionLock.Exit();

            if (invokeImmediately)
                continuation.Invoke();
        }

        public void Complete(Exception? ex)
        {
            Debug.Assert(!_hasCompleted);

            _exception = ex;

            bool lockTaken = false;
            _completionLock.Enter(ref lockTaken);

            Volatile.Write(ref _hasCompleted, true);
            var cont = _continuation;

            _completionLock.Exit();

            if (cont == null)
                return;

            if (_syncContext != null)
            {
                _syncContext.Post(_completeCallback, cont);
            }
            else
            {
                try
                {
                    cont?.Invoke();
                }
                catch (Exception e)
                {
                    ExceptionHandler.OnException(e);
                }
            }
        }


        private void Deactivate()
        {
            _generation++;
            _hasCompleted = false;
            _continuation = null;
            _exception = null;
        }


        private static void CompleteCore(object? state)
        {
            var continuation = (Action)state!;
            continuation.Invoke();
        }
    }
}
