using UnityEngine;

public class AttackRootMotionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform motionRoot;
    [SerializeField] private Transform checkOrigin;

    [Header("Enemy Check")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float stopDistance = 0.8f;
    [SerializeField] private float checkRadius = 0.35f;

    [Header("Attack Root Motion")]
    [SerializeField] private float defaultRootMotionMultiplier = 1f;
    [SerializeField] private float[] attackRootMotionMultipliers = { 0.7f, 0.7f, 0.9f, 1f };
    [SerializeField] private bool applyAttackDeltaRotation;

    private bool _isAttacking;
    private bool _loggedMissingCharacterController;
    private int _attackIndex;
    private float _rootMotionMultiplier = 1f;

    public bool IsHandlingAttackRootMotion => enabled && _isAttacking;

    private void Awake()
    {
        EnsureReferences();
        ResetRootMotionMultiplier();
    }

    private void Reset()
    {
        EnsureReferences();
    }

    private void OnAnimatorMove()
    {
        EnsureReferences();

        if (animator == null)
        {
            return;
        }

        // 非攻击状态交给普通移动逻辑，避免代码移动和 Root Motion 叠加导致移速变快。
        if (!_isAttacking)
        {
            return;
        }

        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;

        // 攻击时保留 Root Motion 推进感，但按当前攻击段倍率缩放。
        deltaPosition *= _rootMotionMultiplier;

        if (ShouldBlockForwardMotion())
        {
            Vector3 forward = GetPlanarForward();
            float forwardAmount = Vector3.Dot(deltaPosition, forward);

            // 只裁掉继续向前挤进怪物身体的位移，保留左右、后退和竖直位移。
            if (forwardAmount > 0f)
            {
                deltaPosition -= forward * forwardAmount;
            }
        }

        ApplyRootMotion(deltaPosition, deltaRotation);
    }

    public void SetAttacking(bool attacking)
    {
        _isAttacking = attacking;
    }

    public void SetRootMotionMultiplier(float multiplier)
    {
        _rootMotionMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetAttackIndex(int attackIndex)
    {
        _attackIndex = attackIndex;

        int arrayIndex = attackIndex - 1;
        if (attackRootMotionMultipliers != null &&
            arrayIndex >= 0 &&
            arrayIndex < attackRootMotionMultipliers.Length)
        {
            SetRootMotionMultiplier(attackRootMotionMultipliers[arrayIndex]);
            return;
        }

        ResetRootMotionMultiplier();
    }

    public void ResetRootMotionMultiplier()
    {
        SetRootMotionMultiplier(defaultRootMotionMultiplier);
    }

    private void ApplyRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        if (characterController == null)
        {
            if (!_loggedMissingCharacterController)
            {
                Debug.LogError($"{nameof(AttackRootMotionController)} on {name} needs a CharacterController reference.", this);
                _loggedMissingCharacterController = true;
            }

            GetMotionRoot().position += deltaPosition;
        }
        else
        {
            characterController.Move(deltaPosition);
        }

        // 默认不应用攻击动画自带旋转，避免普攻动画把角色根对象转偏。
        if (applyAttackDeltaRotation)
        {
            GetMotionRoot().rotation *= deltaRotation;
        }
    }

    private bool ShouldBlockForwardMotion()
    {
        if (enemyLayer.value == 0)
        {
            return false;
        }

        Vector3 origin = GetCheckOrigin();
        Vector3 forward = GetPlanarForward();
        Vector3 checkCenter = origin + forward * (stopDistance * 0.5f);
        float overlapRadius = (stopDistance * 0.5f) + checkRadius;

        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            overlapRadius,
            enemyLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 closestPoint = hits[i].ClosestPoint(origin);
            Vector3 toTarget = closestPoint - origin;
            float forwardDistance = Vector3.Dot(toTarget, forward);

            if (forwardDistance >= 0f && forwardDistance <= stopDistance)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 GetCheckOrigin()
    {
        if (checkOrigin != null)
        {
            return checkOrigin.position;
        }

        return GetMotionRoot().position + Vector3.up;
    }

    private Vector3 GetPlanarForward()
    {
        Vector3 forward = GetMotionRoot().forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.001f)
        {
            return Vector3.forward;
        }

        return forward.normalized;
    }

    private void EnsureReferences()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        if (motionRoot == null && characterController != null)
        {
            motionRoot = characterController.transform;
        }
    }

    private Transform GetMotionRoot()
    {
        return motionRoot != null ? motionRoot : transform;
    }
}
