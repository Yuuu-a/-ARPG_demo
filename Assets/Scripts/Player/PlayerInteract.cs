using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            IInteractable interactable = GetInteractableObject();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    public IInteractable GetInteractableObject()
    {
        List<IInteractable> InteractableList = new List<IInteractable>();

        float InteractRange = 2f;
        Collider[] ColliderArray = Physics.OverlapSphere(transform.position, InteractRange); //补习物理系统

        foreach (var Collider in ColliderArray)
        {
            if (Collider.TryGetComponent<IInteractable>(out IInteractable iIteractableObject))
            {
                InteractableList.Add(iIteractableObject);
            }
        }


        IInteractable closeInteractable = null;
        foreach (IInteractable iIteractable in InteractableList)
        {
            if (closeInteractable == null)
            {
                closeInteractable = iIteractable;
            }
            else
            {
                if (Vector3.Distance(transform.position, iIteractable.GetTransform().position) <
                Vector3.Distance(transform.position, closeInteractable.GetTransform().position))
                    closeInteractable = iIteractable;
            }
        }


        return closeInteractable;
    }
}
