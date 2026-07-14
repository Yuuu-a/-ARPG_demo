using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class InventoryManager
{
    private const string SaveFileName = "InventorySave.json";

    private static InventorySaveData inventoryData;

    public static event Action InventoryChanged;

    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);

    public static IReadOnlyList<InventorySlotSaveData> Items
    {
        get
        {
            EnsureLoaded();
            return inventoryData.items;
        }
    }

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            inventoryData = new InventorySaveData();
            InventoryChanged?.Invoke();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            inventoryData = JsonUtility.FromJson<InventorySaveData>(json);

            if (inventoryData == null)
            {
                inventoryData = new InventorySaveData();
            }

            if (inventoryData.items == null)
            {
                inventoryData.items = new List<InventorySlotSaveData>();
            }

            RemoveInvalidItems();
            MigrateAndNormalizeItems();
            InventoryChanged?.Invoke();
            Debug.Log($"背包读取成功，共 {inventoryData.items.Count} 个格子：{SavePath}");
        }
        catch (Exception exception)
        {
            inventoryData = new InventorySaveData();
            Debug.LogError($"背包存档读取失败：{exception.Message}");
        }
    }

    public static bool Save()
    {
        EnsureLoaded();

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(inventoryData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"背包保存成功：{SavePath}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"背包保存失败：{exception.Message}");
            return false;
        }
    }

    public static bool AddItem(int itemId, int count = 1, int slotIndex = -1)
    {
        EnsureLoaded();

        if (itemId <= 0 || count <= 0)
        {
            Debug.LogWarning("添加物品失败：itemId 和 count 必须大于 0");
            return false;
        }

        PackageItem itemConfig = ItemDataManager.Instance == null
            ? null
            : ItemDataManager.Instance.GetItemData(itemId);

        if (itemConfig == null)
        {
            Debug.LogWarning($"添加物品失败：SO 中不存在物品 ID {itemId}");
            return false;
        }

        bool isStackable = itemConfig.IsStackable;

        if (!isStackable)
        {
            return AddNonStackableItems(itemId, count, slotIndex);
        }

        if (slotIndex >= 0)
        {
            InventorySlotSaveData targetSlot = GetSlot(slotIndex);

            if (targetSlot != null)
            {
                if (targetSlot.itemId != itemId)
                {
                    Debug.LogWarning($"添加物品失败：背包格子 {slotIndex} 已被占用");
                    return false;
                }

                targetSlot.count += count;
                InventoryChanged?.Invoke();
                return true;
            }
        }
        else
        {
            InventorySlotSaveData existingSlot =
                inventoryData.items.Find(item => item.itemId == itemId);

            if (existingSlot != null)
            {
                existingSlot.count += count;
                InventoryChanged?.Invoke();
                return true;
            }

            slotIndex = FindFirstEmptySlotIndex();
        }

        inventoryData.items.Add(new InventorySlotSaveData
        {
            itemId = itemId,
            count = count,
            slotIndex = slotIndex,
            level = 1
        });

        InventoryChanged?.Invoke();
        return true;
    }

    public static bool RemoveItem(int itemId, int count = 1)
    {
        EnsureLoaded();

        if (count <= 0)
        {
            Debug.LogWarning("移除物品失败：count 必须大于 0");
            return false;
        }

        if (GetItemCount(itemId) < count)
        {
            Debug.LogWarning($"移除物品失败：物品 {itemId} 数量不足");
            return false;
        }

        int remainingCount = count;

        for (int index = inventoryData.items.Count - 1;
             index >= 0 && remainingCount > 0;
             index--)
        {
            InventorySlotSaveData slot = inventoryData.items[index];

            if (slot.itemId != itemId)
            {
                continue;
            }

            int removeCount = Mathf.Min(slot.count, remainingCount);
            slot.count -= removeCount;
            remainingCount -= removeCount;

            if (slot.count == 0)
            {
                inventoryData.items.RemoveAt(index);
            }
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public static int GetItemCount(int itemId)
    {
        EnsureLoaded();

        int totalCount = 0;

        foreach (InventorySlotSaveData slot in inventoryData.items)
        {
            if (slot.itemId == itemId)
            {
                totalCount += slot.count;
            }
        }

        return totalCount;
    }

    public static InventorySlotSaveData GetSlot(int slotIndex)
    {
        EnsureLoaded();
        return inventoryData.items.Find(item => item.slotIndex == slotIndex);
    }

    public static InventorySlotSaveData GetItemInstance(string instanceId)
    {
        EnsureLoaded();

        if (string.IsNullOrEmpty(instanceId))
        {
            return null;
        }

        return inventoryData.items.Find(item =>
            item.instanceId == instanceId);
    }

    public static bool AddWeaponExperience(string instanceId, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("添加武器经验失败：amount 必须大于 0。");
            return false;
        }

        InventorySlotSaveData weapon = GetWeaponInstance(instanceId);
        if (weapon == null)
        {
            return false;
        }

        weapon.currentExperience += amount;
        InventoryChanged?.Invoke();
        return true;
    }

    public static bool UpgradeWeapon(string instanceId)
    {
        InventorySlotSaveData weapon = GetWeaponInstance(instanceId);
        if (weapon == null)
        {
            return false;
        }

        weapon.level++;
        InventoryChanged?.Invoke();
        return true;
    }

    public static bool EnhanceWeapon(string instanceId, int amount = 1)
    {
        if (amount <= 0)
        {
            return false;
        }

        InventorySlotSaveData weapon = GetWeaponInstance(instanceId);
        if (weapon == null)
        {
            return false;
        }

        weapon.enhanceLevel += amount;
        InventoryChanged?.Invoke();
        return true;
    }

    public static bool RemoveItemInstance(string instanceId)
    {
        InventorySlotSaveData item = GetItemInstance(instanceId);
        if (item == null)
        {
            return false;
        }

        inventoryData.items.Remove(item);
        InventoryChanged?.Invoke();
        return true;
    }

    public static void Clear()
    {
        EnsureLoaded();
        inventoryData.items.Clear();
        InventoryChanged?.Invoke();
    }

    private static void EnsureLoaded()
    {
        if (inventoryData == null)
        {
            Load();
        }
    }

    private static int FindFirstEmptySlotIndex()
    {
        int slotIndex = 0;

        while (GetSlot(slotIndex) != null)
        {
            slotIndex++;
        }

        return slotIndex;
    }

    private static bool AddNonStackableItems(
        int itemId,
        int count,
        int firstSlotIndex)
    {
        if (firstSlotIndex >= 0)
        {
            for (int offset = 0; offset < count; offset++)
            {
                int targetSlotIndex = firstSlotIndex + offset;

                if (GetSlot(targetSlotIndex) != null)
                {
                    Debug.LogWarning(
                        $"添加物品失败：背包格子 {targetSlotIndex} 已被占用");
                    return false;
                }
            }
        }

        for (int index = 0; index < count; index++)
        {
            int targetSlotIndex = firstSlotIndex >= 0
                ? firstSlotIndex + index
                : FindFirstEmptySlotIndex();

            inventoryData.items.Add(new InventorySlotSaveData
            {
                instanceId = Guid.NewGuid().ToString("N"),
                itemId = itemId,
                count = 1,
                slotIndex = targetSlotIndex,
                level = 1,
                currentExperience = 0,
                enhanceLevel = 0
            });
        }

        InventoryChanged?.Invoke();
        return true;
    }

    private static void RemoveInvalidItems()
    {
        inventoryData.items.RemoveAll(item =>
            item == null ||
            item.itemId <= 0 ||
            item.count <= 0 ||
            item.slotIndex < 0 ||
            (ItemDataManager.Instance != null &&
             !ItemDataManager.Instance.ContainsItem(item.itemId)));
    }

    private static InventorySlotSaveData GetWeaponInstance(string instanceId)
    {
        InventorySlotSaveData item = GetItemInstance(instanceId);
        if (item == null)
        {
            Debug.LogWarning($"没有找到物品实例：{instanceId}");
            return null;
        }

        PackageItem config = ItemDataManager.Instance == null
            ? null
            : ItemDataManager.Instance.GetItemData(item.itemId);

        if (config == null || config.type != ItemType.Weapon)
        {
            Debug.LogWarning($"物品实例 {instanceId} 不是武器。");
            return null;
        }

        return item;
    }

    private static void MigrateAndNormalizeItems()
    {
        foreach (InventorySlotSaveData item in inventoryData.items)
        {
            PackageItem config = ItemDataManager.Instance == null
                ? null
                : ItemDataManager.Instance.GetItemData(item.itemId);

            if (config == null || config.IsStackable)
            {
                continue;
            }

            // 兼容旧存档：旧 JSON 没有这些字段时自动补齐。
            if (string.IsNullOrEmpty(item.instanceId))
            {
                item.instanceId = Guid.NewGuid().ToString("N");
            }

            item.count = 1;
            item.level = Mathf.Max(1, item.level);
            item.currentExperience = Mathf.Max(0, item.currentExperience);
            item.enhanceLevel = Mathf.Max(0, item.enhanceLevel);
        }
    }
}
