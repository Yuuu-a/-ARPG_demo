using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PackageItem
{
    public int id;
    public ItemType type;
    public string name;

    [TextArea]
    public string description;

    [TextArea]
    public string effectDescription;

    [FormerlySerializedAs("WeaponIcon")]
    public Sprite ItemIcon;
    public Sprite TypeIcon;

    public bool IsStackable =>
        type == ItemType.Food || type == ItemType.Prop;
}

public enum ItemType
{
    Weapon = 1,
    Food = 2,
    Prop = 3,
    Equipment = 4
}
