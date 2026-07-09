using UnityEngine;

public class EnemyHitReaction : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private bool logHits;

    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private void Awake()
    {
        EnsureReferences();
    }

    private void Reset()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();

        if (enemyHealth == null)
        {
            Debug.LogWarning($"{nameof(EnemyHitReaction)} on {name} has no EnemyHealth reference.", this);
            return;
        }

        enemyHealth.OnDamaged += HandleDamaged;
        enemyHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.OnDamaged -= HandleDamaged;
        enemyHealth.OnDied -= HandleDied;
    }

    private void HandleDamaged(HitInfo hitInfo)
    {
        if (animator == null)
        {
            Debug.LogWarning($"{nameof(EnemyHitReaction)} on {name} has no Animator reference.", this);
            return;
        }

        if (logHits)
        {
            Debug.Log($"{name} plays Hit reaction. Damage: {hitInfo.Damage}, Combo: {hitInfo.ComboIndex}", this);
        }

        animator.ResetTrigger(HitHash);
        animator.SetTrigger(HitHash);
    }

    private void HandleDied(HitInfo hitInfo)
    {
        if (animator == null)
        {
            Debug.LogWarning($"{nameof(EnemyHitReaction)} on {name} has no Animator reference.", this);
            return;
        }

        animator.ResetTrigger(DeadHash);
        animator.SetTrigger(DeadHash);
    }

    private void EnsureReferences()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
}
