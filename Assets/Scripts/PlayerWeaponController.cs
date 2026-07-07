using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Weapon")]
    [SerializeField] private Transform weapon;
    [SerializeField] private Transform weaponHipSocket;
    [SerializeField] private Transform weaponLeftHandSocket;
    [SerializeField] private Transform weaponRightHandSocket;

    [Header("Rig")]
    [SerializeField] private Rig rightHandRig;

    private readonly int _rightHandIKWeightHash = Animator.StringToHash("RightHandIKWeight");

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
