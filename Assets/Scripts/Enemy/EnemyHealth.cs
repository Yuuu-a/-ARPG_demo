using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5000;
    [SerializeField] private bool logDamage;

    [Header("Enemy Identity")]
    [SerializeField] private string enemyId = "Skeleton";

    private int _currentHealth;
    private bool _isDead;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => _isDead;
    public string EnemyId => enemyId;

    public event Action<HitInfo> OnDamaged;
    public event Action<HitInfo> OnDied;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(HitInfo hitInfo)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth = Mathf.Max(_currentHealth - hitInfo.Damage, 0);

        if (logDamage)
        {
            Debug.Log($"{name} took {hitInfo.Damage} damage. HP: {_currentHealth}/{maxHealth}", this);
        }

        if (_currentHealth <= 0)
        {
            _isDead = true;
            EventCenter.Publish(new EnemyDiedEvent(this, hitInfo));
            OnDied?.Invoke(hitInfo);
            return;
        }

        OnDamaged?.Invoke(hitInfo);
    }
}
