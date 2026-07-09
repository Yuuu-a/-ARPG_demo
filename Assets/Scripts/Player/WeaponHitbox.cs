using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponHitbox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private GameObject attacker;

    [Header("Damage")]
    [SerializeField] private int defaultDamage = 10;
    [SerializeField] private int[] comboDamages = { 10, 12, 15, 20 };
    [SerializeField] private LayerMask targetLayers = ~0;
    [SerializeField] private bool logHits;

    private readonly HashSet<EnemyDamageReceiver> _hitTargets = new();
    private Collider _hitboxCollider;
    private Rigidbody _rigidbody;
    private bool _isActive;
    private int _currentDamage;
    private int _currentComboIndex;

    private void Awake()
    {
        EnsureReferences();

        _hitboxCollider = GetComponent<Collider>();
        _hitboxCollider.isTrigger = true;
        _hitboxCollider.enabled = false;

        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
    }

    private void Reset()
    {
        Collider hitbox = GetComponent<Collider>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;

        Rigidbody hitboxRigidbody = GetComponent<Rigidbody>();
        hitboxRigidbody.isKinematic = true;
        hitboxRigidbody.useGravity = false;

        EnsureReferences();
    }

    public void BeginHitWindow()
    {
        int comboIndex = playerAttack != null ? playerAttack.CurrentComboIndex : 0;
        BeginHitWindow(GetDamageForCombo(comboIndex), comboIndex);
    }

    public void BeginHitWindow(int damage)
    {
        int comboIndex = playerAttack != null ? playerAttack.CurrentComboIndex : 0;
        BeginHitWindow(damage, comboIndex);
    }

    public void EndHitWindow()
    {
        _isActive = false;

        if (_hitboxCollider != null)
        {
            _hitboxCollider.enabled = false;
        }
    }

    private void BeginHitWindow(int damage, int comboIndex)
    {
        _hitTargets.Clear();
        _currentDamage = Mathf.Max(0, damage);
        _currentComboIndex = comboIndex;
        _isActive = true;

        if (_hitboxCollider != null)
        {
            _hitboxCollider.enabled = true;
        }
    }

    private int GetDamageForCombo(int comboIndex)
    {
        int arrayIndex = comboIndex - 1;
        if (comboDamages != null && arrayIndex >= 0 && arrayIndex < comboDamages.Length)
        {
            return comboDamages[arrayIndex];
        }

        return defaultDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        EnsureReferences();

        if (!_isActive || !IsInTargetLayer(other.gameObject.layer))
        {
            return;
        }

        EnemyDamageReceiver damageReceiver = other.GetComponentInParent<EnemyDamageReceiver>();
        if (damageReceiver == null || !damageReceiver.CanReceiveDamage || _hitTargets.Contains(damageReceiver))
        {
            return;
        }

        _hitTargets.Add(damageReceiver);
        HitInfo hitInfo = CreateHitInfo(other);

        if (logHits)
        {
            Debug.Log($"{name} hit {damageReceiver.name}. Damage: {_currentDamage}, Combo: {_currentComboIndex}", this);
        }

        damageReceiver.ReceiveHit(hitInfo);
    }

    private HitInfo CreateHitInfo(Collider other)
    {
        Vector3 closestPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = other.transform.position - (attacker != null ? attacker.transform.position : transform.position);
        hitDirection.y = 0f;

        if (hitDirection.sqrMagnitude > 0.001f)
        {
            hitDirection.Normalize();
        }
        else
        {
            hitDirection = transform.forward;
        }

        return new HitInfo
        {
            Damage = _currentDamage,
            HitPoint = closestPoint,
            HitDirection = hitDirection,
            Attacker = attacker,
            ComboIndex = _currentComboIndex
        };
    }

    private bool IsInTargetLayer(int layer)
    {
        return (targetLayers.value & (1 << layer)) != 0;
    }

    private void EnsureReferences()
    {
        if (playerAttack == null)
        {
            playerAttack = GetComponentInParent<PlayerAttack>();
        }

        if (attacker == null)
        {
            attacker = playerAttack != null ? playerAttack.gameObject : transform.root.gameObject;
        }
    }
}
