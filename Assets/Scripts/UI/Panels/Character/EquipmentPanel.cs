using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EquipmentPanel : BasePanel
{
    private enum EquipmentBagType
    {
        Equipment,
        Special
    }

    [Header("面板按钮")]
    [SerializeField] private Button returnButton;

    [Header("右下角切换按钮")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button abilityButton;

    [Header("装备槽位")]
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;
    [SerializeField] private Button button4;
    [SerializeField] private Button button5;
    [SerializeField] private Button button6;

    [Tooltip("中间的特殊槽位按钮，打开与普通装备槽不同类型的背包内容")]
    [SerializeField] private Button specialSlotButton;

    [Header("Equipped Item Icons")]
    [SerializeField] private Image equipmentIcon1;
    [SerializeField] private Image equipmentIcon2;
    [SerializeField] private Image equipmentIcon3;
    [SerializeField] private Image equipmentIcon4;
    [SerializeField] private Image equipmentIcon5;
    [SerializeField] private Image equipmentIcon6;
    [SerializeField] private Image weaponIcon;

    private UnityAction[] equipmentSlotHandlers;

    private void OnEnable()
    {
        BindButtons();
        EquipmentLoadoutManager.LoadoutChanged += RefreshEquippedIcons;
        RefreshEquippedIcons();
    }

    private void OnDisable()
    {
        EquipmentLoadoutManager.LoadoutChanged -= RefreshEquippedIcons;
        UnbindButtons();
    }

    private void BindButtons()
    {
        BindButton(returnButton, CloseCurrentPanel);
        BindButton(equipmentButton, StayOnEquipmentPanel);
        BindButton(abilityButton, OpenAbilityPanel);
        BindButton(specialSlotButton, OpenSpecialBag);

        Button[] equipmentSlotButtons = GetEquipmentSlotButtons();
        equipmentSlotHandlers = new UnityAction[equipmentSlotButtons.Length];

        for (int i = 0; i < equipmentSlotHandlers.Length; i++)
        {
            Button slotButton = equipmentSlotButtons[i];

            if (slotButton == null)
            {
                continue;
            }

            int slotIndex = i + 1;
            UnityAction handler = () => OpenEquipmentBag(slotIndex);
            equipmentSlotHandlers[i] = handler;
            slotButton.onClick.RemoveListener(handler);
            slotButton.onClick.AddListener(handler);
        }
    }

    private void UnbindButtons()
    {
        returnButton?.onClick.RemoveListener(CloseCurrentPanel);
        equipmentButton?.onClick.RemoveListener(StayOnEquipmentPanel);
        abilityButton?.onClick.RemoveListener(OpenAbilityPanel);
        specialSlotButton?.onClick.RemoveListener(OpenSpecialBag);

        if (equipmentSlotHandlers == null)
        {
            return;
        }

        Button[] equipmentSlotButtons = GetEquipmentSlotButtons();
        int count = Mathf.Min(equipmentSlotButtons.Length, equipmentSlotHandlers.Length);

        for (int i = 0; i < count; i++)
        {
            if (equipmentSlotButtons[i] != null && equipmentSlotHandlers[i] != null)
            {
                equipmentSlotButtons[i].onClick.RemoveListener(equipmentSlotHandlers[i]);
            }
        }

        equipmentSlotHandlers = null;
    }

    private Button[] GetEquipmentSlotButtons()
    {
        return new[]
        {
            button1,
            button2,
            button3,
            button4,
            button5,
            button6
        };
    }

    private Image[] GetEquipmentIcons()
    {
        return new[]
        {
            equipmentIcon1,
            equipmentIcon2,
            equipmentIcon3,
            equipmentIcon4,
            equipmentIcon5,
            equipmentIcon6
        };
    }

    private void RefreshEquippedIcons()
    {
        Image[] equipmentIcons = GetEquipmentIcons();

        for (int index = 0; index < equipmentIcons.Length; index++)
        {
            RefreshEquippedIcon(
                equipmentIcons[index],
                index + 1,
                false);
        }

        RefreshEquippedIcon(
            weaponIcon,
            EquipmentLoadoutManager.WeaponSlotIndex,
            true);
    }

    private void RefreshEquippedIcon(
        Image icon,
        int slotIndex,
        bool useItemIcon)
    {
        if (icon == null)
        {
            return;
        }

        EquippedItemRecord equippedItem =
            EquipmentLoadoutManager.GetEquippedItem(slotIndex);
        PackageItem itemData = equippedItem == null ||
            ItemDataManager.Instance == null
                ? null
                : ItemDataManager.Instance.GetItemData(equippedItem.ItemId);
        Sprite sprite = itemData == null
            ? null
            : useItemIcon
                ? itemData.ItemIcon
                : itemData.TypeIcon;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.gameObject.SetActive(sprite != null);
    }

    private static void BindButton(Button button, UnityAction handler)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
        button.onClick.AddListener(handler);
    }

    private void CloseCurrentPanel()
    {
        UIManager.Instance.ClosePanel(UIConst.EquipmentPanel);
        UIManager.Instance.OpenPanel(UIConst.CharacterPanel);
    }

    private void StayOnEquipmentPanel()
    {
        // 当前已经是装备面板，无需重复打开。
    }

    private void OpenAbilityPanel()
    {
        UIManager.Instance.ClosePanel(UIConst.EquipmentPanel);
        UIManager.Instance.OpenPanel(UIConst.AbilityPanel);
    }

    private void OpenEquipmentBag(int slotIndex)
    {
        BasePanel panel = UIManager.Instance.OpenPanel(
            UIConst.EquipmentPackagePanel);

        if (panel is EquipmentPackagePanel equipmentPackagePanel)
        {
            equipmentPackagePanel.Configure(ItemType.Equipment, slotIndex);
        }
    }

    private void OpenSpecialBag()
    {
        OpenEquipmentBag(EquipmentBagType.Special, -1);
    }

    private void OpenEquipmentBag(EquipmentBagType bagType, int slotIndex)
    {
        BasePanel panel = UIManager.Instance.OpenPanel(
            UIConst.EquipmentPackagePanel);

        if (panel is EquipmentPackagePanel equipmentPackagePanel)
        {
            ItemType itemType = bagType == EquipmentBagType.Special
                ? ItemType.Weapon
                : ItemType.Equipment;

            equipmentPackagePanel.Configure(itemType, slotIndex);
        }

        // TODO: 特有背包面板完成后，在这里传入 bagType 和 slotIndex 打开并刷新内容。
        Debug.Log($"请求打开装备背包：类型={bagType}, 槽位={slotIndex}", this);
    }
}
