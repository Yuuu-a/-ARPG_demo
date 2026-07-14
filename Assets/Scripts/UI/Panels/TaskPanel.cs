using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : BasePanel
{
    [Header("任务 UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject taskFinishedText;

    private void OnEnable()
    {
        closeButton?.onClick.RemoveListener(CloseTaskPanel);
        closeButton?.onClick.AddListener(CloseTaskPanel);
        completeButton?.onClick.RemoveListener(CompleteTask);
        completeButton?.onClick.AddListener(CompleteTask);
        SkeletonQuestManager.QuestChanged -= Refresh;
        SkeletonQuestManager.QuestChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        closeButton?.onClick.RemoveListener(CloseTaskPanel);
        completeButton?.onClick.RemoveListener(CompleteTask);
        SkeletonQuestManager.QuestChanged -= Refresh;
    }

    public override void OpenPanel(string panelName)
    {
        base.OpenPanel(panelName);
        Refresh();
    }

    private void Refresh()
    {
        int killCount = Mathf.Clamp(
            SkeletonQuestManager.CurrentKillCount,
            0,
            SkeletonQuestManager.RequiredKillCount);
        string progress =
            $"{killCount}/{SkeletonQuestManager.RequiredKillCount}";

        if (progressText != null)
        {
            progressText.text = progress;
        }

        bool targetReached = killCount >=
            SkeletonQuestManager.RequiredKillCount;

        if (taskText != null)
        {
            taskText.text = "消灭 NPC 左手边的一只骷髅";
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(!targetReached);
        }

        if (taskFinishedText != null)
        {
            taskFinishedText.SetActive(targetReached);
        }

        if (completeButton != null)
        {
            completeButton.interactable = SkeletonQuestManager.CanComplete;
        }
    }

    private void CompleteTask()
    {
        SkeletonQuestManager.CompleteQuest();
    }

    private void CloseTaskPanel()
    {
        UIManager.Instance.ClosePanel(UIConst.TaskPanel);
    }
}
