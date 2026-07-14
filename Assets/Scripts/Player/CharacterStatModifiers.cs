public readonly struct CharacterStatModifiers
{
    public static CharacterStatModifiers Zero => default;

    public int MaxHealth { get; }
    public int Attack { get; }
    public int Defense { get; }
    public float CriticalRate { get; }
    public float CriticalDamage { get; }
    public int Impact { get; }

    public CharacterStatModifiers(
        int maxHealth,
        int attack,
        int defense,
        float criticalRate,
        float criticalDamage,
        int impact)
    {
        MaxHealth = maxHealth;
        Attack = attack;
        Defense = defense;
        CriticalRate = criticalRate;
        CriticalDamage = criticalDamage;
        Impact = impact;
    }

    public static CharacterStatModifiers operator +(
        CharacterStatModifiers left,
        CharacterStatModifiers right)
    {
        return new CharacterStatModifiers(
            left.MaxHealth + right.MaxHealth,
            left.Attack + right.Attack,
            left.Defense + right.Defense,
            left.CriticalRate + right.CriticalRate,
            left.CriticalDamage + right.CriticalDamage,
            left.Impact + right.Impact);
    }
}
