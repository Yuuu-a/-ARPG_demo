using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public const int CurrentVersion = 1;

    public int version = CurrentVersion;
    public int characterId;
    public int level = 1;
    public int currentExperience;
    public int currentHealth = 1;

    // 装备系统接入后只保存装备实例引用，属性仍由装备配置统一计算。
    public List<EquippedItemSaveData> equippedItems =
        new List<EquippedItemSaveData>();
}

[Serializable]
public class EquippedItemSaveData
{
    public string slotId;
    public string itemInstanceId;
}
