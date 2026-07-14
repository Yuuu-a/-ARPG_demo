using System.Collections.Generic;
using UnityEngine;

public static class CharacterConfigRegistry
{
    private const string ResourcesPath = "Config";

    private static Dictionary<int, CharacterBaseConfig> configsById;

    public static bool TryGetConfig(
        int characterId,
        out CharacterBaseConfig config)
    {
        EnsureInitialized();
        return configsById.TryGetValue(characterId, out config);
    }

    private static void EnsureInitialized()
    {
        if (configsById != null)
        {
            return;
        }

        configsById = new Dictionary<int, CharacterBaseConfig>();
        CharacterBaseConfig[] configs =
            Resources.LoadAll<CharacterBaseConfig>(ResourcesPath);

        foreach (CharacterBaseConfig config in configs)
        {
            if (config == null)
            {
                continue;
            }

            if (configsById.ContainsKey(config.CharacterId))
            {
                Debug.LogError(
                    $"发现重复的角色配置 ID：{config.CharacterId}。" +
                    $"配置 {config.name} 将被忽略。");
                continue;
            }

            configsById.Add(config.CharacterId, config);
        }
    }
}
