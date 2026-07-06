using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private PlayerDash _playerDash;

    private readonly int _isDashingHash = Animator.StringToHash("IsDashing"); //转为哈希 节省开销同时只读

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerDash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        UpdateDashAnimation();
    }

    private void UpdateDashAnimation()
    {
        if (_playerDash == null || _animator == null)
        {
            return;
        }

        _animator.SetBool(_isDashingHash, _playerDash.IsDashing);
    }
}
