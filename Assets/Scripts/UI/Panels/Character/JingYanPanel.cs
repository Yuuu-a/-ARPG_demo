using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JingYanPanel : BasePanel
{
    [Header("经验面板按钮")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button addExperienceButton;

    [Header("测试经验")]
    [Min(1)]
    [SerializeField] private int experiencePerClick = 10;

    [Header("成长数据显示")]
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text projectedLevelText;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Image experienceFillImage;

    [Header("当前属性")]
    [SerializeField] private TMP_Text currentHealthText;
    [SerializeField] private TMP_Text currentAttackText;
    [SerializeField] private TMP_Text currentDefenseText;

    [Header("预期属性增量")]
    [SerializeField] private TMP_Text projectedHealthIncreaseText;
    [SerializeField] private TMP_Text projectedAttackIncreaseText;
    [SerializeField] private TMP_Text projectedDefenseIncreaseText;

    [Header("数据源")]
    [SerializeField] private PlayerProgression playerProgression;
    [SerializeField] private PlayerStats playerStats;

    public override void OpenPanel(string panelName)
    {
        name = panelName;
        gameObject.SetActive(true);
        ResolvePlayerProgression();
        ResolvePlayerStats();
        SubscribeProgression();
        SubscribeStats();
        RefreshProgress();
    }

    public override void ClosePanel()
    {
        isRemove = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        BindButtons();
        ResolvePlayerProgression();
        ResolvePlayerStats();
        SubscribeProgression();
        SubscribeStats();
        RefreshProgress();
    }

    private void OnDisable()
    {
        UnbindButtons();
        UnsubscribeProgression();
        UnsubscribeStats();
    }

    private void BindButtons()
    {
        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(ReturnToAbilityPanel);
            returnButton.onClick.AddListener(ReturnToAbilityPanel);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (addExperienceButton != null)
        {
            addExperienceButton.onClick.RemoveListener(OnAddExperienceClicked);
            addExperienceButton.onClick.AddListener(OnAddExperienceClicked);
        }
    }

    private void UnbindButtons()
    {
        returnButton?.onClick.RemoveListener(ReturnToAbilityPanel);
        upgradeButton?.onClick.RemoveListener(OnUpgradeClicked);
        addExperienceButton?.onClick.RemoveListener(OnAddExperienceClicked);
    }

    private void ReturnToAbilityPanel()
    {
        UIManager.Instance.ClosePanel(UIConst.JingYanPanel);
    }

    private void OnUpgradeClicked()
    {
        playerProgression?.TryUpgrade();
    }

    private void OnAddExperienceClicked()
    {
        playerProgression?.AddExperience(experiencePerClick);
    }

    private void ResolvePlayerProgression()
    {
        if (playerProgression == null)
        {
            playerProgression = FindObjectOfType<PlayerProgression>();
        }
    }

    private void ResolvePlayerStats()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
    }

    private void SubscribeProgression()
    {
        if (playerProgression == null)
        {
            Debug.LogWarning(
                $"{nameof(JingYanPanel)} 没有找到 PlayerProgression。",
                this);
            return;
        }

        playerProgression.ExperienceChanged -= HandleExperienceChanged;
        playerProgression.ExperienceChanged += HandleExperienceChanged;
        playerProgression.LevelChanged -= HandleLevelChanged;
        playerProgression.LevelChanged += HandleLevelChanged;
    }

    private void UnsubscribeProgression()
    {
        if (playerProgression == null)
        {
            return;
        }

        playerProgression.ExperienceChanged -= HandleExperienceChanged;
        playerProgression.LevelChanged -= HandleLevelChanged;
    }

    private void SubscribeStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning(
                $"{nameof(JingYanPanel)} 没有找到 PlayerStats。",
                this);
            return;
        }

        playerStats.StatsChanged -= RefreshProgress;
        playerStats.StatsChanged += RefreshProgress;
    }

    private void UnsubscribeStats()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged -= RefreshProgress;
        }
    }

    private void HandleExperienceChanged(
        int currentExperience,
        int requiredExperience)
    {
        RefreshProgress();
    }

    private void HandleLevelChanged(int level)
    {
        RefreshProgress();
    }

    private void RefreshProgress()
    {
        if (playerProgression == null)
        {
            return;
        }

        SetText(currentLevelText, $"等级{playerProgression.Level}");
        SetText(
            projectedLevelText,
            $"等级{playerProgression.GetProjectedLevel()}");
        SetText(
            experienceText,
            playerProgression.IsMaxLevel
                ? "MAX"
                : $"{playerProgression.CurrentExperience}/" +
                  playerProgression.RequiredExperience);

        RefreshStatsPreview();

        if (experienceFillImage != null)
        {
            experienceFillImage.fillAmount = playerProgression.IsMaxLevel
                ? 1f
                : Mathf.Clamp01(
                    (float)playerProgression.CurrentExperience /
                    playerProgression.RequiredExperience);
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = playerProgression.CanUpgrade;
        }

        if (addExperienceButton != null)
        {
            addExperienceButton.interactable = !playerProgression.IsMaxLevel;
        }
    }

    private void RefreshStatsPreview()
    {
        if (playerStats == null || playerProgression == null)
        {
            return;
        }

        SetText(currentHealthText, playerStats.MaxHealth.ToString());
        SetText(currentAttackText, playerStats.Attack.ToString());
        SetText(currentDefenseText, playerStats.Defense.ToString());

        int projectedLevel = playerProgression.GetProjectedLevel();
        bool hasLevelIncrease = projectedLevel > playerProgression.Level;

        SetVisible(projectedHealthIncreaseText, hasLevelIncrease);
        SetVisible(projectedAttackIncreaseText, hasLevelIncrease);
        SetVisible(projectedDefenseIncreaseText, hasLevelIncrease);

        if (!hasLevelIncrease)
        {
            return;
        }

        CharacterStatsPreview projectedStats =
            playerStats.GetStatsPreview(projectedLevel);

        SetText(
            projectedHealthIncreaseText,
            FormatIncrease(projectedStats.MaxHealth - playerStats.MaxHealth));
        SetText(
            projectedAttackIncreaseText,
            FormatIncrease(projectedStats.Attack - playerStats.Attack));
        SetText(
            projectedDefenseIncreaseText,
            FormatIncrease(projectedStats.Defense - playerStats.Defense));
    }

    private static string FormatIncrease(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private static void SetVisible(TMP_Text target, bool visible)
    {
        if (target != null)
        {
            target.gameObject.SetActive(visible);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private void OnValidate()
    {
        experiencePerClick = Mathf.Max(1, experiencePerClick);
    }
}
