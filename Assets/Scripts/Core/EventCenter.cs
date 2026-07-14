using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局强类型事件中心。
/// 发布者只负责发送事件，监听者负责处理自己的业务逻辑。
/// </summary>
public static class EventCenter
{
    private static readonly Dictionary<Type, Delegate> listeners =
        new Dictionary<Type, Delegate>();

    public static void AddListener<TEvent>(Action<TEvent> listener)
    {
        if (listener == null)
        {
            return;
        }

        Type eventType = typeof(TEvent);

        if (listeners.TryGetValue(eventType, out Delegate registeredListeners))
        {
            Action<TEvent> callbacks = (Action<TEvent>)registeredListeners;
            callbacks -= listener;
            callbacks += listener;
            listeners[eventType] = callbacks;
            return;
        }

        listeners.Add(eventType, listener);
    }

    public static void RemoveListener<TEvent>(Action<TEvent> listener)
    {
        if (listener == null)
        {
            return;
        }

        Type eventType = typeof(TEvent);

        if (!listeners.TryGetValue(eventType, out Delegate registeredListeners))
        {
            return;
        }

        Action<TEvent> callbacks = (Action<TEvent>)registeredListeners;
        callbacks -= listener;

        if (callbacks == null)
        {
            listeners.Remove(eventType);
        }
        else
        {
            listeners[eventType] = callbacks;
        }
    }

    public static void Publish<TEvent>(TEvent eventData)
    {
        if (listeners.TryGetValue(
                typeof(TEvent),
                out Delegate registeredListeners))
        {
            ((Action<TEvent>)registeredListeners).Invoke(eventData);
        }
    }

    public static void RemoveAllListeners<TEvent>()
    {
        listeners.Remove(typeof(TEvent));
    }

    public static void Clear()
    {
        listeners.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetRuntimeListeners()
    {
        Clear();
    }
}

/// <summary>
/// 任意怪物死亡时发布的全局事件数据。
/// </summary>
public readonly struct EnemyDiedEvent
{
    public EnemyDiedEvent(EnemyHealth enemy, HitInfo lastHit)
    {
        Enemy = enemy;
        LastHit = lastHit;
    }

    public EnemyHealth Enemy { get; }
    public HitInfo LastHit { get; }
}
