﻿
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI
{
    [vClassHeader("AI Combat Controller", iconName = "AI-icon")]
    public class vControlAICombat : vControlAI, vIControlAICombat, vEventSystems.vIAttackListener
    {
        [vEditorToolbar("Combat Settings", order = 9)]
        [Header("Attack Settings")]
        [SerializeField] protected float _mintAttackTime = 0.5f;
        [SerializeField] protected float _maxAttackTime = 2f;
        [SerializeField] protected int _minAttackCount = 1;
        [SerializeField] protected int _maxAttackCount = 3;
        [SerializeField] protected float _attackDistance = 1f;
        [Header("Blocking Settings")]
        [Range(0f,100f)]
        [SerializeField] protected float _combatBlockingChance = 50;
        [Range(0f, 100f)]
        [SerializeField] protected float _onDamageBlockingChance = 25;
        [SerializeField] protected float _minStayBlockingTime = 4;
        [SerializeField] protected float _maxStayBlockingTime = 6;
        [SerializeField] protected float _minTimeToTryBlock = 4;
        [SerializeField] protected float _maxTimeToTryBlock = 6;
        [vHelpBox("Damage type that can block")]
        [SerializeField] protected List<string> ignoreDefenseDamageTypes = new List<string>() { "unarmed","melee" } ;
        [Header("Combat Movement")]
        [SerializeField] protected float _minDistanceOfTheTarget = 2;
        [SerializeField] protected float _combatDistance = 4f;
        [SerializeField] protected bool _strafeCombatMovement = true;
        [vHideInInspector("_strafeCombatMovement")]
        [SerializeField, Tooltip("This control random Strafe Combate Movement side, if True the side is ever -1 or 1 else side can be set to zero (0)")]
        protected bool _alwaysStrafe = false;
        [vHideInInspector("_strafeCombatMovement")]
        [SerializeField]
        protected float _minTimeToChangeStrafeSide = 1f, _maxTimeToChangeStrafeSide = 4f;
        [SerializeField, vReadOnly]
        protected bool _isInCombat;
        protected bool _isBlocking;
        private float _attackTime;
        private float _blockingTime;
        private float _tryBlockTime;
        private float _timeToChangeStrafeSide;

        private int isBlockingHash = Animator.StringToHash("IsBlocking");

        protected override void Start()
        {
            base.Start();
            strafeCombatSide = 1;
        }

        protected override void UpdateLockMovement()
        {
            if (isAttacking)
            {
                lockMovement = true;
            }
            else base.UpdateLockMovement();
        }

        protected override void HandleTarget()
        {
            base.HandleTarget();
            UpdateStrafeCombateMovementSide();
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            UpdateCombatAnimator();
        }

        protected virtual void UpdateCombatAnimator()
        {
            if (_isBlocking && Time.time > _blockingTime || customAction)
            {
                _tryBlockTime = Random.Range(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
                _isBlocking = false;
                if (isBlockingHash != -1) animator.SetBool(isBlockingHash, _isBlocking);
            }
        }

        public override void FindTarget()
        {
            if (ragdolled) return;
            if (currentTarget.transform && targetDistance <= combatRange && _targetIsLost || isAttacking)
            {
                if (updateFindTargetTime > Time.time) return;
                updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(findTargetUpdateQuality);
                return;
            }
            base.FindTarget();
        }
      
        public virtual void UpdateStrafeCombateMovementSide()
        {
            if (strafeCombatMovement)
            {
                if (_timeToChangeStrafeSide <= 0)
                {
                    var randomValue = Random.Range(0, 100);
                    if (_alwaysStrafe)
                    {
                        if (randomValue > 50)
                        {
                            strafeCombatSide = 1;
                        }
                        else strafeCombatSide = -1;
                    }
                    else
                    {
                        if (randomValue >= 70)
                        {
                            strafeCombatSide = 1;
                        }
                        else if (randomValue <= 30) strafeCombatSide = -1;
                        else strafeCombatSide = 0;
                    }
                    _timeToChangeStrafeSide = Random.Range(_minTimeToChangeStrafeSide, _maxTimeToChangeStrafeSide);
                }
                else
                    _timeToChangeStrafeSide -= Time.deltaTime;
            }
        }

        public virtual float combatRange { get { return _combatDistance; } }

        public virtual int strafeCombatSide { get; set; }

        public virtual bool strafeCombatMovement { get { return _strafeCombatMovement; } }
     
        public virtual bool isInCombat { get { return _isInCombat; } set { _isInCombat = value; } }
       
        public virtual float minDistanceOfTheTarget { get { return _minDistanceOfTheTarget; } }

        public virtual float attackDistance { get { return _attackDistance; } }

        public virtual int attackCount { get; set; }

        public virtual bool canAttack
        {
            get
            {
                return (_attackTime < Time.time && !ragdolled) && attackCount > 0;
            }
        }

        public virtual bool isAttacking { get { return animatorStateInfos.HasTag("Attack"); } }      

        public virtual void Attack(bool strongAttack = false, int attackID = -1)
        {
            if (canAttack)
            {                
                animator.SetTrigger(strongAttack ? "StrongAttack" : "WeakAttack");
            }                
        }

        System.Random random = new System.Random();
        public float BetterRandomThenUnity(float minimum, float maximum)
        {
            return (float)(random.NextDouble() * (maximum - minimum) + minimum);
        }
        
        public virtual void InitAttackTime()
        {
            _tryBlockTime = BetterRandomThenUnity(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
            _attackTime = BetterRandomThenUnity(_mintAttackTime, _maxAttackTime) + Time.time;
            attackCount = (int)BetterRandomThenUnity(_minAttackCount, _maxAttackCount);
        }

        public virtual void ResetAttackTime()
        {            
            attackCount = 0;
            _attackTime = Random.Range(_mintAttackTime, _maxAttackTime) + Time.time;
        }

        public virtual bool isBlocking
        {
            get
            {               
                return _isBlocking;
            }
            protected set
            {
                if (isBlockingHash != -1) animator.SetBool(isBlockingHash, value);
                _isBlocking = value;
            }
        }

        public virtual bool canBlockInCombat
        {
            get
            {
                return _combatBlockingChance > 0 && Time.time > _tryBlockTime && Time.time> _blockingTime && !customAction;
            }
        }

        public virtual void ResetBlockTime()
        {
            _blockingTime = 0;
        }

        public virtual void Blocking()
        {            
            if (!isBlocking && canBlockInCombat)
            {
                if (CheckChanceToBlock(_combatBlockingChance))
                {
                    isBlocking = true;
                    _blockingTime = Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
                }
            }            
        }

        protected virtual void ImmediateBlocking()
        {
            if (CheckChanceToBlock(_onDamageBlockingChance))
            {                
                _blockingTime = Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
                isBlocking = true;             
            }          
        }

        protected virtual bool CheckChanceToBlock(float chance)
        {
            return Random.Range(0f, 100f) <= chance;
        }

        public virtual void ResetAttackTriggers()
        {
            animator.ResetTrigger("WeakAttack");
            animator.ResetTrigger("StrongAttack");
        }

        public virtual void OnEnableAttack()
        {
            attackCount--;
        }

        public virtual void OnDisableAttack()
        {
            if (attackCount <= 0) InitAttackTime();
        }
       
        public virtual void AimTo(Vector3 point, float stayLookTime = 1, object sender = null)
        {

        }

        public virtual void AimToTarget(float stayLookTime = 1, object sender = null)
        {

        }

        protected virtual void TryBlockAttack(vDamage damage)
        {
            var canBlock = !ignoreDefenseDamageTypes.Contains(damage.damageType) && !damage.ignoreDefense;
            if (string.IsNullOrEmpty(damage.damageType) && canBlock)
            {
                ImmediateBlocking();              
            }
            damage.hitReaction = !isBlocking || !canBlock;
        }

        public override void TakeDamage(vDamage damage)
        {
            TryBlockAttack(damage);
            base.TakeDamage(damage);           
        }
    }
}
