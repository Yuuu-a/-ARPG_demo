using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterBaseConfig",
    menuName = "Character/Character Base Config")]
public class CharacterBaseConfig : ScriptableObject
{
    [Header("角色信息")]
    [Min(1)]
    [SerializeField] private int characterId = 1;
    [SerializeField] private string characterName;
    [Min(1)]
    [SerializeField] private int initialLevel = 1;

    [Header("基础属性")]
    [Min(1)]
    [SerializeField] private int baseMaxHealth = 100;
    [Min(0)]
    [SerializeField] private int baseAttack = 10;
    [Min(0)]
    [SerializeField] private int baseDefense = 5;
    [Range(0f, 1f)]
    [SerializeField] private float baseCriticalRate = 0.05f;
    [Min(1f)]
    [SerializeField] private float baseCriticalDamage = 1.5f;
    [Min(0)]
    [SerializeField] private int baseImpact = 10;

    [Header("每级成长")]
    [Min(0)]
    [SerializeField] private int healthPerLevel = 10;
    [Min(0)]
    [SerializeField] private int attackPerLevel = 2;
    [Min(0)]
    [SerializeField] private int defensePerLevel = 1;

    public int CharacterId => characterId;
    public string CharacterName => characterName;
    public int InitialLevel => initialLevel;
    public int BaseMaxHealth => baseMaxHealth;
    public int BaseAttack => baseAttack;
    public int BaseDefense => baseDefense;
    public float BaseCriticalRate => baseCriticalRate;
    public float BaseCriticalDamage => baseCriticalDamage;
    public int BaseImpact => baseImpact;
    public int HealthPerLevel => healthPerLevel;
    public int AttackPerLevel => attackPerLevel;
    public int DefensePerLevel => defensePerLevel;

    private void OnValidate()
    {
        characterId = Mathf.Max(1, characterId);
        initialLevel = Mathf.Max(1, initialLevel);
        baseMaxHealth = Mathf.Max(1, baseMaxHealth);
        baseAttack = Mathf.Max(0, baseAttack);
        baseDefense = Mathf.Max(0, baseDefense);
        baseCriticalRate = Mathf.Clamp01(baseCriticalRate);
        baseCriticalDamage = Mathf.Max(1f, baseCriticalDamage);
        baseImpact = Mathf.Max(0, baseImpact);
        healthPerLevel = Mathf.Max(0, healthPerLevel);
        attackPerLevel = Mathf.Max(0, attackPerLevel);
        defensePerLevel = Mathf.Max(0, defensePerLevel);
    }
}
