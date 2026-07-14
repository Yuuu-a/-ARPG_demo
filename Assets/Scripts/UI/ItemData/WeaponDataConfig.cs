using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponPackageItem : PackageItem
{
    [Header("Base Weapon Stats")]
    [Min(0)] public int baseAttack;

    [Header("Base Effects (%)")]
    [Min(0f)] public float criticalRateBonusPercent;
    [Min(0f)] public float attackBonusPercent;
    [Min(0f)] public float maxHealthBonusPercent;

    public string GetWeaponEffectDescription()
    {
        List<string> effects = new List<string>();

        if (criticalRateBonusPercent > 0f)
        {
            effects.Add(
                $"暴击率提升 {criticalRateBonusPercent:0.##}%");
        }

        if (attackBonusPercent > 0f)
        {
            effects.Add($"攻击力提升 {attackBonusPercent:0.##}%");
        }

        if (maxHealthBonusPercent > 0f)
        {
            effects.Add(
                $"生命值提升 {maxHealthBonusPercent:0.##}%");
        }

        return string.Join("\n", effects);
    }
}

[CreateAssetMenu(
    fileName = "WeaponDataConfig",
    menuName = "Item/Weapon Data Config")]
public class WeaponDataConfig : ScriptableObject
{
    [SerializeField]
    private List<WeaponPackageItem> itemDataList =
        new List<WeaponPackageItem>();

    public IReadOnlyList<WeaponPackageItem> ItemDataList => itemDataList;
}
