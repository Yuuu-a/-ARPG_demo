using StarterAssets;
using UnityEngine;

public class UIInputController : MonoBehaviour
{
    [SerializeField] private StarterAssetsInputs playerInputs;

    private void Awake()
    {
        ResolvePlayerInputs();
    }

    private void OnEnable()
    {
        ResolvePlayerInputs();

        if (playerInputs == null)
        {
            Debug.LogWarning("UIInputController 没有找到 StarterAssetsInputs", this);
            return;
        }

        playerInputs.PackagePressed -= TogglePackagePanel;
        playerInputs.PackagePressed += TogglePackagePanel;
        playerInputs.CharacterPressed -= ToggleCharacterPanel;
        playerInputs.CharacterPressed += ToggleCharacterPanel;
    }

    private void OnDisable()
    {
        if (playerInputs == null)
        {
            return;
        }

        playerInputs.PackagePressed -= TogglePackagePanel;
        playerInputs.CharacterPressed -= ToggleCharacterPanel;
    }

    private void ResolvePlayerInputs()
    {
        if (playerInputs == null)
        {
            playerInputs = GetComponent<StarterAssetsInputs>();
        }

        if (playerInputs == null)
        {
            playerInputs = FindObjectOfType<StarterAssetsInputs>();
        }
    }

    private void TogglePackagePanel()
    {
        if (UIManager.Instance.IsPanelOpen(UIConst.PackagePanel))
        {
            UIManager.Instance.ClosePanel(UIConst.PackagePanel);
            return;
        }

        CloseOpenCharacterPanels();
        UIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    private void ToggleCharacterPanel()
    {
        if (CloseOpenCharacterPanels())
        {
            return;
        }

        CloseIfOpen(UIConst.PackagePanel);
        UIManager.Instance.OpenPanel(UIConst.CharacterPanel);
    }

    private bool CloseOpenCharacterPanels()
    {
        bool closedAnyPanel = false;

        closedAnyPanel |= CloseIfOpen(UIConst.JingYanPanel);
        closedAnyPanel |= CloseIfOpen(UIConst.EquipmentPackagePanel);
        closedAnyPanel |= CloseIfOpen(UIConst.AbilityPanel);
        closedAnyPanel |= CloseIfOpen(UIConst.EquipmentPanel);
        closedAnyPanel |= CloseIfOpen(UIConst.CharacterPanel);

        return closedAnyPanel;
    }

    private bool CloseIfOpen(string panelName)
    {
        if (!UIManager.Instance.IsPanelOpen(panelName))
        {
            return false;
        }

        UIManager.Instance.ClosePanel(panelName);
        return true;
    }
}
