using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Weapon")]
    [SerializeField] private Transform weapon;
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private Transform weaponHipSocket;
    [SerializeField] private Transform weaponLeftHandSocket;
    [SerializeField] private Transform weaponRightHandSocket;

    [Header("Rig")]
    [SerializeField] private Rig rightHandRig;

    [Header("Debug")]
    [SerializeField] private bool logWeaponHitWindows;

    private readonly int _rightHandIKWeightHash = Animator.StringToHash("RightHandIKWeight");

    private void Awake()
    {
        if (weaponHitbox == null && weapon != null)
        {
            weaponHitbox = weapon.GetComponentInChildren<WeaponHitbox>();
        }
    }

    private void LateUpdate()
    {
        if (animator == null)
        {
            return;
        }

        if (rightHandRig != null)
        {
            rightHandRig.weight = animator.GetFloat(_rightHandIKWeightHash);
        }
    }

    public void AttachWeaponToHand()
    {
        // Keep old animation event name. It currently means attach to right hand.
        AttachWeaponToRightHand();
    }

    public void AttachWeaponToLeftHand()
    {
        AttachWeapon(weaponLeftHandSocket);
    }

    public void AttachWeaponToRightHand()
    {
        AttachWeapon(weaponRightHandSocket);
    }

    public void AttachWeaponToHip()
    {
        AttachWeapon(weaponHipSocket);
    }

    public void BeginWeaponHit()
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning($"{nameof(PlayerWeaponController)} on {name} cannot begin weapon hit: weaponHitbox is missing.", this);
            return;
        }

        if (logWeaponHitWindows)
        {
            Debug.Log($"{name} BeginWeaponHit", this);
        }

        weaponHitbox.BeginHitWindow();
    }

    public void BeginWeaponHit(int damage)
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning($"{nameof(PlayerWeaponController)} on {name} cannot begin weapon hit: weaponHitbox is missing.", this);
            return;
        }

        if (logWeaponHitWindows)
        {
            Debug.Log($"{name} BeginWeaponHit. Damage: {damage}", this);
        }

        weaponHitbox.BeginHitWindow(damage);
    }

    public void EndWeaponHit()
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning($"{nameof(PlayerWeaponController)} on {name} cannot end weapon hit: weaponHitbox is missing.", this);
            return;
        }

        if (logWeaponHitWindows)
        {
            Debug.Log($"{name} EndWeaponHit", this);
        }

        weaponHitbox.EndHitWindow();
    }

    private void AttachWeapon(Transform socket)
    {
        if (weapon == null || socket == null)
        {
            return;
        }

        weapon.SetParent(socket);
        weapon.localPosition = Vector3.zero;
        weapon.localRotation = Quaternion.identity;
        weapon.localScale = Vector3.one;
    }
}
