using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PackageUI : BasePanel
{
    private enum PackageCategory
    {
        Weapon,
        Equipment
    }

    [Header("背包列表")]
    [FormerlySerializedAs("contentRoot")]
    [SerializeField] private Transform listContentRoot;
    [FormerlySerializedAs("packageItemPrefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("物品详情")]
    [SerializeField] private Image detailIcon;
    [FormerlySerializedAs("detailWeaponImage")]
    [SerializeField] private Image detailPreviewImage;
    [FormerlySerializedAs("weaponNameText")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemLevelText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text effectDescriptionText;
    [SerializeField] private TMP_Text baseAttackText;
    [SerializeField] private TMP_Text basicAttributeText;

    [Header("交互")]
    [SerializeField] private Button closeButton;

    [Header("分类页签")]
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color unselectedTabColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);

    private readonly List<PackageItemViewData> visibleItems =
        new List<PackageItemViewData>();

    private PackageItemListView itemListView;
    private PackageItemDetailView itemDetailView;
    private PackageCategory currentCategory = PackageCategory.Weapon;
    private string selectedWeaponKey;
    private string selectedEquipmentKey;

    private void OnEnable()
    {
        _ = InventoryManager.Items.Count;

        InventoryManager.InventoryChanged += Refresh;
        EquipmentLoadoutManager.LoadoutChanged += Refresh;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePackage);
            closeButton.onClick.AddListener(ClosePackage);
        }

        BindCategoryButton(
            weaponButton,
            ShowWeaponCategory);
        BindCategoryButton(
            equipmentButton,
            ShowEquipmentCategory);

        Refresh();
    }

    private void OnDisable()
    {
        InventoryManager.InventoryChanged -= Refresh;
        EquipmentLoadoutManager.LoadoutChanged -= Refresh;
        closeButton?.onClick.RemoveListener(ClosePackage);
        weaponButton?.onClick.RemoveListener(ShowWeaponCategory);
        equipmentButton?.onClick.RemoveListener(ShowEquipmentCategory);
    }

    private void Refresh()
    {
        EnsureViews();

        if (itemListView == null || !itemListView.IsValid)
        {
            Debug.LogError("PackageUI 的列表引用未完整配置。", this);
            return;
        }

        List<InventorySlotSaveData> sortedItems =
            FilterCurrentCategory();

        sortedItems.Sort((left, right) =>
        {
            int idComparison = left.itemId.CompareTo(right.itemId);

            return idComparison != 0
                ? idComparison
                : left.slotIndex.CompareTo(right.slotIndex);
        });

        visibleItems.Clear();

        foreach (InventorySlotSaveData slotData in sortedItems)
        {
            PackageItemViewData viewData = CreateViewData(slotData);

            if (viewData != null)
            {
                visibleItems.Add(viewData);
            }
        }

        itemListView.Refresh(visibleItems, SelectItem);

        PackageItemViewData selectedItem =
            FindVisibleItem(GetCurrentSelectedKey());

        if (selectedItem == null && visibleItems.Count > 0)
        {
            selectedItem = visibleItems[0];
        }

        if (selectedItem != null)
        {
            SelectItem(selectedItem.SelectionKey);
            return;
        }

        SetCurrentSelectedKey(null);
        itemListView.SetSelected(null);
        itemDetailView?.ShowEmpty("当前分类暂无物品");
        UpdateTabVisuals();
    }

    private List<InventorySlotSaveData> FilterCurrentCategory()
    {
        List<InventorySlotSaveData> filteredItems =
            new List<InventorySlotSaveData>();

        if (ItemDataManager.Instance == null)
        {
            return filteredItems;
        }

        foreach (InventorySlotSaveData slotData in InventoryManager.Items)
        {
            if (slotData == null)
            {
                continue;
            }

            PackageItem itemData =
                ItemDataManager.Instance.GetItemData(slotData.itemId);

            if (itemData != null && IsCurrentCategory(itemData.type))
            {
                filteredItems.Add(slotData);
            }
        }

        return filteredItems;
    }

    private bool IsCurrentCategory(ItemType itemType)
    {
        return currentCategory == PackageCategory.Weapon
            ? itemType == ItemType.Weapon
            : itemType == ItemType.Equipment;
    }

    private PackageItemViewData CreateViewData(
        InventorySlotSaveData slotData)
    {
        if (slotData == null || ItemDataManager.Instance == null)
        {
            return null;
        }

        PackageItem itemData =
            ItemDataManager.Instance.GetItemData(slotData.itemId);

        if (itemData == null)
        {
            return null;
        }

        string selectionKey = string.IsNullOrEmpty(slotData.instanceId)
            ? $"slot:{slotData.slotIndex}"
            : slotData.instanceId;
        EquippedItemRecord equippedItem =
            EquipmentLoadoutManager.GetEquippedItem(selectionKey);

        return new PackageItemViewData(
            selectionKey,
            ItemDataManager.Instance.GetItemIcon(itemData.id),
            itemData.TypeIcon,
            slotData.count,
            Mathf.Max(1, slotData.level),
            itemData.name,
            itemData.description,
            ItemDataManager.Instance.GetItemEffectDescription(itemData),
            equippedItem != null,
            ItemDataManager.Instance.GetBaseAttackDescription(itemData),
            ItemDataManager.Instance.GetBasicAttributeDescription(itemData));
    }

    private void SelectItem(string selectionKey)
    {
        PackageItemViewData selectedItem =
            FindVisibleItem(selectionKey);

        if (selectedItem == null)
        {
            return;
        }

        SetCurrentSelectedKey(selectedItem.SelectionKey);
        itemListView.SetSelected(selectedItem.SelectionKey);
        itemDetailView?.Show(selectedItem);
        UpdateTabVisuals();
    }

    private PackageItemViewData FindVisibleItem(string selectionKey)
    {
        if (string.IsNullOrEmpty(selectionKey))
        {
            return null;
        }

        foreach (PackageItemViewData item in visibleItems)
        {
            if (item.SelectionKey == selectionKey)
            {
                return item;
            }
        }

        return null;
    }

    private void EnsureViews()
    {
        if (itemListView == null)
        {
            itemListView = new PackageItemListView(
                listContentRoot,
                itemPrefab);
        }

        if (itemDetailView == null)
        {
            itemDetailView = new PackageItemDetailView(
                detailIcon,
                detailPreviewImage,
                itemNameText,
                itemLevelText,
                descriptionText,
                effectDescriptionText,
                baseAttackText,
                basicAttributeText);
        }
    }

    private void ShowWeaponCategory()
    {
        SwitchCategory(PackageCategory.Weapon);
    }

    private void ShowEquipmentCategory()
    {
        SwitchCategory(PackageCategory.Equipment);
    }

    private void SwitchCategory(PackageCategory category)
    {
        if (currentCategory == category)
        {
            UpdateTabVisuals();
            return;
        }

        currentCategory = category;
        Refresh();
    }

    private string GetCurrentSelectedKey()
    {
        return currentCategory == PackageCategory.Weapon
            ? selectedWeaponKey
            : selectedEquipmentKey;
    }

    private void SetCurrentSelectedKey(string selectionKey)
    {
        if (currentCategory == PackageCategory.Weapon)
        {
            selectedWeaponKey = selectionKey;
        }
        else
        {
            selectedEquipmentKey = selectionKey;
        }
    }

    private void UpdateTabVisuals()
    {
        SetTabColor(
            weaponButton,
            currentCategory == PackageCategory.Weapon);
        SetTabColor(
            equipmentButton,
            currentCategory == PackageCategory.Equipment);
    }

    private void SetTabColor(Button button, bool isSelected)
    {
        if (button != null && button.targetGraphic != null)
        {
            button.targetGraphic.color = isSelected
                ? selectedTabColor
                : unselectedTabColor;
        }
    }

    private static void BindCategoryButton(
        Button button,
        UnityEngine.Events.UnityAction handler)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
        button.onClick.AddListener(handler);
    }

    private void ClosePackage()
    {
        UIManager.Instance.ClosePanel(UIConst.PackagePanel);
    }

}
