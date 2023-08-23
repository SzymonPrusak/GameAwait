using System.Threading;

namespace SimEi.Threading.GameAwait.Internal
{
    internal struct FastSpinLock
    {
        private volatile int _value;

        public void Enter()
        {
            while (Interlocked.CompareExchange(ref _value, 1, 0) == 1) { }
        }

        public void Exit()
        {
            _value = 0;
        }
    }
}
