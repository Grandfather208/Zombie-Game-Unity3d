﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Invector.vShooter
{
    [vClassHeader("Shooter Weapon", openClose = false)]
    public class vShooterWeapon : vMonoBehaviour
    {
        #region variables

        [vEditorToolbar("Weapon Settings")]

        public bool isLeftWeapon = false;
        [Tooltip("Hold Charge Input to charge")]
        public bool chargeWeapon = false;
        [vHideInInspector("chargeWeapon")]
        public bool autoShotOnFinishCharge = false;
        [vHideInInspector("chargeWeapon")]
        public float chargeSpeed = 0.1f;
        [vHideInInspector("chargeWeapon")]
        public float chargeDamageMultiplier = 2;
        [vHideInInspector("chargeWeapon")]
        public bool changeVelocityByCharge = true;
        [vHideInInspector("chargeWeapon")]
        public float chargeVelocityMultiplier = 2;
        [Tooltip("Change between automatic weapon or shot once")]
        [vHideInInspector("chargeWeapon", true)]
        public bool automaticWeapon;
        [Tooltip("Frequency of shots")]
        public float shootFrequency;

        [vEditorToolbar("Ammo")]

        [Tooltip("Max clip size of your weapon")]
        public int clipSize;
        [Tooltip("Starting ammo")]
        [SerializeField]
        protected int ammo;
        [Tooltip("Ammo ID - make sure your AmmoManager and ItemListData use the same ID")]
        public int ammoID;

        [vEditorToolbar("Weapon ID")]
        [Tooltip("What moveset the underbody will play")]
        public float moveSetID;
        [Tooltip("What moveset the uperbody will play")]
        public float upperBodyID;
        [Tooltip("What shot animation will trigger")]
        public float attackID;
        [vHideInInspector("autoReload", true)]
        [Tooltip("What reload animation will play")]
        public int reloadID;
        [Tooltip("Automatically reload the weapon when it's empty")]
        public bool autoReload;
        [Tooltip("What equip animation will play")]
        public int equipID;

        [vEditorToolbar("IK Options")]
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightHandToAim = true;
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightUpperArmToAim = true;
        public bool raycastAimTarget = true;
        [Tooltip("Left IK on Idle")]
        public bool useIkOnIdle = true;
        [Tooltip("Left IK on free locomotion")]
        public bool useIkOnFree = true;
        [Tooltip("Left IK on strafe locomotion")]
        public bool useIkOnStrafe = true;
        [Tooltip("Left IK while attacking")]
        public bool useIkAttacking = false;
        public bool disableIkOnShot = false;
        public bool useIKOnAiming = true;
        public bool canAimWithoutAmmo = true;

        [Tooltip("Use this offset to better align your spine with the aim animation")]
        public Vector2 headTrackOffset;
        [Tooltip("Use this offset to better align your spine with the aim animation")]
        public Vector2 headTrackOffsetCrouch;
        [Tooltip("Left IK of the weapon")]
        public Transform handIKTarget;

        [vEditorToolbar("Projectile")]
        public vShooterWeapon secundaryWeapon;
        [Tooltip("Prefab of the projectile")]
        public GameObject projectile;
        [Tooltip("Assign the projectile you want to hide, for example an RPG with a missile")]
        public GameObject projectileToHide;
        [Tooltip("Delay to active the hidden projectile to syinc with the reload animation")]
        public float projectileToHideDelay;
        [Tooltip("Assign the muzzle of your weapon")]
        public Transform muzzle;
        [Tooltip("Assign the aimReference of your weapon")]
        public Transform aimReference;
        [Tooltip("How many projectiles will spawn per shot")]
        [Range(1, 20)]
        public int projectilesPerShot = 1;
        [Range(0, 90)]
        [Tooltip("how much dispersion the weapon have")]
        public float dispersion = 0;
        [Tooltip("how much precision the weapon have, 1 means no cameraSway and 0 means maxCameraSway from the ShooterManager")]
        [Range(0, 1)]
        public float precision = 0.5f;
        [Range(0, 1000)]
        [Tooltip("Velocity of your projectile")]
        public float velocity = 380;
        [Tooltip("Use the DropOffStart and DropOffEnd to calc damage by distance using min and max damage")]
        public bool damageByDistance = true;
        [Tooltip("Min distance to apply damage")]
        public float DropOffStart = 8f;
        [Tooltip("Max distance to apply damage")]
        public float DropOffEnd = 50f;
        [Tooltip("Minimum damage caused by the shot, regardless the distance")]
        public int minDamage;
        [Tooltip("Maximum damage caused by the close shot")]
        public int maxDamage;
        [Tooltip("Creates a right recoil on the camera")]
        public float recoilRight = 1;
        [Tooltip("Creates a left recoil on the camera")]
        public float recoilLeft = -1;
        [Tooltip("Creates a up recoil on the camera")]
        public float recoilUp = 1;

        [vEditorToolbar("Audio & VFX")]
        [Header("Audio")]
        public AudioClip fireClip;
        public AudioClip emptyClip;
        public AudioClip reloadClip;
        public AudioSource source;

        [Header("Effects")]
        public bool testShootEffect;
        public Light lightOnShot;
        [SerializeField]
        public ParticleSystem[] emittShurykenParticle;

        [vEditorToolbar("Scope UI")]
        [vHelpBox("Third Person Controller Only",vHelpBoxAttribute.MessageType.Info)]
        [Tooltip("Check this bool to use an UI image for the scope, ex: snipers")]
        public bool useUI;
        [Tooltip("You can create different Aim sprites and use for different weapons")]
        public int scopeID;
        [Tooltip("change the FOV of the scope view\n **The calc is default value (60)-scopeZoom**"), Range(-118, 60)]
        public float scopeZoom = 60;

        [Tooltip("assign an empty transform with the pos/rot of your scope view")]
        public Transform scopeTarget;

        public Camera zoomScopeCamera;
        [vHelpBox("Keep Scope Camera Z is used to align z rotation of the zoomScopeCamera to z rotation of the weapon muzzle<color=red> (Projectile toolbar)</color>. if you want to align camera with Vector3.up in z rotation enable this.")]
        public bool keepScopeCameraRotationZ = true;
        protected Transform sender;

        [HideInInspector]
        public OnDestroyEvent onDestroy;
        [System.Serializable]
        public class OnDestroyEvent : UnityEvent<GameObject> { }
        [System.Serializable]
        public class OnInstantiateProjectile : UnityEvent<vProjectileControl> { }
        [System.Serializable]
        public class OnChangePowerCharger : UnityEvent<float> { }
        [HideInInspector]
        public bool isAiming, usingScope;

        [vEditorToolbar("Events")]
        public UnityEvent onShot, onReload, onEmptyClip, onEnableAim, onDisableAim, onEnableScope, onDisableScope, onFullPower;
        public OnChangePowerCharger onChangerPowerCharger;
        public OnInstantiateProjectile onInstantiateProjectile;
        [HideInInspector]
        public List<string> ignoreTags = new List<string>();
        [HideInInspector]
        public LayerMask hitLayer;
        [HideInInspector]
        public Transform root;
        [HideInInspector]
        public bool isSecundaryWeapon;
        private float _charge;
        public delegate bool CheckAmmoHandle(ref bool isValid, ref int totalAmmo);
        public delegate void ChangeAmmoHandle(int value);
        public CheckAmmoHandle checkAmmoHandle;
        public ChangeAmmoHandle changeAmmoHandle;
        #endregion

        [System.NonSerialized] private float testTime;

        void OnDrawGizmos()
        {
            if (!Application.isPlaying && testShootEffect)
            {
                if (testTime <= 0)
                    Shootest();
                else testTime -= Time.deltaTime;
            }
        }

        private void Start()
        {
            SetScopeZoom(scopeZoom);
        }

        void Shootest()
        {
            testTime = shootFrequency;

            StartEmitters();
            lightOnShot.enabled = true;
            source.PlayOneShot(fireClip);
            Invoke("StopShootTest", .037f);
        }

        void StopShootTest()
        {
            StopEmitters();
            lightOnShot.enabled = false;
        }

        public float powerCharge
        {
            get
            {
                return _charge;
            }
            set
            {
                if (value != _charge)
                {
                    _charge = value;
                    onChangerPowerCharger.Invoke(_charge);
                    if (_charge >= 1) onFullPower.Invoke();
                }
            }
        }

        public void SetPrecision(float value)
        {
            precision = Mathf.Clamp(value, 0, 1);
        }

        public void StartEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                    pe.Emit(1);
            }
        }

        void StopEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                    pe.Stop();
            }
        }

        public virtual string weaponName
        {
            get
            {
                var value = gameObject.name.Replace("(Clone)", string.Empty);
                return value;
            }
        }

        public void OnDestroy()
        {
            onDestroy.Invoke(gameObject);
        }

        public virtual void ShootEffect(Transform sender = null)
        {
            this.sender = sender != null ? sender : transform;
            HandleShotEffect(muzzle.position + muzzle.forward * 100f);
        }

        public virtual void ShootEffect(Vector3 aimPosition, Transform sender = null)
        {
            if (checkAmmoHandle != null && changeAmmoHandle != null)
            {

                bool isValidAmmo = false;
                int totalAmmo = 0;
                checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo)
                    changeAmmoHandle(-1);
                else
                    ammo--;
            }
            else
                ammo--;
            this.sender = sender != null ? sender : transform;
            HandleShotEffect(aimPosition);
        }

        public bool HasAmmo()
        {
            if (checkAmmoHandle != null)
            {
                bool isValidAmmo = false;
                int totalAmmo = 0;
                var hasAmmo = checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo) return hasAmmo;
                else return ammo > 0;
            }
            else
                return ammo > 0;
        }

        public int ammoCount
        {
            get
            {
                if (checkAmmoHandle != null)
                {
                    bool isValidAmmo = false;
                    int totalAmmo = 0;
                    checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                    if (isValidAmmo) return totalAmmo;
                    else return ammo;
                }
                return ammo;
            }
        }

        public void AddAmmo(int value)
        {
            if (checkAmmoHandle != null && changeAmmoHandle != null)
            {

                bool isValidAmmo = false;
                int totalAmmo = 0;
                checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo)
                    changeAmmoHandle(value);
                else
                    ammo += value;
            }
            else
                ammo += value;
        }

        public virtual void ReloadEffect()
        {
            if (source)
            {
                source.Stop();
                source.PlayOneShot(reloadClip);
            }

            if (projectileToHide != null)
                StartCoroutine(ActiveHiddenProjectile(projectileToHideDelay));
            onReload.Invoke();
        }

        public virtual void EmptyClipEffect()
        {
            if (source)
            {
                source.Stop();
                source.PlayOneShot(emptyClip);
            }

            onEmptyClip.Invoke();
        }

        public virtual void StopSound()
        {
            source.Stop();
        }

        protected virtual IEnumerator ActiveHiddenProjectile(float time)
        {
            yield return new WaitForSeconds(time);
            projectileToHide.gameObject.SetActive(true);
        }

        protected virtual IEnumerator LightOnShoot(float time)
        {
            if (lightOnShot)
            {
                lightOnShot.enabled = true;

                yield return new WaitForSeconds(time);
                lightOnShot.enabled = false;
            }

        }

        protected virtual Vector3 Dispersion(Vector3 aim, float distance, float variance)
        {
            aim.Normalize();
            Vector3 v3 = Vector3.zero;
            do
            {
                v3 = Random.insideUnitSphere;
            }
            while (v3 == aim || v3 == -aim);
            v3 = Vector3.Cross(aim, v3);
            v3 = v3 * Random.Range(0f, variance);
            return aim * distance + v3;
        }

        protected virtual void HandleShotEffect(Vector3 aimPosition)
        {
            onShot.Invoke();

            StopCoroutine("LightOnShoot");
            if (source)
            {
                source.Stop();
                source.PlayOneShot(fireClip);
            }

            StartCoroutine("LightOnShoot", 0.037f);
            StartEmitters();

            if (projectileToHide != null)
                projectileToHide.gameObject.SetActive(false);
            var dir = aimPosition - muzzle.position;
            var rotation = Quaternion.LookRotation(dir);


            if (dispersion > 0 && projectile)
            {
                for (int i = 0; i < projectilesPerShot; i++)
                {
                    var spreadRotation = Quaternion.LookRotation(Dispersion(dir.normalized, DropOffEnd, dispersion));
                    var obj = Instantiate(projectile, muzzle.transform.position, spreadRotation) as GameObject;

                    var pCtrl = obj.GetComponent<vProjectileControl>();

                    pCtrl.shooterTransform = root;
                    pCtrl.ignoreTags = ignoreTags;
                    pCtrl.hitLayer = hitLayer;
                    pCtrl.damage.sender = sender;
                    pCtrl.startPosition = obj.transform.position;
                    pCtrl.maxDamage = (maxDamage / projectilesPerShot) * damageMultiplier;
                    pCtrl.minDamage = (minDamage / projectilesPerShot) * damageMultiplier;
                    pCtrl.DropOffStart = DropOffStart;
                    pCtrl.DropOffEnd = DropOffEnd;
                    onInstantiateProjectile.Invoke(pCtrl);
                    StartCoroutine(ShootBullet(obj, spreadRotation * Vector3.forward));
                }
            }
            else if (projectilesPerShot > 0 && projectile)
            {
                var obj = Instantiate(projectile, muzzle.transform.position, rotation) as GameObject;
                var pCtrl = obj.GetComponent<vProjectileControl>();
                pCtrl.shooterTransform = root;
                pCtrl.ignoreTags = ignoreTags;
                pCtrl.hitLayer = hitLayer;
                pCtrl.damage.sender = sender;
                pCtrl.startPosition = obj.transform.position;
                pCtrl.maxDamage = (maxDamage / projectilesPerShot) * damageMultiplier;
                pCtrl.minDamage = (minDamage / projectilesPerShot) * damageMultiplier;
                pCtrl.DropOffStart = DropOffStart;
                pCtrl.DropOffEnd = DropOffEnd;
                onInstantiateProjectile.Invoke(pCtrl);
                StartCoroutine(ShootBullet(obj, dir));
            }
        }

        protected int damageMultiplier
        {
            get
            {
                if (!chargeWeapon) return 1;
                return (int)(1 + Mathf.Lerp(0, chargeDamageMultiplier, _charge));
            }
        }

        public float velocityMultiplier
        {
            get
            {
                if (!chargeWeapon || !changeVelocityByCharge) return 1;
                return (1 + Mathf.Lerp(0, chargeVelocityMultiplier, _charge));
            }
        }

        protected virtual IEnumerator ShootBullet(GameObject bullet, Vector3 dir)
        {
            var velocityChanged = velocity * velocityMultiplier;
            yield return new WaitForSeconds(0.01f);
            try
            {
                var _rigidbody = bullet.GetComponent<Rigidbody>();
                _rigidbody.mass = _rigidbody.mass / projectilesPerShot;//Change mass per projectiles count.

                _rigidbody.AddForce((dir.normalized * velocityChanged), ForceMode.VelocityChange);
            }
            catch
            {

            }
        }

        public void SetScopeZoom(float value)
        {
            if (zoomScopeCamera)
            {
                var zoom = Mathf.Clamp(61 - value, 1, 179);
                zoomScopeCamera.fieldOfView = zoom;
            }
        }

        public void SetActiveAim(bool value)
        {

            if (isAiming != value)
            {
                isAiming = value;
                if (isAiming)
                    onEnableAim.Invoke();
                else
                    onDisableAim.Invoke();
            }
        }

        /// <summary>
        /// Set if Weapon is using scope
        /// </summary>
        /// <param name="value"></param>
        public void SetActiveScope(bool value)
        {
            if (usingScope != value)
            {
                usingScope = value;
                if (usingScope)
                    onEnableScope.Invoke();
                else
                    onDisableScope.Invoke();
            }
        }

        /// <summary>
        /// Set look target point to Zoom scope camera
        /// </summary>
        /// <param name="point"></param>
        public void SetScopeLookTarget(Vector3 point)
        {
            if (zoomScopeCamera)
            {
                var euler = Quaternion.LookRotation(point - zoomScopeCamera.transform.position, Vector3.up).eulerAngles;
                if (keepScopeCameraRotationZ) euler.z = muzzle.transform.eulerAngles.z;

                zoomScopeCamera.transform.eulerAngles = euler;
            }
        }
    }
}
