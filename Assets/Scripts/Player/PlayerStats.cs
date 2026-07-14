using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("角色配置")]
    [SerializeField] private CharacterBaseConfig baseConfig;

    private int level;
    private int currentHealth;
    private CharacterStatModifiers equipmentModifiers;
    private CharacterStatModifiers buffModifiers;

    public event Action StatsChanged;

    public CharacterBaseConfig BaseConfig => baseConfig;
    public string CharacterName =>
        baseConfig == null ? string.Empty : baseConfig.CharacterName;
    public int Level => level;
    public int CurrentHealth => currentHealth;
    public int BaseMaxHealthValue => GetBaseMaxHealth();
    public int BaseAttackValue => GetBaseAttack();

    public int MaxHealth => Mathf.Max(
        1,
        GetBaseMaxHealth() + TotalModifiers.MaxHealth);

    public int Attack => Mathf.Max(
        0,
        GetBaseAttack() + TotalModifiers.Attack);

    public int Defense => Mathf.Max(
        0,
        GetBaseDefense() + TotalModifiers.Defense);

    public float CriticalRate => Mathf.Clamp01(
        GetBaseCriticalRate() + TotalModifiers.CriticalRate);

    public float CriticalDamage => Mathf.Max(
        1f,
        GetBaseCriticalDamage() + TotalModifiers.CriticalDamage);

    public int Impact => Mathf.Max(
        0,
        GetBaseImpact() + TotalModifiers.Impact);

    private CharacterStatModifiers TotalModifiers =>
        equipmentModifiers + buffModifiers;

    private void Awake()
    {
        if (baseConfig == null)
        {
            Debug.LogError(
                $"{nameof(PlayerStats)} 没有配置 CharacterBaseConfig。",
                this);
            level = 1;
            currentHealth = 1;
            return;
        }

        level = baseConfig.InitialLevel;
        currentHealth = MaxHealth;
    }

    private void OnEnable()
    {
        EquipmentLoadoutManager.RegisterPlayerStats(this);
    }

    private void OnDisable()
    {
        EquipmentLoadoutManager.UnregisterPlayerStats(this);
    }

    public void SetLevel(int newLevel)
    {
        int validatedLevel = Mathf.Max(1, newLevel);

        if (level == validatedLevel)
        {
            return;
        }

        level = validatedLevel;
        currentHealth = Mathf.Min(currentHealth, MaxHealth);
        NotifyStatsChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        NotifyStatsChanged();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth >= MaxHealth)
        {
            return;
        }

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        NotifyStatsChanged();
    }

    public void RestoreHealth(int health)
    {
        int validatedHealth = Mathf.Clamp(health, 0, MaxHealth);

        if (currentHealth == validatedHealth)
        {
            return;
        }

        currentHealth = validatedHealth;
        NotifyStatsChanged();
    }

    public void RestoreBaseConfig(CharacterBaseConfig config)
    {
        if (config == null)
        {
            Debug.LogError("无法恢复角色配置：配置不能为空。", this);
            return;
        }

        if (baseConfig == config)
        {
            return;
        }

        baseConfig = config;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        NotifyStatsChanged();
    }

    public void SetEquipmentModifiers(CharacterStatModifiers modifiers)
    {
        equipmentModifiers = modifiers;
        ClampCurrentHealthAndNotify();
    }

    public void SetBuffModifiers(CharacterStatModifiers modifiers)
    {
        buffModifiers = modifiers;
        ClampCurrentHealthAndNotify();
    }

    public CharacterStatsPreview GetStatsPreview(int targetLevel)
    {
        int validatedLevel = Mathf.Max(1, targetLevel);
        CharacterStatModifiers modifiers = TotalModifiers;

        int previewMaxHealth = baseConfig == null
            ? 1
            : baseConfig.BaseMaxHealth +
              (validatedLevel - 1) * baseConfig.HealthPerLevel;
        int previewAttack = baseConfig == null
            ? 0
            : baseConfig.BaseAttack +
              (validatedLevel - 1) * baseConfig.AttackPerLevel;
        int previewDefense = baseConfig == null
            ? 0
            : baseConfig.BaseDefense +
              (validatedLevel - 1) * baseConfig.DefensePerLevel;

        return new CharacterStatsPreview(
            validatedLevel,
            Mathf.Max(1, previewMaxHealth + modifiers.MaxHealth),
            Mathf.Max(0, previewAttack + modifiers.Attack),
            Mathf.Max(0, previewDefense + modifiers.Defense));
    }

    private int GetBaseMaxHealth()
    {
        if (baseConfig == null)
        {
            return 1;
        }

        return baseConfig.BaseMaxHealth +
               (level - 1) * baseConfig.HealthPerLevel;
    }

    private int GetBaseAttack()
    {
        if (baseConfig == null)
        {
            return 0;
        }

        return baseConfig.BaseAttack +
               (level - 1) * baseConfig.AttackPerLevel;
    }

    private int GetBaseDefense()
    {
        if (baseConfig == null)
        {
            return 0;
        }

        return baseConfig.BaseDefense +
               (level - 1) * baseConfig.DefensePerLevel;
    }

    private float GetBaseCriticalRate()
    {
        return baseConfig == null ? 0f : baseConfig.BaseCriticalRate;
    }

    private float GetBaseCriticalDamage()
    {
        return baseConfig == null ? 1f : baseConfig.BaseCriticalDamage;
    }

    private int GetBaseImpact()
    {
        return baseConfig == null ? 0 : baseConfig.BaseImpact;
    }

    private void ClampCurrentHealthAndNotify()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        NotifyStatsChanged();
    }

    private void NotifyStatsChanged()
    {
        StatsChanged?.Invoke();
    }
}
