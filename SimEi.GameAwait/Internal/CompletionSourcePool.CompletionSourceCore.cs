using System;
using System.Diagnostics;
using System.Threading;
using SimEi.Threading.GameAwait.Execution;

namespace SimEi.Threading.GameAwait
{
    internal static partial class CompletionSourcePool<T>
    {
        internal struct CompletionSource
        {
            private static readonly SendOrPostCallback _completeCallback = CompleteCore;

            public T State;

            private ushort _generation;
            private SpinLock _completionLock;
            private bool _hasCompleted;
            private SynchronizationContext? _syncContext;
            private Action? _continuation;


            public readonly bool HasCompleted => _hasCompleted;
            public readonly ushort Generation => _generation;


            public void Activate()
            {
                Debug.Assert(!_hasCompleted);
                _syncContext = SynchronizationContext.Current;
            }

            public void Deactivate()
            {
                Debug.Assert(_hasCompleted);
                _generation++;
                _hasCompleted = false;
                _continuation = null;
            }


            public void UnsafeOnCompleted(Action continuation)
            {
                if (_continuation != null)
                    throw new InvalidOperationException("Multiple continuations are not supported.");

                bool lockTaken = false;
                _completionLock.Enter(ref lockTaken);

                Volatile.Write(ref _continuation, continuation);
                bool invokeImmediately = Volatile.Read(ref _hasCompleted);

                _completionLock.Exit();

                if (invokeImmediately)
                    continuation.Invoke();
            }

            public void Complete()
            {
                Debug.Assert(!_hasCompleted);

                bool lockTaken = false;
                _completionLock.Enter(ref lockTaken);

                Volatile.Write(ref _hasCompleted, true);
                var cont = Volatile.Read(ref _continuation);

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


            private static void CompleteCore(object? state)
            {
                var continuation = (Action)state!;
                continuation.Invoke();
            }
        }
    }
}
