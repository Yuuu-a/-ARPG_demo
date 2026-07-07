using StarterAssets;
using UnityEngine;

/// <summary>
/// 角色动画参数统一出口。
/// 移动、Dash、攻击脚本只负责玩法逻辑，这个脚本负责把逻辑状态写入 Animator。
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private PlayerAttack playerAttack;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int CanComboHash = Animator.StringToHash("CanCombo");

    private void Reset()
    {
        // Reset 只在编辑器中辅助填充，正式引用仍建议在 Inspector 手动确认。
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        playerDash = GetComponent<PlayerDash>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        UpdateMovementParameters();
        UpdateDashParameters();
        UpdateAttackParameters();
    }

    private void UpdateMovementParameters()
    {
        if (thirdPersonController == null)
        {
            return;
        }

        // 保持 Starter Assets 原本使用的移动参数。
        animator.SetFloat(SpeedHash, thirdPersonController.AnimationBlend);
        animator.SetFloat(MotionSpeedHash, thirdPersonController.MotionSpeed);
        animator.SetBool(GroundedHash, thirdPersonController.Grounded);
    }

    private void UpdateDashParameters()
    {
        if (playerDash == null)
        {
            animator.SetBool(IsDashingHash, false);
            return;
        }

        animator.SetBool(IsDashingHash, playerDash.IsDashing);

        // Dash Trigger 是一次性信号，由 Dash 逻辑产生，由动画控制器消费。
        if (playerDash.ConsumeDashTrigger())
        {
            animator.ResetTrigger(DashHash);
            animator.SetTrigger(DashHash);
        }
    }

    private void UpdateAttackParameters()
    {
        if (playerAttack == null)
        {
            animator.SetBool(IsAttackingHash, false);
            animator.SetBool(CanComboHash, false);
            animator.SetInteger(ComboIndexHash, 0);
            return;
        }

        animator.SetBool(IsAttackingHash, playerAttack.IsAttacking);
        animator.SetBool(CanComboHash, playerAttack.CanCombo);
        animator.SetInteger(ComboIndexHash, playerAttack.CurrentComboIndex);

        // Attack Trigger 是一次性信号，由攻击逻辑产生，由动画控制器消费。
        if (playerAttack.ConsumeAttackTrigger())
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(AttackHash);
        }
    }
}
