using System;
using System.Diagnostics;
using System.Threading;
using SimEi.Threading.GameAwait.Execution;
using SimEi.Threading.GameAwait.Internal;

namespace SimEi.Threading.GameAwait
{
    internal static partial class CompletionSourcePool<T>
        where T : ICompletionSourceState, new()
    {
        internal struct CompletionSource
        {
            private static readonly SendOrPostCallback _completeCallback = CompleteCore;

            public T State;

            private ushort _generation;
            private FastSpinLock _completionLock;
            private bool _isActive;
            private bool _hasCompleted;
            private SynchronizationContext? _syncContext;
            private Action? _continuation;


            public readonly bool IsActive => _isActive;
            public readonly bool HasCompleted => _hasCompleted;
            public readonly ushort Generation => _generation;


            public void Activate()
            {
                Debug.Assert(!_isActive);
                Debug.Assert(!_hasCompleted);
                _isActive = true;
                _syncContext = SynchronizationContext.Current;
            }

            public void Deactivate()
            {
                Debug.Assert(_isActive);
                Debug.Assert(_hasCompleted);
                _generation++;
                _hasCompleted = false;
                _isActive = false;
                _continuation = null;
            }


            public void UnsafeOnCompleted(Action continuation)
            {
                if (_continuation != null)
                    throw new InvalidOperationException("Multiple continuations are not supported.");

                _completionLock.Enter();
                _continuation = continuation;
                bool invoke = _hasCompleted;
                _completionLock.Exit();

                if (invoke)
                    continuation.Invoke();
            }

            public void Complete()
            {
                Debug.Assert(!_hasCompleted);

                _completionLock.Enter();
                _hasCompleted = true;
                bool invoke = _continuation != null;
                _completionLock.Exit();

                if (!invoke)
                    return;

                if (_syncContext != null)
                {
                    _syncContext.Post(_completeCallback, _continuation);
                }
                else
                {
                    try
                    {
                        _continuation?.Invoke();
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
