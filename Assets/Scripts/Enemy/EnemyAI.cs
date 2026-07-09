using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private NavMeshAgent agent;

    [Header("Detection")]
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float loseTargetRange = 10f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float turnSpeed = 12f;

    [Header("Animator Parameters")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string speedFloatName = "Speed";

    private int _attackHash;
    private int _speedHash;
    private float _attackTimer;
    private bool _isChasing;

    private void Awake()
    {
        EnsureReferences();
        CacheAnimatorHashes();
    }

    private void Reset()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();

        if (enemyHealth != null)
        {
            enemyHealth.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= HandleDied;
        }
    }

    private void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            StopMoving();
            UpdateSpeedParameter(0f);
            return;
        }

        EnsureTarget();

        if (target == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            UpdateSpeedParameter(0f);
            return;
        }

        _attackTimer -= Time.deltaTime;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (!_isChasing && distanceToTarget <= chaseRange)
        {
            _isChasing = true;
        }
        else if (_isChasing && distanceToTarget > loseTargetRange)
        {
            _isChasing = false;
        }

        if (!_isChasing)
        {
            StopMoving();
            UpdateSpeedParameter(0f);
            return;
        }

        if (distanceToTarget <= attackRange)
        {
            StopMoving();
            FaceTarget();
            TryAttack();
            UpdateSpeedParameter(0f);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(target.position);
        UpdateSpeedParameter(agent.velocity.magnitude);
    }

    private void TryAttack()
    {
        if (_attackTimer > 0f || animator == null)
        {
            return;
        }

        animator.ResetTrigger(_attackHash);
        animator.SetTrigger(_attackHash);
        _attackTimer = attackCooldown;
    }

    private void StopMoving()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void UpdateSpeedParameter(float speed)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(_speedHash, speed);
    }

    private void HandleDied(HitInfo hitInfo)
    {
        StopMoving();
        UpdateSpeedParameter(0f);

        if (agent != null)
        {
            agent.enabled = false;
        }

        enabled = false;
    }

    private void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void EnsureReferences()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        EnsureTarget();
    }

    private void CacheAnimatorHashes()
    {
        _attackHash = Animator.StringToHash(attackTriggerName);
        _speedHash = Animator.StringToHash(speedFloatName);
    }

    private void OnValidate()
    {
        chaseRange = Mathf.Max(0f, chaseRange);
        attackRange = Mathf.Max(0f, attackRange);
        loseTargetRange = Mathf.Max(chaseRange, loseTargetRange);
        attackCooldown = Mathf.Max(0f, attackCooldown);
        turnSpeed = Mathf.Max(0f, turnSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
