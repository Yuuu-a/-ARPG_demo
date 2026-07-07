using System;
using UnityEngine;

/// <summary>
/// 旧脚本兼容占位。
/// 新的动画参数统一控制逻辑请使用 PlayerAnimationController。
/// </summary>
[Obsolete("请使用 PlayerAnimationController。这个脚本只用于兼容旧引用，不再控制 Animator 参数。")]
public class PlayerAnimator : MonoBehaviour
{
}
