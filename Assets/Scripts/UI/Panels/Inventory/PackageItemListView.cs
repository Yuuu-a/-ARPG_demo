using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PackageItemListView
{
    private readonly Transform contentRoot;
    private readonly GameObject itemPrefab;
    private readonly List<PackageSlotUI> slots =
        new List<PackageSlotUI>();

    public PackageItemListView(
        Transform contentRoot,
        GameObject itemPrefab)
    {
        this.contentRoot = contentRoot;
        this.itemPrefab = itemPrefab;
    }

    public bool IsValid => contentRoot != null && itemPrefab != null;

    public void Refresh(
        IReadOnlyList<PackageItemViewData> items,
        Action<string> selectedCallback)
    {
        Clear();

        if (!IsValid || items == null)
        {
            return;
        }

        foreach (PackageItemViewData item in items)
        {
            GameObject itemObject =
                UnityEngine.Object.Instantiate(
                    itemPrefab,
                    contentRoot,
                    false);

            PackageSlotUI slot =
                itemObject.GetComponent<PackageSlotUI>();

            if (slot == null)
            {
                Debug.LogError(
                    "PackageItem Prefab 没有挂载 PackageSlotUI。",
                    itemPrefab);
                UnityEngine.Object.Destroy(itemObject);
                continue;
            }

            slot.Bind(item, selectedCallback);
            slots.Add(slot);
        }
    }

    public void SetSelected(string selectionKey)
    {
        foreach (PackageSlotUI slot in slots)
        {
            slot.SetSelected(
                slot.SelectionKey == selectionKey);
        }
    }

    public void Clear()
    {
        foreach (PackageSlotUI slot in slots)
        {
            if (slot != null)
            {
                UnityEngine.Object.Destroy(slot.gameObject);
            }
        }

        slots.Clear();
    }
}
