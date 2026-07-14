using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStats), typeof(PlayerProgression))]
public class PlayerSaveManager : MonoBehaviour
{
    private const string SaveFileName = "PlayerSave.json";

    [Header("角色运行时数据")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerProgression playerProgression;

    [Header("存档时机")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool saveOnApplicationQuit = true;

    private readonly List<EquippedItemSaveData> equippedItems =
        new List<EquippedItemSaveData>();

    public string SavePath =>
        Path.Combine(Application.persistentDataPath, SaveFileName);
    public IReadOnlyList<EquippedItemSaveData> EquippedItems =>
        equippedItems;

    private void Awake()
    {
        ResolveDependencies();
    }

    private void OnEnable()
    {
        EquipmentLoadoutManager.LoadoutChanged += SyncEquippedItems;
    }

    private void OnDisable()
    {
        EquipmentLoadoutManager.LoadoutChanged -= SyncEquippedItems;
    }

    private void Start()
    {
        if (loadOnStart)
        {
            Load();
        }
    }

    public bool Save()
    {
        if (!TryValidateDependencies())
        {
            return false;
        }

        SyncEquippedItems();

        CharacterBaseConfig config = playerStats.BaseConfig;
        if (config == null)
        {
            Debug.LogError("角色存档保存失败：PlayerStats 缺少角色配置。", this);
            return false;
        }

        PlayerSaveData saveData = new PlayerSaveData
        {
            characterId = config.CharacterId,
            level = playerProgression.Level,
            currentExperience = playerProgression.CurrentExperience,
            currentHealth = playerStats.CurrentHealth,
            equippedItems = CloneEquippedItems(equippedItems)
        };

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"角色存档保存成功：{SavePath}", this);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"角色存档保存失败：{exception.Message}", this);
            return false;
        }
    }

    public bool Load()
    {
        if (!TryValidateDependencies())
        {
            return false;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log($"未找到角色存档，使用角色初始配置：{SavePath}", this);
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            PlayerSaveData saveData =
                JsonUtility.FromJson<PlayerSaveData>(json);

            if (!TryRestore(saveData))
            {
                return false;
            }

            Debug.Log($"角色存档读取成功：{SavePath}", this);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"角色存档读取失败：{exception.Message}", this);
            return false;
        }
    }

    public void SetEquippedItems(
        IEnumerable<EquippedItemSaveData> items)
    {
        equippedItems.Clear();

        if (items != null)
        {
            equippedItems.AddRange(CloneEquippedItems(items));
        }
    }

    private bool TryRestore(PlayerSaveData saveData)
    {
        if (saveData == null || saveData.characterId <= 0)
        {
            Debug.LogError("角色存档无效：缺少有效的角色 ID。", this);
            return false;
        }

        if (saveData.version > PlayerSaveData.CurrentVersion)
        {
            Debug.LogError(
                $"角色存档版本 {saveData.version} 高于当前支持版本 " +
                $"{PlayerSaveData.CurrentVersion}。",
                this);
            return false;
        }

        if (!CharacterConfigRegistry.TryGetConfig(
                saveData.characterId,
                out CharacterBaseConfig config))
        {
            Debug.LogError(
                $"角色存档读取失败：找不到角色 ID " +
                $"{saveData.characterId} 对应的 CharacterBaseConfig。",
                this);
            return false;
        }

        playerStats.RestoreBaseConfig(config);
        playerStats.SetEquipmentModifiers(CharacterStatModifiers.Zero);
        playerStats.SetBuffModifiers(CharacterStatModifiers.Zero);
        playerProgression.RestoreProgress(
            saveData.level,
            saveData.currentExperience);
        playerStats.RestoreHealth(saveData.currentHealth);

        SetEquippedItems(saveData.equippedItems);
        EquipmentLoadoutManager.RestoreFromSaveData(equippedItems);

        return true;
    }

    private void SyncEquippedItems()
    {
        SetEquippedItems(EquipmentLoadoutManager.CreateSaveData());
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
        {
            Save();
        }
    }

    private void ResolveDependencies()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerProgression == null)
        {
            playerProgression = GetComponent<PlayerProgression>();
        }
    }

    private bool TryValidateDependencies()
    {
        ResolveDependencies();

        if (playerStats != null && playerProgression != null)
        {
            return true;
        }

        Debug.LogError(
            $"{nameof(PlayerSaveManager)} 需要 PlayerStats 和 " +
            "PlayerProgression。",
            this);
        return false;
    }

    private static List<EquippedItemSaveData> CloneEquippedItems(
        IEnumerable<EquippedItemSaveData> source)
    {
        List<EquippedItemSaveData> result =
            new List<EquippedItemSaveData>();

        foreach (EquippedItemSaveData item in source)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(item.slotId) ||
                string.IsNullOrWhiteSpace(item.itemInstanceId))
            {
                continue;
            }

            result.Add(new EquippedItemSaveData
            {
                slotId = item.slotId,
                itemInstanceId = item.itemInstanceId
            });
        }

        return result;
    }
}
