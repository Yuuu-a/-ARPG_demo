public readonly struct CharacterStatsPreview
{
    public int Level { get; }
    public int MaxHealth { get; }
    public int Attack { get; }
    public int Defense { get; }

    public CharacterStatsPreview(
        int level,
        int maxHealth,
        int attack,
        int defense)
    {
        Level = level;
        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
    }
}
