using System;
using UnityEngine;

public enum SkeletonQuestState
{
    NotAccepted,
    InProgress,
    ReadyToComplete,
    Completed
}

public static class SkeletonQuestManager
{
    public const int RequiredKillCount = 1;
    public const string TargetEnemyId = "Skeleton";

    private static bool isSubscribed;
    private static int currentKillCount;
    private static SkeletonQuestState state = SkeletonQuestState.NotAccepted;

    public static event Action QuestChanged;

    public static int CurrentKillCount => currentKillCount;
    public static SkeletonQuestState State => state;
    public static bool CanComplete => state == SkeletonQuestState.ReadyToComplete;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetRuntimeState()
    {
        if (isSubscribed)
        {
            EventCenter.RemoveListener<EnemyDiedEvent>(HandleEnemyDied);
        }

        isSubscribed = false;
        currentKillCount = 0;
        state = SkeletonQuestState.NotAccepted;
        QuestChanged = null;
    }

    public static void AcceptQuest()
    {
        EnsureSubscribed();

        if (state != SkeletonQuestState.NotAccepted)
        {
            return;
        }

        currentKillCount = 0;
        state = SkeletonQuestState.InProgress;
        QuestChanged?.Invoke();
    }

    public static bool CompleteQuest()
    {
        if (!CanComplete)
        {
            return false;
        }

        state = SkeletonQuestState.Completed;
        QuestChanged?.Invoke();
        return true;
    }

    private static void EnsureSubscribed()
    {
        if (isSubscribed)
        {
            return;
        }

        EventCenter.AddListener<EnemyDiedEvent>(HandleEnemyDied);
        isSubscribed = true;
    }

    private static void HandleEnemyDied(EnemyDiedEvent eventData)
    {
        EnemyHealth enemy = eventData.Enemy;

        if (state != SkeletonQuestState.InProgress ||
            enemy == null ||
            !string.Equals(
                enemy.EnemyId,
                TargetEnemyId,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        currentKillCount = Math.Min(
            currentKillCount + 1,
            RequiredKillCount);

        if (currentKillCount >= RequiredKillCount)
        {
            state = SkeletonQuestState.ReadyToComplete;
        }

        QuestChanged?.Invoke();
    }
}
