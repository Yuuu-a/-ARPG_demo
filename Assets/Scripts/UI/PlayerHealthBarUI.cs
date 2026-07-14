using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色血条：当前血量立即更新，缓冲血条平滑追赶当前血量。
/// 挂在血条根物体上，并在 Inspector 依次拖入第二层和第三层 Image。
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("血条层级")]
    [SerializeField] private Image bufferedHealthImage;
    [SerializeField] private Image currentHealthImage;

    [Header("可选：血量文字")]
    [SerializeField] private TMP_Text healthText;

    [Header("缓冲速度")]
    [SerializeField, Min(0.01f)] private float bufferLerpSpeed = 3f;

    [Header("角色数据")]
    [SerializeField] private PlayerStats playerStats;

    private float targetHealthFill = 1f;
    private bool hasInitialized;

    private void OnEnable()
    {
        ResolvePlayerStats();
        SubscribePlayerStats();
        RefreshImmediately();
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.StatsChanged -= HandleStatsChanged;
        }
    }

    private void Update()
    {
        if (!hasInitialized || bufferedHealthImage == null)
        {
            return;
        }

        bufferedHealthImage.fillAmount = Mathf.Lerp(
            bufferedHealthImage.fillAmount,
            targetHealthFill,
            bufferLerpSpeed * Time.deltaTime);

        if (Mathf.Abs(bufferedHealthImage.fillAmount - targetHealthFill) < 0.001f)
        {
            bufferedHealthImage.fillAmount = targetHealthFill;
        }
    }

    private void ResolvePlayerStats()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
    }

    private void SubscribePlayerStats()
    {
        if (playerStats == null)
        {
            Debug.LogWarning(
                $"{nameof(PlayerHealthBarUI)} 没有找到 PlayerStats。",
                this);
            return;
        }

        playerStats.StatsChanged -= HandleStatsChanged;
        playerStats.StatsChanged += HandleStatsChanged;
    }

    private void HandleStatsChanged()
    {
        RefreshHealth(false);
    }

    private void RefreshImmediately()
    {
        RefreshHealth(true);
    }

    private void RefreshHealth(bool setBufferImmediately)
    {
        if (playerStats == null)
        {
            return;
        }

        targetHealthFill = Mathf.Clamp01(
            (float)playerStats.CurrentHealth / playerStats.MaxHealth);

        if (currentHealthImage != null)
        {
            currentHealthImage.fillAmount = targetHealthFill;
        }

        if (bufferedHealthImage != null &&
            (setBufferImmediately || !hasInitialized))
        {
            bufferedHealthImage.fillAmount = targetHealthFill;
        }

        if (healthText != null)
        {
            healthText.text =
                $"{playerStats.CurrentHealth}/{playerStats.MaxHealth}";
        }

        hasInitialized = true;
    }
}
