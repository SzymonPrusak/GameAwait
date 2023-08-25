namespace SimEi.Threading.GameTask.Internal
{
    internal readonly struct TaskToken
    {
        public readonly object? Reference;
        private readonly uint _token;

        public TaskToken(ushort index, ushort generation)
        {
            Reference = null;
            _token = index | (uint)(generation << 16);
        }

        public TaskToken(object reference)
        {
            Reference = reference;
            _token = uint.MaxValue;
        }


        public ushort Index => (ushort)(_token & 0xFFFF);
        public ushort Generation => (ushort)(_token >> 16);
    }
}
