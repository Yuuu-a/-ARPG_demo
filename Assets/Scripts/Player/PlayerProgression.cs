using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("运行时属性")]
    [SerializeField] private PlayerStats playerStats;

    [Header("升级规则")]
    [Min(1)]
    [SerializeField] private int baseRequiredExperience = 100;
    [Min(0)]
    [SerializeField] private int requiredExperienceGrowthPerLevel = 25;
    [Min(1)]
    [SerializeField] private int maxLevel = 60;

    private int currentExperience;

    public event Action<int, int> ExperienceChanged;
    public event Action<int> LevelChanged;

    public int Level => playerStats == null ? 1 : playerStats.Level;
    public int CurrentExperience => currentExperience;
    public int RequiredExperience => CalculateRequiredExperience(Level);
    public int MaxLevel => maxLevel;
    public bool IsMaxLevel => Level >= maxLevel;
    public bool CanUpgrade =>
        !IsMaxLevel && currentExperience >= RequiredExperience;

    private void Awake()
    {
        ResolvePlayerStats();
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError(
                $"{nameof(PlayerProgression)} 没有找到 PlayerStats。",
                this);
            return;
        }

        if (playerStats.Level > maxLevel)
        {
            playerStats.SetLevel(maxLevel);
        }

        NotifyProgressChanged();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0 || IsMaxLevel)
        {
            return;
        }

        long result = (long)currentExperience + amount;
        currentExperience = result > int.MaxValue
            ? int.MaxValue
            : (int)result;

        ExperienceChanged?.Invoke(currentExperience, RequiredExperience);
    }

    public bool TryUpgrade()
    {
        if (playerStats == null || !CanUpgrade)
        {
            return false;
        }

        CalculateProjectedProgress(
            out int projectedLevel,
            out int remainingExperience);

        playerStats.SetLevel(projectedLevel);
        currentExperience = IsMaxLevel ? 0 : remainingExperience;

        LevelChanged?.Invoke(Level);
        ExperienceChanged?.Invoke(currentExperience, RequiredExperience);
        return true;
    }

    public int GetProjectedLevel()
    {
        CalculateProjectedProgress(
            out int projectedLevel,
            out _);
        return projectedLevel;
    }

    public void RestoreProgress(int savedLevel, int savedExperience)
    {
        ResolvePlayerStats();

        if (playerStats == null)
        {
            Debug.LogError(
                "无法恢复角色成长数据：缺少 PlayerStats。",
                this);
            return;
        }

        int validatedLevel = Mathf.Clamp(savedLevel, 1, maxLevel);
        currentExperience = validatedLevel >= maxLevel
            ? 0
            : Mathf.Max(0, savedExperience);

        playerStats.SetLevel(validatedLevel);
        NotifyProgressChanged();
    }

    public int CalculateRequiredExperience(int targetLevel)
    {
        int validatedLevel = Mathf.Max(1, targetLevel);
        long required = (long)baseRequiredExperience +
                        (long)(validatedLevel - 1) *
                        requiredExperienceGrowthPerLevel;

        return (int)Math.Min(int.MaxValue, Math.Max(1L, required));
    }

    private void ResolvePlayerStats()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
    }

    private void NotifyProgressChanged()
    {
        LevelChanged?.Invoke(Level);
        ExperienceChanged?.Invoke(currentExperience, RequiredExperience);
    }

    private void CalculateProjectedProgress(
        out int projectedLevel,
        out int remainingExperience)
    {
        projectedLevel = Level;
        remainingExperience = currentExperience;

        while (projectedLevel < maxLevel)
        {
            int requiredExperience =
                CalculateRequiredExperience(projectedLevel);

            if (remainingExperience < requiredExperience)
            {
                break;
            }

            remainingExperience -= requiredExperience;
            projectedLevel++;
        }

        if (projectedLevel >= maxLevel)
        {
            remainingExperience = 0;
        }
    }

    private void OnValidate()
    {
        baseRequiredExperience = Mathf.Max(1, baseRequiredExperience);
        requiredExperienceGrowthPerLevel =
            Mathf.Max(0, requiredExperienceGrowthPerLevel);
        maxLevel = Mathf.Max(1, maxLevel);
    }
}
