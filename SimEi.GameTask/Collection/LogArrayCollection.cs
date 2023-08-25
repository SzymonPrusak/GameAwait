using System;
using System.Numerics;

namespace SimEi.Threading.GameTask.Collection
{
    internal class LogArrayCollection<T>
    {
        private readonly int _maxArrayCount;
        private readonly int _baseLevel;
        private readonly T[][] _arrays;
        private int _arrayCount;

        public LogArrayCollection(int maxArrayCount, int baseLevel = 1)
        {
            _maxArrayCount = maxArrayCount;
            _baseLevel = baseLevel;
            _arrays = new T[maxArrayCount][];
        }


        public int TotalCapacity => GetCapacityForArrayCount(_arrayCount, _baseLevel);


        public static int GetCapacityForArrayCount(int arrayCount, int baseLevel = 1)
        {
            return arrayCount == 0 ? 0 : (1 << (arrayCount + baseLevel)) - 1;
        }

        public ref T GetItem(int index)
        {
            int arrayIndex = Math.Max(0, 31 - BitOperations.LeadingZeroCount((uint)index + 1) - _baseLevel);
            if (arrayIndex >= _arrayCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            int baseIndex = arrayIndex == 0
                ? 0
                : (1 << (arrayIndex + _baseLevel)) - 1;
            int indexInArray = index - baseIndex;

            return ref _arrays[arrayIndex][indexInArray];
        }

        public void AllocateNextArray()
        {
            if (_arrayCount == _maxArrayCount)
                throw new InvalidOperationException($"Max array count of {_maxArrayCount} exceeded.");

            int size = _arrayCount == 0
                ? (1 << (_baseLevel + 1)) - 1
                : 1 << (_arrayCount + _baseLevel);
            var array = new T[size];
            _arrays[_arrayCount] = array;
            _arrayCount++;
        }
    }
}
