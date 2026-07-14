using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class GMCmd
{
    private const int InitialWeaponCount = 10;
    private const int InitialEquipmentCount = 10;

    [MenuItem("GMcmd/打开背包")]
    public static void OpenPackage()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("请先进入 Play Mode，再执行 GMcmd/打开背包");
            return;
        }

        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    [MenuItem("GMcmd/背包/添加10个随机武器")]
    public static void AddRandomWeapons()
    {
        AddRandomItems(
            ItemType.Weapon,
            InitialWeaponCount,
            "武器");
    }

    [MenuItem("GMcmd/背包/添加10个随机装备")]
    public static void AddRandomEquipment()
    {
        AddRandomItems(
            ItemType.Equipment,
            InitialEquipmentCount,
            "装备");
    }

    [MenuItem("GMcmd/背包/清空背包")]
    public static void ClearPackage()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("请先进入 Play Mode，再清空背包");
            return;
        }

        InventoryManager.Clear();

        if (InventoryManager.Save())
        {
            Debug.Log("背包已经清空");
        }
    }

    [MenuItem("GMcmd/读取物品配置")]
    public static void ReadValue()
    {
        if (!EditorApplication.isPlaying || ItemDataManager.Instance == null)
        {
            Debug.LogError("请在 Play Mode 中读取物品配置。");
            return;
        }

        List<PackageItem> items = ItemDataManager.Instance.GetAllItemData();
        foreach (PackageItem item in items)
        {
            Debug.Log(
                $"物品ID：{item.id}，" +
                $"名称：{item.name}，" +
                $"类型：{item.type}，" +
                $"描述：{item.description}，" +
                $"效果：{item.effectDescription}，" +
                $"图标：{item.ItemIcon}"
            );
        }

        Debug.Log($"物品遍历完成，共 {items.Count} 件");
    }

    private static void AddRandomItems(
        ItemType itemType,
        int count,
        string displayName)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning(
                $"请先进入 Play Mode，再添加随机{displayName}");
            return;
        }

        if (ItemDataManager.Instance == null)
        {
            Debug.LogError("场景中没有可用的 ItemDataManager");
            return;
        }

        List<PackageItem> configs =
            ItemDataManager.Instance.GetAllItemData()
                .FindAll(item => item.type == itemType);

        if (configs.Count == 0)
        {
            Debug.LogError(
                $"静态配置中没有可用的{displayName}配置");
            return;
        }

        int addedCount = 0;

        for (int index = 0; index < count; index++)
        {
            int randomIndex = Random.Range(0, configs.Count);
            PackageItem item = configs[randomIndex];

            if (InventoryManager.AddItem(item.id))
            {
                addedCount++;
            }
        }

        if (InventoryManager.Save())
        {
            Debug.Log(
                $"背包添加完成：已添加 {addedCount} 个随机{displayName}");
        }
    }
}
#endif
