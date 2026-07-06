using UnityEngine;
using StarterAssets;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f; //冷却

    private StarterAssetsInputs _input;
    private Transform _mainCameraTransform;

    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector3 _dashDirection;

    public bool IsDashing => _isDashing;
    public float DashSpeed => dashSpeed;
    public Vector3 DashDirection => _dashDirection;

    private void Awake()
    {
        _input = GetComponent<StarterAssetsInputs>();

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            _mainCameraTransform = mainCamera.transform;
        }
    }

    private void Update() //先更新冷却时间，再更新冲刺状态，最后尝试开始冲刺
    {
        UpdateCooldown();
        UpdateDashState();
        TryStartDash();
    }

    private void UpdateCooldown()
    {
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateDashState()
    {
        if (!_isDashing)
        {
            return;
        }

        _dashTimer -= Time.deltaTime;

        if (_dashTimer <= 0f)
        {
            _isDashing = false;
        }
    }

    private void TryStartDash()
    {
        if (_isDashing) //正在dash 不允许再次dash
        {
            return;
        }

        if (_dashCooldownTimer > 0f)
        {
            return;
        }

        if (!_input.Dash)
        {
            return;
        }

        StartDash();

        // 消耗这次输入，防止一直触发
        _input.Dash = false;
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;

        _dashDirection = GetDashDirection();
    }

    private Vector3 GetDashDirection() //获取dash方向 这样的话dash方向就可以和移动方向一致了 如果没有移动方向 就朝角色当前面朝方向 Dash 
    {
        Vector2 moveInput = _input.move;

        // 如果有移动输入，就朝移动方向 Dash
        if (moveInput != Vector2.zero && _mainCameraTransform != null)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                                   + _mainCameraTransform.eulerAngles.y;

            return Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
        }

        // 如果没有移动输入，就朝角色当前面朝方向 Dash
        return transform.forward;
    }
}