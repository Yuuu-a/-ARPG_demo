using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityPanel : BasePanel
{
    [Header("面板按钮")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button jingYanButton;

    [Header("右下角切换按钮")]
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button equipmentButton;

    [Header("角色属性显示")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text impactText;
    [SerializeField] private TMP_Text criticalRateText;
    [SerializeField] private TMP_Text criticalDamageText;

    [Header("数据源")]
    [SerializeField] private PlayerStats playerStats;

    private void OnEnable()
    {
        BindButtons();
        ResolvePlayerStats();
        SubscribeStats();
        EquipmentLoadoutManager.LoadoutChanged -= RefreshStats;
        EquipmentLoadoutManager.LoadoutChanged += RefreshStats;
        EquipmentLoadoutManager.RefreshPlayerStatModifiers();
        RefreshStats();
    }

    private void OnDisable()
    {
        UnbindButtons();
        UnsubscribeStats();
        EquipmentLoadoutManager.LoadoutChanged -= RefreshStats;
    }

    public override void OpenPanel(string panelName)
    {
        base.OpenPanel(panelName);
        ResolvePlayerStats();
        SubscribeStats();
        EquipmentLoadoutManager.RefreshPlayerStatModifiers();
        RefreshStats();
    }

    private void BindButtons()
    {
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(Return);
            returnButton.onClick.AddListener(Return);
        }

        if (jingYanButton != null)
        {
            jingYanButton.onClick.RemoveListener(OpenJingYanPanel);
            jingYanButton.onClick.AddListener(OpenJingYanPanel);
        }

        if (abilityButton != null)
        {
            abilityButton.onClick.RemoveListener(StayOnAbilityPanel);
            abilityButton.onClick.AddListener(StayOnAbilityPanel);
        }

        if (equipmentButton != null)
        {
            equipmentButton.onClick.RemoveListener(OpenEquipmentPanel);
            equipmentButton.onClick.AddListener(OpenEquipmentPanel);
        }
    }

    private void UnbindButtons()
    {
        returnButton?.onClick.RemoveListener(Return);
        jingYanButton?.onClick.RemoveListener(OpenJingYanPanel);
        abilityButton?.onClick.RemoveListener(StayOnAbilityPanel);
        equipmentButton?.onClick.RemoveListener(OpenEquipmentPanel);
    }

    private void Return()
    {
        SwitchTo(UIConst.CharacterPanel);
    }

    private void OpenJingYanPanel()
    {
        if (!UIManager.Instance.IsPanelOpen(UIConst.JingYanPanel))
        {
            UIManager.Instance.OpenPanel(UIConst.JingYanPanel);
        }
    }

    private void StayOnAbilityPanel()
    {
        // 当前已经是基础属性面板，无需重复打开。
    }

    private void OpenEquipmentPanel()
    {
        SwitchTo(UIConst.EquipmentPanel);
    }

    private void SwitchTo(string targetPanel)
    {
        UIManager.Instance.ClosePanel(UIConst.AbilityPanel);
        UIManager.Instance.OpenPanel(targetPanel);
    }

    private void ResolvePlayerStats()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
    }

    private void SubscribeStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning(
                $"{nameof(AbilityPanel)} 没有找到 PlayerStats。",
                this);
            return;
        }

        playerStats.StatsChanged -= RefreshStats;
        playerStats.StatsChanged += RefreshStats;
    }

    private void UnsubscribeStats()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged -= RefreshStats;
        }
    }

    private void RefreshStats()
    {
        if (playerStats == null)
        {
            return;
        }

        SetText(characterNameText, playerStats.CharacterName);
        SetText(levelText, $"等级{playerStats.Level}");
        SetText(healthText, playerStats.MaxHealth.ToString());
        SetText(attackText, playerStats.Attack.ToString());
        SetText(defenseText, playerStats.Defense.ToString());
        SetText(impactText, playerStats.Impact.ToString());
        SetText(
            criticalRateText,
            $"{playerStats.CriticalRate * 100f:0.#}%");
        SetText(
            criticalDamageText,
            $"{playerStats.CriticalDamage * 100f:0.#}%");
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
