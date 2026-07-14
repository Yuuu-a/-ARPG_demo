using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPackagePanel : BasePanel
{
    [Header("Equipment List")]
    [SerializeField] private Transform listContentRoot;
    [SerializeField] private GameObject itemPrefab;

    [Header("Equipment Details")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemLevelText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text effectDescriptionText;
    [SerializeField] private TMP_Text baseAttackText;
    [SerializeField] private TMP_Text basicAttributeText;

    [Header("Panel Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;

    private readonly List<PackageItemViewData> visibleItems =
        new List<PackageItemViewData>();

    private PackageItemListView itemListView;
    private PackageItemDetailView itemDetailView;
    private string selectedItemKey;
    private ItemType currentItemType = ItemType.Equipment;

    public int TargetSlotIndex { get; private set; } = -1;

    private void OnEnable()
    {
        _ = InventoryManager.Items.Count;
        InventoryManager.InventoryChanged += Refresh;
        EquipmentLoadoutManager.LoadoutChanged += Refresh;

        closeButton?.onClick.RemoveListener(CloseEquipmentPackage);
        closeButton?.onClick.AddListener(CloseEquipmentPackage);
        equipButton?.onClick.RemoveListener(EquipSelectedItem);
        equipButton?.onClick.AddListener(EquipSelectedItem);
        unequipButton?.onClick.RemoveListener(UnequipCurrentSlot);
        unequipButton?.onClick.AddListener(UnequipCurrentSlot);

        Refresh();
    }

    private void OnDisable()
    {
        InventoryManager.InventoryChanged -= Refresh;
        EquipmentLoadoutManager.LoadoutChanged -= Refresh;
        closeButton?.onClick.RemoveListener(CloseEquipmentPackage);
        equipButton?.onClick.RemoveListener(EquipSelectedItem);
        unequipButton?.onClick.RemoveListener(UnequipCurrentSlot);
    }

    public void Configure(ItemType itemType, int slotIndex)
    {
        if (itemType != ItemType.Equipment && itemType != ItemType.Weapon)
        {
            Debug.LogWarning(
                $"EquipmentPackagePanel does not support {itemType}.",
                this);
            return;
        }

        currentItemType = itemType;
        TargetSlotIndex = slotIndex;

        if (isActiveAndEnabled)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        EnsureViews();

        if (itemListView == null || !itemListView.IsValid)
        {
            Debug.LogError(
                "EquipmentPackagePanel list references are not configured.",
                this);
            return;
        }

        List<InventorySlotSaveData> filteredItems =
            GetFilteredItems();

        filteredItems.Sort((left, right) =>
        {
            int idComparison = left.itemId.CompareTo(right.itemId);

            return idComparison != 0
                ? idComparison
                : left.slotIndex.CompareTo(right.slotIndex);
        });

        visibleItems.Clear();

        foreach (InventorySlotSaveData slotData in filteredItems)
        {
            PackageItemViewData viewData = CreateViewData(slotData);

            if (viewData != null)
            {
                visibleItems.Add(viewData);
            }
        }

        itemListView.Refresh(visibleItems, SelectItem);

        PackageItemViewData selectedItem =
            FindVisibleItem(selectedItemKey);

        if (selectedItem == null && visibleItems.Count > 0)
        {
            selectedItem = visibleItems[0];
        }

        if (selectedItem != null)
        {
            SelectItem(selectedItem.SelectionKey);
            return;
        }

        selectedItemKey = null;
        itemListView.SetSelected(null);
        itemDetailView?.ShowEmpty(
            currentItemType == ItemType.Weapon
                ? "背包中暂无武器"
                : "背包中暂无装备");
        RefreshActionButtons();
    }

    private List<InventorySlotSaveData> GetFilteredItems()
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

            if (itemData != null && itemData.type == currentItemType)
            {
                filteredItems.Add(slotData);
            }
        }

        return filteredItems;
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

        if (itemData == null || itemData.type != currentItemType)
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

        selectedItemKey = selectedItem.SelectionKey;
        itemListView.SetSelected(selectedItem.SelectionKey);
        itemDetailView?.Show(selectedItem);
        RefreshActionButtons();
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
                null,
                null,
                itemNameText,
                itemLevelText,
                descriptionText,
                effectDescriptionText,
                baseAttackText,
                basicAttributeText);
        }
    }

    private void EquipSelectedItem()
    {
        InventorySlotSaveData selectedItem = GetSelectedInventoryItem();

        if (selectedItem == null)
        {
            return;
        }

        EquipmentLoadoutManager.Equip(TargetSlotIndex, selectedItem);
        RefreshActionButtons();
    }

    private void UnequipCurrentSlot()
    {
        EquipmentLoadoutManager.Unequip(TargetSlotIndex);
        RefreshActionButtons();
    }

    private InventorySlotSaveData GetSelectedInventoryItem()
    {
        if (string.IsNullOrEmpty(selectedItemKey))
        {
            return null;
        }

        foreach (InventorySlotSaveData item in InventoryManager.Items)
        {
            if (item == null)
            {
                continue;
            }

            string selectionKey = string.IsNullOrEmpty(item.instanceId)
                ? $"slot:{item.slotIndex}"
                : item.instanceId;

            if (selectionKey == selectedItemKey)
            {
                return item;
            }
        }

        return null;
    }

    private void RefreshActionButtons()
    {
        if (equipButton != null)
        {
            equipButton.interactable = GetSelectedInventoryItem() != null;
        }

        if (unequipButton != null)
        {
            unequipButton.interactable =
                EquipmentLoadoutManager.GetEquippedItem(TargetSlotIndex) != null;
        }
    }

    private void CloseEquipmentPackage()
    {
        UIManager.Instance.ClosePanel(UIConst.EquipmentPackagePanel);
    }
}
