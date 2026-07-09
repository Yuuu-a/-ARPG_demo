using UnityEngine;

public class EnemyDamageReceiver : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;

    public bool CanReceiveDamage => enemyHealth != null && !enemyHealth.IsDead;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Reset()
    {
        EnsureReferences();
    }

    public void ReceiveHit(HitInfo hitInfo)
    {
        EnsureReferences();

        if (!CanReceiveDamage)
        {
            return;
        }

        enemyHealth.TakeDamage(hitInfo);
    }

    private void EnsureReferences()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }
    }
}
