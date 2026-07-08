using O2un.DataStore;
using R3;

namespace O2un.Combat
{
    public sealed class PlayerHealthAdapter : IHealth
    {
        private readonly IPlayerDataReader _reader;
        private readonly IPlayerDataWriter _writer;

        public PlayerHealthAdapter(IPlayerDataReader reader, IPlayerDataWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public ReadOnlyReactiveProperty<int> CurrentHP => _reader.CurrentHP;
        public int MaxHP => _reader.MaxHP.CurrentValue;

        public void VaryHP(int delta)
        {
            _writer.VaryHP(delta);
        }
    }
}
