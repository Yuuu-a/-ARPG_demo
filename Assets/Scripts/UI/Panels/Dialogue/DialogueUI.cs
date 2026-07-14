using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePanel : BasePanel
{
    [Header("UI")]
    [SerializeField] private TMP_Text dialogueText;

    // 当前 TXT 文件中的所有对话
    private readonly List<string> dialogueLines = new();

    // 当前显示的句子下标
    private int currentLineIndex;

    // 是否已经正式开始对话
    private bool isDialogueActive;

    // 记录打开对话的帧，避免打开面板的同一次点击直接跳过第一句
    private int dialogueStartFrame;
    private Action dialogueEndedCallback;

    public override void OpenPanel(string panelName)
    {
        // 负责激活面板、解锁鼠标、关闭玩家操作
        base.OpenPanel(panelName);

        dialogueLines.Clear();
        currentLineIndex = 0;
        isDialogueActive = false;
        dialogueEndedCallback = null;

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }
    }

    private void Update()
    {
        if (!isDialogueActive)
            return;

        // 避免刚打开对话的这一帧又读取到鼠标点击
        if (Time.frameCount == dialogueStartFrame)
            return;

        // 旧版输入系统：0 表示鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextLine();
        }
    }

    /// <summary>
    /// 接收 NPC 传入的 TXT 文件并开始对话。
    /// </summary>
    public void StartDialogue(
        TextAsset dialogueTxt,
        Action onDialogueEnded = null)
    {
        if (dialogueTxt == null)
        {
            Debug.LogWarning("NPC 没有配置对话 TXT 文件");
            UIManager.Instance.ClosePanel(UIConst.DialoguePanel);
            return;
        }

        LoadTxt(dialogueTxt);

        if (dialogueLines.Count == 0)
        {
            Debug.LogWarning("对话 TXT 文件中没有有效内容");
            UIManager.Instance.ClosePanel(UIConst.DialoguePanel);
            return;
        }

        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueStartFrame = Time.frameCount;
        dialogueEndedCallback = onDialogueEnded;

        ShowCurrentLine();
    }

    /// <summary>
    /// 显示下一句话。
    /// </summary>
    public void ShowNextLine()
    {
        if (!isDialogueActive || dialogueLines.Count == 0)
            return;

        currentLineIndex++;

        // 最后一句已经播放完毕
        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    /// <summary>
    /// 结束对话并关闭面板。
    /// </summary>
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueLines.Clear();

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        // 调用 BasePanel 的关闭流程：
        // 恢复玩家输入、锁定鼠标、销毁面板、从字典中移除
        Action callback = dialogueEndedCallback;
        dialogueEndedCallback = null;
        callback?.Invoke();
        UIManager.Instance.ClosePanel(UIConst.DialoguePanel);
    }

    /// <summary>
    /// 读取 TXT，并按照换行拆分。
    /// </summary>
    private void LoadTxt(TextAsset dialogueTxt)
    {
        dialogueLines.Clear();

        string content = dialogueTxt.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        string[] lines = content.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            dialogueLines.Add(line);
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueText == null)
        {
            Debug.LogWarning("DialoguePanel 没有绑定 DialogueText");
            return;
        }

        dialogueText.text = dialogueLines[currentLineIndex];
    }
}
