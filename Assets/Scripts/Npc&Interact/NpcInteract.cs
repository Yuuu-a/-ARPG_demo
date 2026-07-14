using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcInteract : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private TextAsset dialogueFile;
    [SerializeField] private TextAsset taskCompletedDialogueFile;
    [SerializeField] private string interactText;

    private void OnEnable()
    {
        EventCenter.AddListener<EnemyDiedEvent>(HandleEnemyDied);
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener<EnemyDiedEvent>(HandleEnemyDied);
    }

    public void Interact()
    {
        DialoguePanel dialoguePanel =
            UIManager.Instance.OpenPanel(UIConst.DialoguePanel)
            as DialoguePanel;

        if (dialoguePanel == null)
            return;

        dialoguePanel.StartDialogue(dialogueFile, HandleDialogueEnded);
    }

    private void HandleDialogueEnded()
    {
        SkeletonQuestManager.AcceptQuest();

        if (!UIManager.Instance.IsPanelOpen(UIConst.TaskPanel))
        {
            UIManager.Instance.OpenPanel(UIConst.TaskPanel);
        }
    }

    private void HandleEnemyDied(EnemyDiedEvent eventData)
    {
        EnemyHealth enemy = eventData.Enemy;

        if (enemy == null ||
            SkeletonQuestManager.State == SkeletonQuestState.NotAccepted ||
            !string.Equals(
                enemy.EnemyId,
                SkeletonQuestManager.TargetEnemyId,
                System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (taskCompletedDialogueFile != null)
        {
            dialogueFile = taskCompletedDialogueFile;
        }
    }
    public string GetInteractText()
    {
        return interactText;
    }

    public Transform GetTransform()
    {
        return transform;
    }

}
