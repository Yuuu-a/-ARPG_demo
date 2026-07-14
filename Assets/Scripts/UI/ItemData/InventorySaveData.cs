using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<InventorySlotSaveData> items = new List<InventorySlotSaveData>();
}

[Serializable]
public class InventorySlotSaveData
{
    // 不可堆叠物品的唯一实例 ID，用于区分同种武器。
    public string instanceId;
    public int itemId;
    public int count;
    public int slotIndex;

    // 武器运行时数据，会写入 InventorySave.json。
    public int level = 1;
    public int currentExperience;
    public int enhanceLevel;
}
