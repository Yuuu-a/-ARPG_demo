using StarterAssets;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BasePanel : MonoBehaviour
{
    protected bool isRemove;
    protected new string name;

    private StarterAssetsInputs playerInput;
    private bool isCursorReleased;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput unityPlayerInput;
    private InputAction packageAction;
    private InputAction characterAction;
    private bool isPlayerInputDeactivated;
#endif

    public virtual void OpenPanel(string name)
    {
        this.name = name;
        gameObject.SetActive(true);
        EnterUIMode();
    }

    public virtual void ClosePanel()
    {
        bool keepUIMode = UIManager.Instance != null &&
            UIManager.Instance.HasOtherOpenPanel(name);

        if (keepUIMode)
        {
            // Another panel is still visible underneath this one.
            // Leave the shared cursor and player input in UI mode.
            isCursorReleased = false;
        }
        else
        {
            ExitUIMode();
        }

        isRemove = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    protected virtual void EnterUIMode()
    {
        if (isCursorReleased)
        {
            return;
        }

        playerInput = FindObjectOfType<StarterAssetsInputs>();

        if (playerInput != null)
        {
            playerInput.cursorLocked = false;
            playerInput.cursorInputForLook = false;
            ResetGameplayInput();

#if ENABLE_INPUT_SYSTEM
            unityPlayerInput = playerInput.GetComponent<PlayerInput>();

            if (unityPlayerInput != null && unityPlayerInput.enabled)
            {
                unityPlayerInput.DeactivateInput();
                isPlayerInputDeactivated = true;

                packageAction =
                    unityPlayerInput.actions.FindAction("Package", false);
                characterAction =
                    unityPlayerInput.actions.FindAction("Character", false);
                packageAction?.Enable();
                characterAction?.Enable();
            }
#endif
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorReleased = true;
    }

    protected virtual void ExitUIMode()
    {
        if (!isCursorReleased)
        {
            return;
        }

        if (playerInput == null)
        {
            playerInput = FindObjectOfType<StarterAssetsInputs>();
        }

        if (playerInput != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (unityPlayerInput == null)
            {
                unityPlayerInput = playerInput.GetComponent<PlayerInput>();
            }

            if (unityPlayerInput != null && isPlayerInputDeactivated)
            {
                packageAction?.Disable();
                characterAction?.Disable();
                unityPlayerInput.ActivateInput();
                isPlayerInputDeactivated = false;
                packageAction = null;
                characterAction = null;
            }
#endif

            playerInput.cursorLocked = true;
            playerInput.cursorInputForLook = true;
            ResetGameplayInput();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorReleased = false;
    }

    private void ResetGameplayInput()
    {
        if (playerInput == null)
        {
            return;
        }

        playerInput.MoveInput(Vector2.zero);
        playerInput.LookInput(Vector2.zero);
        playerInput.SprintInput(false);
        playerInput.Dash = false;
        playerInput.Attack = false;
    }

    protected virtual void OnDestroy()
    {
        ExitUIMode();
    }
}
