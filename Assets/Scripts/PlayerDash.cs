using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-10)]
public class PlayerDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Transform mainCameraTransform;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private PlayerInput playerInput;
#endif

    [Header("Dash Settings")]
    [SerializeField] private float dashCooldown = 1f;

    private bool _isDashing;
    private bool _dashTriggerRequested;
    private float _dashCooldownTimer;
    private Vector3 _dashDirection;

    public bool IsDashing => _isDashing;
    public Vector3 DashDirection => _dashDirection;

    private void Reset()
    {
        // 编辑器辅助填充，最终请在 Inspector 中确认引用。
        input = GetComponent<StarterAssetsInputs>();
        playerAttack = GetComponent<PlayerAttack>();
#if ENABLE_INPUT_SYSTEM
        playerInput = GetComponent<PlayerInput>();
#endif
    }

    private void Update()
    {
        UpdateCooldown();
        TryStartDash();
    }

    private void UpdateCooldown()
    {
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void TryStartDash()
    {
        if (_isDashing)
        {
            return;
        }

        if (playerAttack != null && playerAttack.IsAttacking)
        {
            if (input != null)
            {
                input.Dash = false;
            }

            return;
        }

        if (_dashCooldownTimer > 0f)
        {
            return;
        }

        if (!HasDashInput())
        {
            return;
        }

        StartDash();

        if (input != null)
        {
            input.Dash = false;
        }
    }

    private bool HasDashInput()
    {
        if (input != null && input.Dash)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (playerInput != null)
        {
            InputAction dashAction = playerInput.actions.FindAction("Dash", false);
            return dashAction != null && dashAction.IsPressed();
        }
#endif

        return false;
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashCooldownTimer = dashCooldown;
        _dashDirection = GetDashDirection();

        transform.rotation = Quaternion.LookRotation(_dashDirection, Vector3.up);
        _dashTriggerRequested = true;
    }

    public bool ConsumeDashTrigger()
    {
        // Trigger 只应该被 Animator 消费一次，避免每帧重复触发 Dash 动画。
        if (!_dashTriggerRequested)
        {
            return false;
        }

        _dashTriggerRequested = false;
        return true;
    }

    public void OnDashAnimationEnd()
    {
        EndDash();
    }

    private void EndDash()
    {
        _isDashing = false;
    }

    private Vector3 GetDashDirection()
    {
        Vector2 moveInput = input != null ? input.move : Vector2.zero;

        if (moveInput != Vector2.zero && mainCameraTransform != null)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                                   + mainCameraTransform.eulerAngles.y;

            return Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
        }

        return transform.forward;
    }
}
