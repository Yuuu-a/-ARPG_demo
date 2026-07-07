using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))] // 需要角色控制器组件
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif

    #region 变量
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.335f;

        [Tooltip("Animator Speed value used by the move animation threshold")]
        public float MoveAnimationSpeed = 4.0f;

        [Tooltip("Animator Speed value used by the run animation threshold")]
        public float SprintAnimationSpeed = 7.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // Cinemachine 摄像机相关
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // 玩家移动相关
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // 下落计时器
        private float _fallTimeoutDelta;

        // 给 PlayerAnimationController 读取的移动动画数据
        private float _motionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private PlayerDash _playerDash;
        private PlayerAttack _playerAttack;

        private const float _threshold = 0.01f;

        public float AnimationBlend => _animationBlend;
        public float MotionSpeed => _motionSpeed;

        #endregion

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // 获取主摄像机引用
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y; // 初始化摄像机偏航角为摄像机目标的 Y 轴旋转角度

            TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerDash = GetComponent<PlayerDash>();
            _playerAttack = GetComponent<PlayerAttack>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // 启动时重置计时器
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            if (_animator == null)
            {
                TryGetComponent(out _animator);
            }

            GroundedCheck();
            ApplyGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

#if ENABLE_INPUT_SYSTEM
        public void InputMove(InputAction.CallbackContext context)
        {
            EnsureInputReference();
            _input.MoveInput(context.ReadValue<Vector2>());
        }

        public void InputLook(InputAction.CallbackContext context)
        {
            EnsureInputReference();
            if (_input.cursorInputForLook)
            {
                _input.LookInput(context.ReadValue<Vector2>());
            }
        }

        public void InputJump(InputAction.CallbackContext context)
        {
            // Jump is intentionally disabled, but this method is kept for existing PlayerInput event bindings.
        }

        public void InputSprint(InputAction.CallbackContext context)
        {
            EnsureInputReference();
            _input.SprintInput(context.ReadValueAsButton());
        }

        private void EnsureInputReference()
        {
            if (_input == null)
            {
                _input = GetComponent<StarterAssetsInputs>();
            }
        }
#endif

        private void GroundedCheck() // 生成一个球体来检测角色是否在地面上，球体半径应和角色控制器半径一致
        {
            // 设置带偏移量的检测球位置
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation() // 摄像机旋转
        {
            // 如果有视角输入且摄像机位置没有被锁定
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 鼠标输入不要乘以 Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 限制旋转角度，避免数值超出 360 度范围
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine 会跟随这个目标
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            if (IsUsingRootMotionAction())
            {
                _speed = 0f;
                _animationBlend = 0f;
                _motionSpeed = 0f;

                return;
            }
            // 根据移动速度、冲刺速度和冲刺输入设置实际移动速度与动画阈值
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            float animationTargetSpeed = _input.sprint ? SprintAnimationSpeed : MoveAnimationSpeed;

            // 简化的加速和减速逻辑，便于后续移除、替换或迭代

            // 注意：Vector2 的 == 运算符使用近似比较，不容易受浮点误差影响，并且比 magnitude 更省性能
            // 如果没有移动输入，则将目标速度设为 0
            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
                animationTargetSpeed = 0.0f;
            }

            // 获取玩家当前水平速度
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            _motionSpeed = inputMagnitude;

            // 加速或减速到目标速度
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 生成曲线变化结果，而不是线性变化，让速度过渡更自然
                // 注意：Lerp 的 T 参数会被限制，因此不需要额外限制速度
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // 将速度四舍五入到 3 位小数
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, animationTargetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 标准化输入方向
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // 注意：Vector2 的 != 运算符使用近似比较，不容易受浮点误差影响，并且比 magnitude 更省性能
            // 如果有移动输入，则在玩家移动时旋转玩家
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // 旋转角色，使其朝向相对于摄像机的输入方向
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // 移动玩家
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        }

        private void OnAnimatorMove()
        {
            if (!IsUsingRootMotionAction() || _animator == null || _controller == null)
            {
                return;
            }

            Vector3 rootMotionDelta = _animator.deltaPosition; //获取动画器的位移增量
            rootMotionDelta.y = 0f;

            Vector3 gravityDelta = new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
            _controller.Move(rootMotionDelta + gravityDelta);
        }

        private bool IsUsingRootMotionAction() // 检查当前是否正在执行使用 Root Motion 的动作
        {
            return (_playerDash != null && _playerDash.IsDashing)
                   || (_playerAttack != null && _playerAttack.IsAttacking);
        }

        private void ApplyGravity()
        {
            if (Grounded)
            {
                // 重置下落计时器
                _fallTimeoutDelta = FallTimeout;

                // 在接地时阻止垂直速度无限向下增加
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                // 下落计时
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                }
            }

            // 如果尚未达到终端速度，则随时间应用重力
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // 选中对象时，在接地检测碰撞体的位置绘制半径一致的 Gizmo
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}
