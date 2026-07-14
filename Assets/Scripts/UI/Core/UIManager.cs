using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Dictionary<string, string> pathDic;
    private Dictionary<string, GameObject> prefabsDic;
    private Dictionary<string, BasePanel> panelDic;

    private static UIManager _instance;
    [SerializeField] private Transform uiRoot;

    public Transform UIRoot
    {
        get
        {
            if (uiRoot == null)
            {
                Canvas canvas = GetComponent<Canvas>();

                if (canvas == null)
                {
                    canvas = FindObjectOfType<Canvas>(true);
                }

                if (canvas != null)
                {
                    uiRoot = canvas.transform;
                }
                else
                {
                    Debug.LogError("场景中没有找到 Canvas，无法打开 UI 面板");
                }
            }

            return uiRoot;
        }
    }

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>(true);

                if (_instance == null && Application.isPlaying)
                {
                    GameObject managerObject = new GameObject(nameof(UIManager));
                    _instance = managerObject.AddComponent<UIManager>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        InitDic();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void InitDic()
    {
        prefabsDic = new Dictionary<string, GameObject>();
        panelDic = new Dictionary<string, BasePanel>();
        pathDic = new Dictionary<string, string>
        {
            {UIConst.PackagePanel, "PackagePanel"},
            {UIConst.CharacterPanel, "CharacterPanel"},
            {UIConst.AbilityPanel, "AbilityPanel"},
            {UIConst.JingYanPanel, "JingYanPanel"},
            {UIConst.EquipmentPanel, "EquipmentPanel"},
            {UIConst.EquipmentPackagePanel, "EquipmentPackagePanel"},
            {UIConst.DialoguePanel, "DialoguePanel"},
            {UIConst.TaskPanel, "TaskPanel"}
        };
    }

    public BasePanel OpenPanel(string name) //传入一个name 检测是否已经打开 使用过
    {
        EnsureInitialized();

        if (panelDic.TryGetValue(name, out BasePanel panel))
        {
            Debug.LogError("该面板已经被打开了:" + name);
            return null;
        }

        if (!pathDic.TryGetValue(name, out string path))
        {
            Debug.LogError("该面板未配置路径:" + name);
            return null;
        }

        if (!prefabsDic.TryGetValue(name, out GameObject panelPrefab))
        {
            string realPath = "Prefabs/UIPanel/" + path;
            panelPrefab = Resources.Load<GameObject>(realPath);

            if (panelPrefab == null)
            {
                Debug.LogError("没有找到 UI Prefab：Resources/" + realPath);
                return null;
            }

            prefabsDic.Add(name, panelPrefab);
        }

        if (UIRoot == null)
        {
            return null;
        }

        GameObject panelObject = Instantiate(panelPrefab, UIRoot, false);
        panel = panelObject.GetComponent<BasePanel>();

        if (panel == null)
        {
            Debug.LogError($"UI Prefab {path} 的根节点没有挂载 BasePanel 或其子类");
            Destroy(panelObject);
            return null;
        }

        panel.OpenPanel(name);
        panelDic.Add(name, panel);
        return panel;
    }

    public bool ClosePanel(string name)
    {
        EnsureInitialized();

        if (!panelDic.TryGetValue(name, out BasePanel panel))
        {
            Debug.LogError("界面未被打开");
            return false;
        }

        panel.ClosePanel();

        if (panelDic.TryGetValue(name, out BasePanel registeredPanel) &&
            registeredPanel == panel)
        {
            panelDic.Remove(name);
        }

        return true;
    }

    public void TogglePanel(string name)
    {
        EnsureInitialized();

        if (panelDic.ContainsKey(name))
        {
            ClosePanel(name);
        }
        else
        {
            OpenPanel(name);
        }
    }

    public bool IsPanelOpen(string name)
    {
        EnsureInitialized();
        return panelDic.ContainsKey(name);
    }

    public bool HasOtherOpenPanel(string panelName)
    {
        EnsureInitialized();

        foreach (string openPanelName in panelDic.Keys)
        {
            if (openPanelName != panelName)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureInitialized()
    {
        if (pathDic == null || prefabsDic == null || panelDic == null)
        {
            InitDic();
        }
    }
}

public class UIConst
{
    public const string PackagePanel = "PackagePanel";
    public const string CharacterPanel = "CharacterPanel";
    public const string AbilityPanel = "AbilityPanel";
    public const string JingYanPanel = "JingYanPanel";
    public const string EquipmentPanel = "EquipmentPanel";
    public const string EquipmentPackagePanel = "EquipmentPackagePanel";
    public const string DialoguePanel = "DialoguePanel";
    public const string TaskPanel = "TaskPanel";
}
