using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : BasePanel
{
    [SerializeField] private GameObject containerGameObject; //提示ui面板 不用UIManager
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI InteractText;

    private IInteractable currentInteractable;

    private void Update()
    {
        IInteractable newInteractable = playerInteract.GetInteractableObject();

        // 检测对象没有发生变化，不更新 UI
        if (newInteractable == currentInteractable)
            return;

        currentInteractable = newInteractable;

        if (currentInteractable != null)
        {
            Show(currentInteractable);
        }
        else
        {
            Hide();
        }
    }

    private void Show(IInteractable interactable)
    {
        containerGameObject.SetActive(true);
        InteractText.text = interactable.GetInteractText();
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }

}
