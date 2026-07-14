using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentPackageItem : PackageItem
{
    [Header("Equipment Stats")]
    [Min(0)] public int maxHealth;
    [Min(0)] public int attack;
    [Min(0)] public int defense;

    public string GetAttributeDescription()
    {
        List<string> attributes = new List<string>();

        if (maxHealth > 0)
        {
            attributes.Add($"生命值 +{maxHealth}");
        }

        if (attack > 0)
        {
            attributes.Add($"攻击力 +{attack}");
        }

        if (defense > 0)
        {
            attributes.Add($"防御力 +{defense}");
        }

        return string.Join("\n", attributes);
    }
}

[CreateAssetMenu(
    fileName = "EquipmentDataConfig",
    menuName = "Item/Equipment Data Config")]
public class EquipmentDataConfig : ScriptableObject
{
    [SerializeField]
    private List<EquipmentPackageItem> itemDataList =
        new List<EquipmentPackageItem>();

    public IReadOnlyList<EquipmentPackageItem> ItemDataList => itemDataList;
}
