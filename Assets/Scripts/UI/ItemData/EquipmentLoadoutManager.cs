using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EquippedItemRecord
{
    public EquippedItemRecord(
        int itemId,
        string selectionKey,
        ItemType itemType)
    {
        ItemId = itemId;
        SelectionKey = selectionKey;
        ItemType = itemType;
    }

    public int ItemId { get; }
    public string SelectionKey { get; }
    public ItemType ItemType { get; }
}

public static class EquipmentLoadoutManager
{
    public const int WeaponSlotIndex = -1;

    private static readonly Dictionary<int, EquippedItemRecord> equippedItems =
        new Dictionary<int, EquippedItemRecord>();

    // The character can be created dynamically, so do not rely only on a
    // scene-wide search when an equip button is pressed.
    private static PlayerStats registeredPlayerStats;

    public static event Action LoadoutChanged;

    public static void RegisterPlayerStats(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            return;
        }

        registeredPlayerStats = playerStats;
        RefreshPlayerStatModifiers();
    }

    public static void UnregisterPlayerStats(PlayerStats playerStats)
    {
        if (registeredPlayerStats == playerStats)
        {
            registeredPlayerStats = null;
        }
    }

    public static bool Equip(
        int targetSlotIndex,
        InventorySlotSaveData inventoryItem)
    {
        if (inventoryItem == null || ItemDataManager.Instance == null)
        {
            return false;
        }

        PackageItem itemData =
            ItemDataManager.Instance.GetItemData(inventoryItem.itemId);

        if (itemData == null || !CanEquip(targetSlotIndex, itemData.type))
        {
            return false;
        }

        string selectionKey = string.IsNullOrEmpty(inventoryItem.instanceId)
            ? $"slot:{inventoryItem.slotIndex}"
            : inventoryItem.instanceId;

        int previousSlotIndex = int.MinValue;

        foreach (KeyValuePair<int, EquippedItemRecord> pair in equippedItems)
        {
            if (pair.Value.SelectionKey == selectionKey)
            {
                previousSlotIndex = pair.Key;
                break;
            }
        }

        if (previousSlotIndex != int.MinValue &&
            previousSlotIndex != targetSlotIndex)
        {
            equippedItems.Remove(previousSlotIndex);
        }

        equippedItems[targetSlotIndex] = new EquippedItemRecord(
            inventoryItem.itemId,
            selectionKey,
            itemData.type);

        RefreshPlayerStatModifiers();
        LoadoutChanged?.Invoke();
        return true;
    }

    public static bool Unequip(int targetSlotIndex)
    {
        if (!equippedItems.Remove(targetSlotIndex))
        {
            return false;
        }

        RefreshPlayerStatModifiers();
        LoadoutChanged?.Invoke();
        return true;
    }

    public static EquippedItemRecord GetEquippedItem(int targetSlotIndex)
    {
        equippedItems.TryGetValue(
            targetSlotIndex,
            out EquippedItemRecord equippedItem);

        return equippedItem;
    }

    public static EquippedItemRecord GetEquippedItem(string selectionKey)
    {
        if (string.IsNullOrEmpty(selectionKey))
        {
            return null;
        }

        foreach (EquippedItemRecord equippedItem in equippedItems.Values)
        {
            if (equippedItem.SelectionKey == selectionKey)
            {
                return equippedItem;
            }
        }

        return null;
    }

    public static List<EquippedItemSaveData> CreateSaveData()
    {
        List<EquippedItemSaveData> result = new List<EquippedItemSaveData>();

        foreach (KeyValuePair<int, EquippedItemRecord> pair in equippedItems)
        {
            result.Add(new EquippedItemSaveData
            {
                slotId = GetSaveSlotId(pair.Key),
                itemInstanceId = pair.Value.SelectionKey
            });
        }

        return result;
    }

    public static void RestoreFromSaveData(
        IEnumerable<EquippedItemSaveData> saveData)
    {
        equippedItems.Clear();

        if (saveData != null && ItemDataManager.Instance != null)
        {
            foreach (EquippedItemSaveData savedItem in saveData)
            {
                if (savedItem == null ||
                    !TryGetSlotIndex(savedItem.slotId, out int slotIndex))
                {
                    continue;
                }

                InventorySlotSaveData inventoryItem =
                    FindInventoryItem(savedItem.itemInstanceId);
                PackageItem itemData = inventoryItem == null
                    ? null
                    : ItemDataManager.Instance.GetItemData(inventoryItem.itemId);

                if (itemData == null || !CanEquip(slotIndex, itemData.type))
                {
                    continue;
                }

                string selectionKey = string.IsNullOrEmpty(inventoryItem.instanceId)
                    ? $"slot:{inventoryItem.slotIndex}"
                    : inventoryItem.instanceId;
                equippedItems[slotIndex] = new EquippedItemRecord(
                    inventoryItem.itemId,
                    selectionKey,
                    itemData.type);
            }
        }

        RefreshPlayerStatModifiers();
        LoadoutChanged?.Invoke();
    }

    private static bool CanEquip(int targetSlotIndex, ItemType itemType)
    {
        return targetSlotIndex == WeaponSlotIndex
            ? itemType == ItemType.Weapon
            : targetSlotIndex >= 1 &&
              targetSlotIndex <= 6 &&
              itemType == ItemType.Equipment;
    }

    private static string GetSaveSlotId(int slotIndex)
    {
        return slotIndex == WeaponSlotIndex ? "weapon" : $"equipment_{slotIndex}";
    }

    private static bool TryGetSlotIndex(string slotId, out int slotIndex)
    {
        if (slotId == "weapon")
        {
            slotIndex = WeaponSlotIndex;
            return true;
        }

        const string equipmentPrefix = "equipment_";

        if (slotId != null && slotId.StartsWith(equipmentPrefix) &&
            int.TryParse(slotId.Substring(equipmentPrefix.Length), out slotIndex))
        {
            return slotIndex >= 1 && slotIndex <= 6;
        }

        slotIndex = 0;
        return false;
    }

    private static InventorySlotSaveData FindInventoryItem(string selectionKey)
    {
        if (string.IsNullOrEmpty(selectionKey))
        {
            return null;
        }

        foreach (InventorySlotSaveData inventoryItem in InventoryManager.Items)
        {
            if (inventoryItem == null)
            {
                continue;
            }

            string itemKey = string.IsNullOrEmpty(inventoryItem.instanceId)
                ? $"slot:{inventoryItem.slotIndex}"
                : inventoryItem.instanceId;

            if (itemKey == selectionKey)
            {
                return inventoryItem;
            }
        }

        return null;
    }

    public static void RefreshPlayerStatModifiers()
    {
        PlayerStats playerStats = registeredPlayerStats;

        if (playerStats == null)
        {
            playerStats = UnityEngine.Object.FindObjectOfType<PlayerStats>();
            registeredPlayerStats = playerStats;
        }

        if (playerStats == null)
        {
            return;
        }

        EquippedItemRecord weaponRecord =
            GetEquippedItem(WeaponSlotIndex);
        WeaponPackageItem weaponData = weaponRecord == null ||
            ItemDataManager.Instance == null
                ? null
                : ItemDataManager.Instance.GetItemData(
                    weaponRecord.ItemId) as WeaponPackageItem;

        int attackBonus = 0;
        int maxHealthBonus = 0;
        int defenseBonus = 0;
        float criticalRateBonus = 0f;

        if (weaponData != null)
        {
            attackBonus += weaponData.baseAttack +
                Mathf.RoundToInt(
                    playerStats.BaseAttackValue *
                    weaponData.attackBonusPercent / 100f);
            maxHealthBonus += Mathf.RoundToInt(
                playerStats.BaseMaxHealthValue *
                weaponData.maxHealthBonusPercent / 100f);
            criticalRateBonus +=
                weaponData.criticalRateBonusPercent / 100f;
        }

        for (int slotIndex = 1; slotIndex <= 6; slotIndex++)
        {
            EquippedItemRecord equipmentRecord =
                GetEquippedItem(slotIndex);
            EquipmentPackageItem equipmentData = equipmentRecord == null ||
                ItemDataManager.Instance == null
                    ? null
                    : ItemDataManager.Instance.GetItemData(
                        equipmentRecord.ItemId) as EquipmentPackageItem;

            if (equipmentData == null)
            {
                continue;
            }

            maxHealthBonus += equipmentData.maxHealth;
            attackBonus += equipmentData.attack;
            defenseBonus += equipmentData.defense;
        }

        playerStats.SetEquipmentModifiers(
            new CharacterStatModifiers(
                maxHealthBonus,
                attackBonus,
                defenseBonus,
                criticalRateBonus,
                0f,
                0));
    }
}
