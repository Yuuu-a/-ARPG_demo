using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackageSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("格子显示")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image typeIcon;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject characterIcon;

    [Header("选中状态")]
    [SerializeField] private GameObject selectedState;
    [SerializeField] private GameObject unselectedState;

    private Action<string> onSelected;

    public string SelectionKey { get; private set; }

    public void Bind(
        PackageItemViewData item,
        Action<string> selectedCallback)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        SelectionKey = item.SelectionKey;
        onSelected = selectedCallback;

        if (itemIcon != null)
        {
            itemIcon.sprite = item.ItemIcon;
            itemIcon.enabled = itemIcon.sprite != null;
        }

        if (typeIcon != null)
        {
            typeIcon.sprite = item.TypeIcon;
            typeIcon.enabled = typeIcon.sprite != null;
        }

        if (countText != null)
        {
            countText.text = item.Count > 1
                ? item.Count.ToString()
                : string.Empty;
        }

        if (levelText != null)
        {
            levelText.text = $"等级{item.Level}";
        }

        SetCharacterIconVisible(item.IsEquipped);
        SetSelected(false);
    }

    public void Select()
    {
        if (!string.IsNullOrEmpty(SelectionKey))
        {
            onSelected?.Invoke(SelectionKey);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Select();
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedState != null)
        {
            selectedState.SetActive(isSelected);
        }

        if (unselectedState != null)
        {
            unselectedState.SetActive(!isSelected);
        }
    }

    private void Clear()
    {
        SelectionKey = null;
        onSelected = null;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (typeIcon != null)
        {
            typeIcon.sprite = null;
            typeIcon.enabled = false;
        }

        if (countText != null)
        {
            countText.text = string.Empty;
        }

        if (levelText != null)
        {
            levelText.text = string.Empty;
        }

        SetCharacterIconVisible(false);
        SetSelected(false);
    }

    private void SetCharacterIconVisible(bool visible)
    {
        if (characterIcon == null)
        {
            return;
        }

        characterIcon.SetActive(visible);
    }
}
