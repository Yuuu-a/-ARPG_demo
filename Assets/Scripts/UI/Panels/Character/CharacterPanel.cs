using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : BasePanel
{
    [Header("角色面板按钮")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button selectedButton;
    [SerializeField] private Button equipmentButton;

    private void OnEnable()
    {
        BindButtons();
    }

    private void OnDisable()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        BindButton(returnButton, CloseCurrentPanel);
        BindButton(abilityButton, OpenAbilityPanel);
        BindButton(selectedButton, OpenAbilityPanel);
        BindButton(equipmentButton, OpenEquipmentPanel);
    }

    private void UnbindButtons()
    {
        returnButton?.onClick.RemoveListener(CloseCurrentPanel);
        abilityButton?.onClick.RemoveListener(OpenAbilityPanel);
        selectedButton?.onClick.RemoveListener(OpenAbilityPanel);
        equipmentButton?.onClick.RemoveListener(OpenEquipmentPanel);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction handler)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(handler);
        button.onClick.AddListener(handler);
    }

    private void OpenAbilityPanel()
    {
        SwitchTo(UIConst.AbilityPanel);
    }

    private void OpenEquipmentPanel()
    {
        SwitchTo(UIConst.EquipmentPanel);
    }

    private void CloseCurrentPanel()
    {
        UIManager.Instance.ClosePanel(UIConst.CharacterPanel);
    }

    private void SwitchTo(string targetPanel)
    {
        UIManager.Instance.ClosePanel(UIConst.CharacterPanel);
        UIManager.Instance.OpenPanel(targetPanel);
    }
}
