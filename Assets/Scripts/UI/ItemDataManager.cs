using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance { get; private set; }

    private Dictionary<int, PackageItem> itemDataDict = new Dictionary<int, PackageItem>();

    private void Awake()
    {
        Instance = this;
        LoadItemData();
    }

    private void LoadItemData()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("Config/WeaponConfig");

        if (jsonAsset == null)
        {
            Debug.LogError("没有找到Json文件");
            return;
        }

        ItemDataConfig config = JsonUtility.FromJson<ItemDataConfig>(jsonAsset.text);

        if (config == null || config.ItemDataList == null)
        {
            Debug.LogError("ItemData.json 解析失败，请检查 JSON 格式");
            return;
        }

        itemDataDict.Clear();

        foreach (PackageItem item in config.ItemDataList)
        {
            if (itemDataDict.ContainsKey(item.id))
            {
                Debug.LogWarning($"物品 id 重复：{item.id}");
                continue;
            }

            itemDataDict.Add(item.id, item);
        }

        Debug.Log($"物品配置加载完成，数量：{itemDataDict.Count}");
    }

    public PackageItem GetItemData(int id)
    {
        if (itemDataDict.TryGetValue(id, out PackageItem item))
        {
            return item;
        }

        Debug.LogWarning($"没有找到物品 id：{id}");
        return null;
    }

    public Sprite GetItemIcon(int id)
    {
        PackageItem item = GetItemData(id);

        if (item == null)
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(item.imagePath);

        if (sprite == null)
        {
            Debug.LogWarning($"没有找到物品图片：{item.imagePath}");
        }

        return sprite;
    }
}