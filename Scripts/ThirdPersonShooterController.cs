using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using System.IO.Compression;
using Unity.VisualScripting;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] public float lookSensitivity; // Camera sensitivity when looking normally
    [SerializeField] public float aimSensitivity; // Camera sensitivity when aiming
    [SerializeField] public CinemachineVirtualCamera playerZoomCamera;
    //[SerializeField] public GameObject cinemachineCameraTarget;
    [SerializeField] public Transform prefabBulletProjectile;
    [SerializeField] public Transform debugTransform;
    [SerializeField] public Transform spawnBulletPosition;
    [SerializeField] public Transform vfxHitMetal;
    [SerializeField] public Transform vfxHitConcrete;
    [SerializeField] public Transform vfxHitCactus;
    [SerializeField] public Rig rifleAimRig;
    [SerializeField] public Rig pistolAimRig;
    [SerializeField] public Rig macheteAimRig;
    [SerializeField] public LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] public GameObject weaponRifleObject;
    [SerializeField] public GameObject weaponPistolObject;
    [SerializeField] public GameObject weaponMeleeObject;
    [SerializeField] public GameObject weaponHolstered;
    [SerializeField] public GameObject inventory;
    [SerializeField] public GameObject ammoDisplay;
    [SerializeField] public GameObject ammoCounter;
    [SerializeField] public GameObject itemPickupDisplay;
    [SerializeField] public AudioSource metalBulletHit;
    [SerializeField] public AudioSource concreteBulletHit;
    [SerializeField] public AudioSource cactusBulletHit;
    [SerializeField] public AudioSource metalKnifeHit;
    [SerializeField] public AudioSource concreteKnifeHit;
    [SerializeField] public AudioSource cactusKnifeHit;
    [SerializeField] public AudioSource cactusBreak;

    public ThirdPersonController thirdPersonController;
    public StarterAssetsInputs starterAssetsInputs;
    public Animator animator;
    //public CinemachineImpulseSource cinemachineImpulseSource;
    public float aimRigWeight;
    public bool isHipfiring = false;
    public float hipfireTimeout = 0f; // Time left until hipfire times out
    public float hipfireDuration = 1f; // How long hipfire stays on
    public int activeWeaponType = 0;
    public GameObject activeWeapon;
    public int weaponsEquipped = 1;

    #region Melee
    public float attackDistance = 3f;
    public float attackDelay = 2f;
    public float attackSpeed = 3f;

    public bool attacking = false;
    bool readyToAttack = true;
    #endregion

    public bool isReloading = false;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        //cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();

        animator.SetFloat("ActiveWeaponType", activeWeaponType);
        animator.SetBool("Attacking", false);
        animator.SetBool("Reload", false);

        switch (activeWeaponType)
        {
            case 0:
                activeWeapon = null;
                break;
            case 1:
                activeWeapon = weaponRifleObject;
                break;
            case 2:
                activeWeapon = weaponPistolObject;
                break;
            case 3:
                activeWeapon = weaponMeleeObject;
                break;
        }

        playerZoomCamera.gameObject.SetActive(false);
    }

    private void Update() {

        HandleAimWorldPosition();
        HandleWeaponSwitching();
        HandleAiming();
        ShowAmmo();
        HandleAttacking();
        HandleReloading();

        //Aim rig only enables while aiming
        SetAimRig();
    }

    private void HandleAimWorldPosition()
    {
        debugTransform.position = Vector3.Lerp(debugTransform.position, GetAimWorldPosition(), Time.deltaTime * 200f);
    }

    private Vector3 GetAimWorldPosition()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, aimColliderLayerMask)) // Mouse/AimWorldPosition
        {
            //return raycastHit.point;
            return ray.GetPoint(10);
        }
        else
        {
            //return Vector3.zero;
            return ray.GetPoint(10);
        }
    }

    private void HandleAiming()
    {
        if (activeWeapon != null && !inventory.activeInHierarchy && !isReloading)
        {
            if (starterAssetsInputs.aim) // Aim On
            {
                isHipfiring = false; // Disable hipfire when committing to aiming
                hipfireTimeout = 0; // Reset hipfire duration timer
                playerZoomCamera.gameObject.SetActive(true); // Switch to zoomed in camera
                thirdPersonController.SetSensitivity(aimSensitivity); // Switch to ADS sensitivity
                thirdPersonController.SetRotateOnMove(false); // Make player face forwards while aiming
                SetAnimationLayer();
                aimRigWeight = 1;

                Vector3 worldAimTarget = GetAimWorldPosition();
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                // Speed player rotates towards target
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 200f);
            }
            else if (isHipfiring)
            {
                thirdPersonController.SetSensitivity(aimSensitivity);
                thirdPersonController.SetRotateOnMove(false);
                SetAnimationLayer();
                aimRigWeight = 1;

                Vector3 worldAimTarget = GetAimWorldPosition();
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                // Speed player rotates towards target
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 200f);
            }
            else // Aim Off
            {
                playerZoomCamera.gameObject.SetActive(false); // Switch to normal (zoomed out) camera
                thirdPersonController.SetSensitivity(lookSensitivity); // Switch to normal sensitivity
                thirdPersonController.SetRotateOnMove(true); // Allow player to rotate towards walk direction again
                animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); // Switch to base animation layer
                animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0f, Time.deltaTime * 10f));
                animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
                aimRigWeight = 0;
            }
        }
        else
        {
            isHipfiring = false;
            hipfireTimeout = 0;
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
            aimRigWeight = 0;
            thirdPersonController.SetRotateOnMove(true);

            if (starterAssetsInputs.aim) // Aim On
            {
                playerZoomCamera.gameObject.SetActive(true); // Switch to zoomed in camera
                thirdPersonController.SetSensitivity(aimSensitivity); // Switch to ADS sensitivity
            }
            else // Aim Off
            {
                playerZoomCamera.gameObject.SetActive(false); // Switch to normal (zoomed out) camera
                thirdPersonController.SetSensitivity(lookSensitivity); // Switch to normal sensitivity
            }
        }
    }

    private void HandleAttacking()
    {
        if (starterAssetsInputs.attack && activeWeaponType != 0 && !inventory.activeInHierarchy && !isReloading)
        {
            Attack();
        }
        else
        {
            starterAssetsInputs.attack = false;
        }
    }

    private void Attack() // No Projectiles, Yes Hitscan
    {
        var activWep = activeWeapon.GetComponent<WeaponProperties>();
        var currentAmmo = activWep.currentAmmo;
        var attackSound = activWep.attackSound;
        var emptySound = activWep.emptySound;
        attackSound.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        Vector3 targetPosition = GetAimWorldPosition();
        targetPosition.y = transform.position.y;
        Vector3 aimDirection = (targetPosition - transform.position).normalized;

        if (!starterAssetsInputs.aim) // If you attack while not aiming, hipfire
        {
            isHipfiring = true;
            hipfireTimeout += hipfireDuration; // Add unit/multiple of hipfireDuration to time left for hipfire to remain on
            Invoke(nameof(ResetHipfire), hipfireDuration); // Check to turn off hipfire after one unit of hipfireDuration has passed
        }

        if (activeWeaponType == 1 || activeWeaponType == 2 && weaponsEquipped != 0 && currentAmmo > 0) // Rifle or Pistol Shoot
        {
            //animator.SetTrigger("AttackSingle");

            //cinemachineImpulseSource.GenerateImpulse();

            activWep.currentAmmo -= 1;

            activWep.muzzleFlash.SetActive(true);
            Invoke(nameof(ResetMuzzleFlash), 0.05f);

            // Play Sound
            attackSound.PlayOneShot(attackSound.clip);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                // Apply hit force to rigidbody
                if (raycastHit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
                {
                    rigidbody.AddExplosionForce(500f, raycastHit.point + raycastHit.normal * .001f, 5f);
                }

                // Apply damage to damageable
                //if  (raycastHit.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
                //{
                //    damageable.Damage(33, raycastHit.point);
                //}

                var hitTransform = raycastHit.transform;

                if (hitTransform != null)
                {
                    // Hit something
                    if (hitTransform.GetComponent<BulletTarget>() != null)
                    {
                        // Hit target
                        Instantiate(vfxHitMetal, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                        AudioSource.PlayClipAtPoint(metalBulletHit.clip, raycastHit.point + raycastHit.normal * .001f);
                    }
                    else if (hitTransform.GetComponent<CactusScript>() != null)
                    {
                        Instantiate(vfxHitCactus, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                        hitTransform.GetComponent<CactusScript>().hits += 1;
                        AudioSource.PlayClipAtPoint(cactusBulletHit.clip, raycastHit.point + raycastHit.normal * .001f);
                    }
                    else if (hitTransform.GetComponent<TargetMove>() != null)
                    {
                        Instantiate(vfxHitMetal, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                        AudioSource.PlayClipAtPoint(metalBulletHit.clip, raycastHit.point + raycastHit.normal * .001f);
                        hitTransform.GetComponent<TargetMove>().Move();
                    }
                    else
                    {
                        // Hit not target
                        Instantiate(vfxHitConcrete, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                        AudioSource.PlayClipAtPoint(concreteBulletHit.clip, raycastHit.point + raycastHit.normal * .001f);
                    }
                }

            }
        }
        else if (activeWeaponType == 3)// Machete Attack
        {
            if (readyToAttack || !attacking)
            {
                readyToAttack = false;
                attacking = true;

                animator.SetBool("Attacking", true);
                Invoke(nameof(ResetAttackAnim), attackSpeed);
                Invoke(nameof(ResetMeleeAttack), attackSpeed);
                Invoke(nameof(MeleeAttackRaycast), attackDelay);                
            }
        }
        else if (currentAmmo == 0)
        {
            emptySound.PlayOneShot(emptySound.clip);
        }

        // Stop attacking
        starterAssetsInputs.attack = false;
    }

    private void HandleReloading()
    {
        if (starterAssetsInputs.reload && !isReloading && (activeWeaponType == 1 || activeWeaponType == 2))
        {
            Reload();
        }
        else
        {
            starterAssetsInputs.reload = false;
        }
    }

    private void Reload()
    {
        var ammoLoaded = activeWeapon.GetComponent<WeaponProperties>().currentAmmo;
        var ammoReserve = activeWeapon.GetComponent<WeaponProperties>().totalAmmo;
        var magSize = activeWeapon.GetComponent<WeaponProperties>().magSize;
        var reloadSound = activeWeapon.GetComponent<WeaponProperties>().reloadSound;
        var spaceToLoad = magSize - ammoLoaded;
        var ammoToLoad = 0;

        if (spaceToLoad <= ammoReserve)
        {
            ammoToLoad = spaceToLoad;
        }
        else
        {
            if (ammoReserve > 0)
            {
                ammoToLoad = ammoReserve;
            }
        }

        if ((ammoLoaded != magSize) && (ammoReserve > 0) && (ammoToLoad > 0))
        {
            isReloading = true;

            reloadSound.PlayOneShot(reloadSound.clip);

            isHipfiring = false;
            hipfireTimeout = 0;
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
            aimRigWeight = 0;
            thirdPersonController.SetRotateOnMove(true);

            animator.SetBool("Reload", true);

            Invoke(nameof(ResetReload), 2.5f);

            activeWeapon.GetComponent<WeaponProperties>().currentAmmo += ammoToLoad;
            activeWeapon.GetComponent<WeaponProperties>().totalAmmo -= ammoToLoad;
        }
        else
        {

        }

        starterAssetsInputs.reload = false;
    }

    private void ResetHipfire() // Check if hipfireTimeout has reached 0, if it has, turn it off; if not, subtract one unit of hipfireDuration time having passed
    {
        if (hipfireTimeout > 0f)
        {
            hipfireTimeout -= hipfireDuration;
        }

        if (hipfireTimeout == 0f)
        {
            isHipfiring = false;
        }
    }

    void ResetMeleeAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void MeleeAttackRaycast()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        var attackSound = activeWeapon.GetComponent<WeaponProperties>().attackSound;
        attackSound.PlayOneShot(attackSound.clip);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, isHipfiring ? 5.5f : 3f))
        {
            // Apply hit force to rigidbody
            if (raycastHit.collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                rigidbody.AddExplosionForce(500f, raycastHit.point + raycastHit.normal * .001f, 5f);
            }

            // Apply damage to damageable
            //if  (raycastHit.collider.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            //{
            //    damageable.Damage(33, raycastHit.point);
            //}

            var hitTransform = raycastHit.transform;

            if (hitTransform != null)
            {
                // Hit something
                if (hitTransform.GetComponent<BulletTarget>() != null)
                {
                    // Hit target
                    Instantiate(vfxHitMetal, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                    AudioSource.PlayClipAtPoint(metalKnifeHit.clip, raycastHit.point + raycastHit.normal * .001f);
                }
                else if (hitTransform.GetComponent<CactusScript>() != null)
                {
                    Instantiate(vfxHitCactus, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                    hitTransform.GetComponent<CactusScript>().hits += 1;
                    AudioSource.PlayClipAtPoint(cactusKnifeHit.clip, raycastHit.point + raycastHit.normal * .001f);
                }
                else
                {
                    // Hit not target
                    Instantiate(vfxHitConcrete, raycastHit.point + raycastHit.normal * .001f, Quaternion.LookRotation(raycastHit.normal));
                    AudioSource.PlayClipAtPoint(concreteKnifeHit.clip, raycastHit.point + raycastHit.normal * .001f);
                }
            }

        }
    }

    private void HandleWeaponSwitching()
    {
        if (!starterAssetsInputs.aim && !attacking && !isReloading)
        {
            // Primary (rifle) ready
            if (starterAssetsInputs.switchToRifle && activeWeaponType != 1 && weaponRifleObject.GetComponent<WeaponProperties>().isPickedUp)
            {
                activeWeaponType = 1;
                activeWeapon = weaponRifleObject;
                weaponRifleObject.SetActive(true);
                weaponPistolObject.SetActive(false);
                weaponMeleeObject.SetActive(false);
                weaponHolstered.SetActive(false);
                starterAssetsInputs.switchToRifle = false; // Reset input
            }
            else
            {
                starterAssetsInputs.switchToRifle = false;
            }

            // Secondary (pistol) ready
            if (starterAssetsInputs.switchToPistol && activeWeaponType != 2 && weaponPistolObject.GetComponent<WeaponProperties>().isPickedUp)
            {
                activeWeaponType = 2;
                activeWeapon = weaponPistolObject;
                weaponRifleObject.SetActive(false);
                weaponPistolObject.SetActive(true);
                weaponMeleeObject.SetActive(false);
                weaponHolstered.SetActive(false);
                starterAssetsInputs.switchToPistol = false; // Reset input
            }
            else
            {
                starterAssetsInputs.switchToPistol = false;
            }

            // Melee (machete) ready
            if (starterAssetsInputs.switchToMelee && activeWeaponType != 3 && weaponMeleeObject.GetComponent<WeaponProperties>().isPickedUp)
            {
                activeWeaponType = 3;
                activeWeapon = weaponMeleeObject;
                weaponRifleObject.SetActive(false);
                weaponPistolObject.SetActive(false);
                weaponMeleeObject.SetActive(true);
                weaponHolstered.SetActive(false);
                starterAssetsInputs.switchToMelee = false; // Reset input
            }
            else
            {
                starterAssetsInputs.switchToMelee = false;
            }

            if (starterAssetsInputs.holsterWeapon && activeWeaponType != 0)
            {
                isHipfiring = false;
                hipfireTimeout = 0;
                activeWeaponType = 0;
                activeWeapon = null;
                weaponRifleObject.SetActive(false);
                weaponPistolObject.SetActive(false);
                weaponMeleeObject.SetActive(false);
                weaponHolstered.SetActive(true);
                starterAssetsInputs.holsterWeapon = false; // Reset input
            }
            else
            {
                starterAssetsInputs.holsterWeapon = false;
            }
        }

        starterAssetsInputs.switchToRifle = false;
        starterAssetsInputs.switchToPistol = false;
        starterAssetsInputs.switchToMelee = false;
        starterAssetsInputs.holsterWeapon = false;

        animator.SetFloat("ActiveWeaponType", activeWeaponType);
    }

    public void SwitchOnPickup(int weaponType)
    {
        activeWeaponType = weaponType;

        if (!starterAssetsInputs.aim)
        {
            if (weaponType == 1)
            {
                activeWeapon = weaponRifleObject;
                weaponRifleObject.SetActive(true);
                weaponPistolObject.SetActive(false);
                weaponMeleeObject.SetActive(false);
            }
            else if (weaponType == 2)
            {
                activeWeapon = weaponPistolObject;
                weaponRifleObject.SetActive(false);
                weaponPistolObject.SetActive(true);
                weaponMeleeObject.SetActive(false);
            }
            else if (weaponType == 3)
            {
                activeWeapon = weaponMeleeObject;
                weaponRifleObject.SetActive(false);
                weaponPistolObject.SetActive(false);
                weaponMeleeObject.SetActive(true);
            }
        }

        starterAssetsInputs.switchToRifle = false;
        starterAssetsInputs.switchToPistol = false;
        starterAssetsInputs.switchToMelee = false;
        starterAssetsInputs.holsterWeapon = false;
    }

    public void SetAnimationLayer()
    {
        if (activeWeaponType == 1)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
        }
        else if (activeWeaponType == 2)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 1f, Time.deltaTime * 10f));
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
        }
        else if (activeWeaponType == 3)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0f, Time.deltaTime * 10f));
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 1f, Time.deltaTime * 10f));
        }
    }

    public void SetAimRig()
    {
        if (activeWeaponType == 1)
        {
            rifleAimRig.weight = Mathf.Lerp(rifleAimRig.weight, aimRigWeight, Time.deltaTime * 20f);
        }
        else if (activeWeaponType == 2)
        {
            pistolAimRig.weight = Mathf.Lerp(pistolAimRig.weight, aimRigWeight, Time.deltaTime * 20f);
        }
        else if (activeWeaponType == 3)
        {
            macheteAimRig.weight = Mathf.Lerp(macheteAimRig.weight, aimRigWeight, Time.deltaTime * 20f);
        }
        else
        {
            pistolAimRig.weight = Mathf.Lerp(pistolAimRig.weight, 0, Time.deltaTime * 20f);
            rifleAimRig.weight = Mathf.Lerp(rifleAimRig.weight, 0, Time.deltaTime * 20f);
            macheteAimRig.weight = Mathf.Lerp(rifleAimRig.weight, 0, Time.deltaTime * 20f);
        }
    }

    public void ShowAmmo()
    {
        if(activeWeaponType == 1 || activeWeaponType == 2)
        {
            ammoDisplay.SetActive(true);
            var ammoLoaded = activeWeapon.GetComponent<WeaponProperties>().currentAmmo;
            var totalAmmo = activeWeapon.GetComponent<WeaponProperties>().totalAmmo;
            ammoCounter.GetComponent<Text>().text = ammoLoaded.ToString() + "/" + totalAmmo.ToString();
        }
        else
        {
            ammoDisplay.SetActive(false);
        }
    }

    public void ResetReload()
    {
        animator.SetBool("Reload", false);
        isReloading = false;
    }

    public void ResetAttackAnim()
    {
        animator.SetBool("Attacking", false);
    }

    public void ResetMuzzleFlash()
    {
        activeWeapon.GetComponent<WeaponProperties>().muzzleFlash.SetActive(false);
    }


}
