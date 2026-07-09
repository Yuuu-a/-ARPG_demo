using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDataConfig
{
    public List<PackageItem> ItemDataList = new List<PackageItem>();
}

[System.Serializable]
public class PackageItem
{
    public int id;
    public int type;
    public string name;
    public string description;
    public string imagePath;
}
