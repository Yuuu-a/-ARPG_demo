using UnityEngine;

public sealed class PackageItemViewData
{
    public PackageItemViewData(
        string selectionKey,
        Sprite itemIcon,
        Sprite typeIcon,
        int count,
        int level,
        string itemName,
        string description,
        string effectDescription,
        bool isEquipped = false,
        string baseAttackDescription = "",
        string basicAttributeDescription = "")
    {
        SelectionKey = selectionKey;
        ItemIcon = itemIcon;
        TypeIcon = typeIcon;
        Count = count;
        Level = level;
        ItemName = itemName;
        Description = description;
        EffectDescription = effectDescription;
        IsEquipped = isEquipped;
        BaseAttackDescription = baseAttackDescription;
        BasicAttributeDescription = basicAttributeDescription;
    }

    public string SelectionKey { get; }
    public Sprite ItemIcon { get; }
    public Sprite TypeIcon { get; }
    public int Count { get; }
    public int Level { get; }
    public string ItemName { get; }
    public string Description { get; }
    public string EffectDescription { get; }
    public bool IsEquipped { get; }
    public string BaseAttackDescription { get; }
    public string BasicAttributeDescription { get; }
}
