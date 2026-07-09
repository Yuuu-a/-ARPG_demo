using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-9)]
public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private AttackRootMotionController attackRootMotionController;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private PlayerInput playerInput;
#endif

    [Header("Attack Settings")]
    [SerializeField] private float attackLockTimeout = 5.0f;
    [SerializeField] private float[] comboRootMotionMultipliers = { 0.7f, 0.7f, 0.9f, 1f };
    [SerializeField] private bool allowMovementCancelDuringRecovery = true;
    [SerializeField] private float movementCancelInputThreshold = 0.1f;

    private const int MaxComboIndex = 4;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
    private static readonly int CanComboHash = Animator.StringToHash("CanCombo");
    private static readonly int MoveCancelHash = Animator.StringToHash("MoveCancel");

    private bool _isAttacking;
    private bool _attackTriggerRequested;
    private bool _canCombo;
    private bool _comboInputBuffered;
    private bool _moveCancelBuffered;
    private float _attackLockTimer;
    private int _currentComboIndex;

    public bool IsAttacking => _isAttacking;
    public bool CanCombo => _canCombo;
    public bool HasBufferedComboInput => _comboInputBuffered;
    public int CurrentComboIndex => _currentComboIndex;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Reset()
    {
        // Editor helper only. Please still check these references in Inspector.
        EnsureReferences();
#if ENABLE_INPUT_SYSTEM
        playerInput = GetComponent<PlayerInput>();
#endif
    }

    private void Update()
    {
        if (_isAttacking)
        {
            UpdateAttackState();
            if (!_isAttacking)
            {
                return;
            }

            TryBufferNextCombo();
            return;
        }

        SetAnimatorBool(MoveCancelHash, false);
        TryStartAttack();
    }

    private void TryStartAttack()
    {
        if (playerDash != null && playerDash.IsDashing)
        {
            ClearAttackInput();
            return;
        }

        if (!HasAttackInput())
        {
            return;
        }

        StartAttack();
        ClearAttackInput();
    }

    private void TryBufferNextCombo()
    {
        if (!HasAttackInput())
        {
            return;
        }

        if (_canCombo && _currentComboIndex < MaxComboIndex)
        {
            _comboInputBuffered = true;
            EnterComboSegment(_currentComboIndex + 1, true);
        }

        // Consume the input once, even when the combo window is closed.
        ClearAttackInput();
    }

    private bool HasAttackInput()
    {
        if (input != null && input.Attack)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
        {
            InputAction attackAction = playerInput.actions.FindAction("Attack", false);
            return attackAction != null && attackAction.WasPressedThisFrame();
        }
#endif

        return false;
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _attackTriggerRequested = false;
        _canCombo = false;
        _comboInputBuffered = false;
        _moveCancelBuffered = false;

        SetAnimatorBool(MoveCancelHash, false);

        if (attackRootMotionController != null)
        {
            attackRootMotionController.SetAttacking(true);
        }

        SetAnimatorBool(IsAttackingHash, true);
        SetAnimatorBool(CanComboHash, false);

        FaceAttackDirection();
        EnterComboSegment(1, false);
    }

    private void EnterComboSegment(int comboIndex, bool consumedBufferedInput)
    {
        _currentComboIndex = Mathf.Clamp(comboIndex, 1, MaxComboIndex);
        _attackLockTimer = attackLockTimeout;
        UpdateAttackRootMotion(_currentComboIndex);

        // A new attack segment must wait for its own animation event to open the next combo window.
        _canCombo = false;
        if (!consumedBufferedInput)
        {
            _comboInputBuffered = false;
        }

        SetAnimatorInteger(ComboIndexHash, _currentComboIndex);
        SetAnimatorBool(CanComboHash, false);
        SetAnimatorTrigger(AttackHash);
    }

    private void UpdateAttackState()
    {
        if (allowMovementCancelDuringRecovery && HasMoveInput())
        {
            _moveCancelBuffered = true;
        }

        if (TryCancelRecoveryWithMovement())
        {
            return;
        }

        // Safety fallback if the ending animation event does not call OnAttackAnimationEnd.
        _attackLockTimer -= Time.deltaTime;
        if (_attackLockTimer <= 0f)
        {
            EndAttack();
        }
    }

    public bool ConsumeAttackTrigger()
    {
        // Attack Trigger is now written directly by PlayerAttack.
        // Keep this method so PlayerAnimationController can still compile if it references it.
        if (!_attackTriggerRequested)
        {
            return false;
        }

        _attackTriggerRequested = false;
        return false;
    }

    public void OpenComboWindow()
    {
        if (!_isAttacking || _currentComboIndex >= MaxComboIndex)
        {
            return;
        }

        _canCombo = true;
        _comboInputBuffered = false;
        SetAnimatorBool(CanComboHash, true);
    }

    public void CloseComboWindow()
    {
        // Closing the window should not end the attack. Animator transitions decide whether to end or continue.
        _canCombo = false;
        SetAnimatorBool(CanComboHash, false);
    }

    public void OnAttackAnimationEnd(AnimationEvent animationEvent)
    {
        int eventComboIndex = animationEvent != null ? animationEvent.intParameter : 0;

        // Ignore delayed end events from a previous combo segment after we have
        // already transitioned into a newer one.
        if (eventComboIndex > 0 && eventComboIndex != _currentComboIndex)
        {
            return;
        }

        EndAttack();
    }

    private void EndAttack(bool resetMoveCancel = true)
    {
        if (attackRootMotionController != null)
        {
            attackRootMotionController.SetAttacking(false);
            attackRootMotionController.ResetRootMotionMultiplier();
        }

        _isAttacking = false;
        _attackTriggerRequested = false;
        _canCombo = false;
        _comboInputBuffered = false;
        _moveCancelBuffered = false;
        _currentComboIndex = 0;

        SetAnimatorBool(IsAttackingHash, false);
        SetAnimatorBool(CanComboHash, false);
        SetAnimatorInteger(ComboIndexHash, 0);
        if (resetMoveCancel)
        {
            SetAnimatorBool(MoveCancelHash, false);
        }

        ClearAttackInput();
    }

    private bool TryCancelRecoveryWithMovement()
    {
        if (!allowMovementCancelDuringRecovery || !_moveCancelBuffered || !IsInAttackRecoveryState())
        {
            return false;
        }

        SetAnimatorBool(MoveCancelHash, true);
        EndAttack(false);

        return true;
    }

    private bool HasMoveInput()
    {
        if (input != null && input.move.sqrMagnitude >= movementCancelInputThreshold * movementCancelInputThreshold)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
        {
            InputAction moveAction = playerInput.actions.FindAction("Move", false);
            return moveAction != null &&
                   moveAction.ReadValue<Vector2>().sqrMagnitude >= movementCancelInputThreshold * movementCancelInputThreshold;
        }
#endif

        return false;
    }

    private bool IsInAttackRecoveryState()
    {
        if (animator == null)
        {
            return false;
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        return IsAttackRecoveryState(currentState);
    }

    private static bool IsAttackRecoveryState(AnimatorStateInfo stateInfo)
    {
        return stateInfo.IsName("Attcak-aEnd01") ||
               stateInfo.IsName("Attack-aEnd02") ||
               stateInfo.IsName("Attack-aEnd03") ||
               stateInfo.IsName("Attack-aEnd04");
    }

    private void UpdateAttackRootMotion(int comboIndex)
    {
        if (attackRootMotionController == null)
        {
            return;
        }

        attackRootMotionController.SetAttackIndex(comboIndex);

        int arrayIndex = comboIndex - 1;
        if (comboRootMotionMultipliers != null &&
            arrayIndex >= 0 &&
            arrayIndex < comboRootMotionMultipliers.Length)
        {
            attackRootMotionController.SetRootMotionMultiplier(comboRootMotionMultipliers[arrayIndex]);
        }
    }

    private void EnsureReferences()
    {
        if (input == null)
        {
            input = GetComponent<StarterAssetsInputs>();
        }

        if (playerDash == null)
        {
            playerDash = GetComponent<PlayerDash>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (attackRootMotionController == null)
        {
            attackRootMotionController = GetComponent<AttackRootMotionController>();
        }

        if (attackRootMotionController == null)
        {
            attackRootMotionController = GetComponentInChildren<AttackRootMotionController>();
        }

#if ENABLE_INPUT_SYSTEM
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
#endif
    }

    private void ClearAttackInput()
    {
        if (input != null)
        {
            input.Attack = false;
        }
    }

    private void FaceAttackDirection()
    {
        if (input == null || input.move == Vector2.zero || mainCameraTransform == null)
        {
            return;
        }

        Vector3 inputDirection = new Vector3(input.move.x, 0f, input.move.y).normalized;
        float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                               + mainCameraTransform.eulerAngles.y;

        transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);
    }

    private void SetAnimatorTrigger(int parameterHash)
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(parameterHash);
        animator.SetTrigger(parameterHash);
    }

    private void SetAnimatorBool(int parameterHash, bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(parameterHash, value);
    }

    private void SetAnimatorInteger(int parameterHash, int value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetInteger(parameterHash, value);
    }
}
