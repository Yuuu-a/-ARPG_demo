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
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private PlayerInput playerInput;
#endif

    [Header("Attack Settings")]
    [SerializeField] private float attackLockTimeout = 2f;

    private bool _isAttacking;
    private bool _attackTriggerRequested;
    private bool _canCombo;
    private float _attackLockTimer;
    private int _currentComboIndex;

    public bool IsAttacking => _isAttacking;
    public bool CanCombo => _canCombo;
    public int CurrentComboIndex => _currentComboIndex;

    private void Reset()
    {
        // 编辑器辅助填充，最终请在 Inspector 中确认引用。
        input = GetComponent<StarterAssetsInputs>();
        playerDash = GetComponent<PlayerDash>();
#if ENABLE_INPUT_SYSTEM
        playerInput = GetComponent<PlayerInput>();
#endif
    }

    private void Update()
    {
        if (_isAttacking)
        {
            UpdateAttackState();
            return;
        }

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
        _attackTriggerRequested = true;
        _canCombo = false;
        _currentComboIndex = 1;
        _attackLockTimer = attackLockTimeout;

        FaceAttackDirection();
    }

    private void UpdateAttackState()
    {
        // 如果动画事件没有正确调用结束方法，用超时保护避免攻击状态永久卡住。
        _attackLockTimer -= Time.deltaTime;
        if (_attackLockTimer <= 0f)
        {
            EndAttack();
        }
    }

    public bool ConsumeAttackTrigger()
    {
        // Trigger 只应该被 Animator 消费一次，避免每帧重复触发攻击动画。
        if (!_attackTriggerRequested)
        {
            return false;
        }

        _attackTriggerRequested = false;
        return true;
    }

    public void OpenComboWindow()
    {
        // 后续四段连招可以在攻击动画事件里打开可缓存输入窗口。
        _canCombo = true;
    }

    public void CloseComboWindow()
    {
        _canCombo = false;
    }

    public void OnAttackAnimationEnd()
    {
        EndAttack();
    }

    private void EndAttack()
    {
        _isAttacking = false;
        _attackTriggerRequested = false;
        _canCombo = false;
        _currentComboIndex = 0;
        ClearAttackInput();
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
}
