using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }

    [Header("静态物品数据")]
    [FormerlySerializedAs("itemDataConfig")]
    [SerializeField] private WeaponDataConfig weaponDataConfig;
    [SerializeField] private EquipmentDataConfig equipmentDataConfig;

    private readonly Dictionary<int, PackageItem> itemDataDict =
        new Dictionary<int, PackageItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在重复的 ItemDataManager。", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadItemData();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LoadItemData()
    {
        if (weaponDataConfig == null)
        {
            weaponDataConfig =
                Resources.Load<WeaponDataConfig>(
                    "Config/WeaponDataConfig");
        }

        if (equipmentDataConfig == null)
        {
            equipmentDataConfig =
                Resources.Load<EquipmentDataConfig>(
                    "Config/EquipmentDataConfig");
        }

        itemDataDict.Clear();

        if (weaponDataConfig == null && equipmentDataConfig == null)
        {
            Debug.LogError(
                "未配置 WeaponDataConfig 或 EquipmentDataConfig。",
                this);
            return;
        }

        AddItems(
            weaponDataConfig == null
                ? null
                : weaponDataConfig.ItemDataList,
            weaponDataConfig);
        AddItems(
            equipmentDataConfig == null
                ? null
                : equipmentDataConfig.ItemDataList,
            equipmentDataConfig);

        Debug.Log($"物品 SO 加载完成，数量：{itemDataDict.Count}");
    }

    private void AddItems<TItem>(
        IReadOnlyList<TItem> items,
        Object sourceConfig)
        where TItem : PackageItem
    {
        if (items == null)
        {
            return;
        }

        foreach (PackageItem item in items)
        {
            if (item == null || item.id <= 0)
            {
                Debug.LogWarning("忽略了 ID 无效的物品配置。", sourceConfig);
                continue;
            }

            if (itemDataDict.ContainsKey(item.id))
            {
                Debug.LogWarning($"物品 ID 重复：{item.id}", sourceConfig);
                continue;
            }

            itemDataDict.Add(item.id, item);
        }
    }

    public bool ContainsItem(int id)
    {
        return itemDataDict.ContainsKey(id);
    }

    public PackageItem GetItemData(int id)
    {
        if (itemDataDict.TryGetValue(id, out PackageItem item))
        {
            return item;
        }

        Debug.LogWarning($"没有找到物品 ID：{id}");
        return null;
    }

    public List<PackageItem> GetAllItemData()
    {
        return new List<PackageItem>(itemDataDict.Values);
    }

    public Sprite GetItemIcon(int id)
    {
        PackageItem item = GetItemData(id);
        return item == null ? null : item.ItemIcon;
    }

    public string GetItemEffectDescription(PackageItem item)
    {
        return item == null ? string.Empty : item.effectDescription;
    }

    public string GetBaseAttackDescription(PackageItem item)
    {
        return item is WeaponPackageItem weaponItem &&
               weaponItem.baseAttack > 0
            ? $"基础攻击力：{weaponItem.baseAttack}"
            : string.Empty;
    }

    public string GetBasicAttributeDescription(PackageItem item)
    {
        if (item is WeaponPackageItem weaponItem)
        {
            return weaponItem.GetWeaponEffectDescription();
        }

        if (item is EquipmentPackageItem equipmentItem)
        {
            return equipmentItem.GetAttributeDescription();
        }

        return string.Empty;
    }

}
