namespace O2un.Progression
{
    public readonly struct LevelUpEvent
    {
        public int NewLevel { get; }

        public LevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }
}
