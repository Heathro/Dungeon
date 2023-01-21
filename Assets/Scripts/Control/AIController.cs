using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Combat;
using Movement;
using Attributes;
using UI.Ingame;
using Utils;
using Stats;
using Abilities;
using Skills;
using Audio;

namespace Control
{
    public class AIController : MonoBehaviour, IController
    {
        // CONFIG

        [SerializeField] float walkSpeed = 1.789172f;
        [SerializeField] float runSpeed = 4.197435f;
        [SerializeField] float scanDistance = 5f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 5f;
        [SerializeField] bool aggressive = false;

        [SerializeField] int startActionPoints = 4;
        [SerializeField] float thinkingTimeMin = 1f;
        [SerializeField] float thinkingTimeMax = 3f;
        [SerializeField] float afterTurnDelay = 2f;
        [SerializeField] float afterSkillDelay = 3f;
        [SerializeField] float maxHealthFractionToHeal = 0.75f;

        [SerializeField] int maxAttempts = 3;

        // CACHE

        PlayerController[] players;
        NavMeshAgent navMeshAgent;
        NavMeshObstacle navMeshObstacle;
        Fighter fighter;
        Mover mover;        
        Health health;
        FightScheduler fightScheduler;
        BattleMarker battleMarker;
        BaseStats baseStats;
        BuffStore buffStore;
        GameObject player = null;
        ControlSwitcher controlSwitcher;
        Health tank;
        AISkillStore aiSkillStore;
        CooldownStore cooldownStore;
        AudioController audioController;

        // STATE

        LazyValue<Vector3> guardPosition;
        float timeAtWaypoint = Mathf.Infinity;
        int currentWaypointIndex = 0;
        bool fightMode = false;
        bool aggrevated = false;

        bool takingTurn = false;
        int actionPoints = 0;
        bool isFighting = false;
        int attemptsMade = 0;

        float mileage = -1f;

        // LIFECYCLE

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshObstacle = GetComponent<NavMeshObstacle>();
            baseStats = GetComponent<BaseStats>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            health = GetComponent<Health>();
            battleMarker = GetComponentInChildren<BattleMarker>();
            buffStore = GetComponent<BuffStore>();
            aiSkillStore = GetComponent<AISkillStore>();
            cooldownStore = GetComponent<CooldownStore>();
            audioController = GetComponent<AudioController>();

            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        void Start()
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            players = controlSwitcher.GetPlayers();
            tank = controlSwitcher.GetTank();

            guardPosition.ForceInit();
            health.SetAggrevateTarget(Aggrevate);
        }

        void Update()
        {
            if (IsEnemyDead()) return;

            if (fightMode)
            {
                FightMode();
            }
            else if (aggressive && ScanForPlayer())
            {
                Provoke();
            }
            else
            {
                CivilMode();
            }

            UpdateTimers();
        }

        // PUBLIC

        public void Provoke()
        {
            fightMode = true;
            fightScheduler.StartFight(transform.parent);
        }

        public void Aggrevate()
        {            
            aggressive = true;
            aggrevated = true;

            if (!isFighting) Provoke();
        }

        public Coroutine TakeTurn()
        {
            takingTurn = true;
            mileage = -1;
            actionPoints = startActionPoints;
            return StartCoroutine(TakingTurn());
        }

        public void EnableControl(bool isEnable)
        {
            enabled = isEnable;
        }

        public void EnableAgent(bool isEnable)
        {
            if (isEnable && navMeshObstacle.enabled) return;
            navMeshAgent.enabled = isEnable;
        }

        public void EnableObstacle(bool isEnable)
        {
            if (isEnable && navMeshAgent.enabled) return;
            navMeshObstacle.enabled = isEnable;
        }

        public void SetInFight(bool isFighting)
        {
            this.isFighting = isFighting;
        }

        public bool IsFighting()
        {
            return isFighting;
        }    

        public bool IsEnemy()
        {
            return true;
        }

        public Health GetHealth()
        {
            return health;
        }

        public void EndTurn()
        {
            takingTurn = false;
        }

        public void SetBattleMarker(FighterType fighterType)
        {
            battleMarker.SetBattleMarker(fighterType == FighterType.None ? FighterType.None : FighterType.Enemy);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public float GetInitiative()
        {
            return baseStats.GetStat(CharacterStat.Initiative);
        }

        public bool IsAggressive()
        {
            return aggressive;
        }

        public void UpdateBuffs()
        {
            buffStore.UpdateFightTimers();
        }

        public void ApplyBuffEffect()
        {
            buffStore.ApplyBuffEffect();
        }

        public int GetActionPoints()
        {
            return actionPoints;
        }

        public void UseActionPoints(int spend)
        {
            actionPoints -= spend;
        }

        // PRIVATE

        IEnumerator TakingTurn()
        {
            attemptsMade = 0;
            fighter.enabled = false;
            player = null;

            MakeFirstDecision();

            while (takingTurn) yield return null;

            yield return new WaitForSeconds(afterTurnDelay);
        }

        void ActionFinished()
        {   
            fighter.enabled = false;
            player = null;
            StartCoroutine(MakeDecision());
        }

        void MakeFirstDecision()
        {
            StartCoroutine(MakeDecision());
        }
        
        IEnumerator MakeDecision()
        {      
            yield return new WaitForSeconds(UnityEngine.Random.Range(thinkingTimeMin, thinkingTimeMax));

            attemptsMade++;

            if (actionPoints < 1 || attemptsMade > maxAttempts)
            {
                EndTurn();
            }
            else if (actionPoints == 1)
            {
                AttemptHeal();
            }
            else if (actionPoints > 1)
            {
                AttemptDamage();
            }
        }

        void AttemptHeal()
        {
            Ability healingSkill = aiSkillStore.GetHealingSpell();
            if (healingSkill == null || cooldownStore.GetTimeRemaining(healingSkill) > 0)
            {
                AttemptBuff();
                return;
            }

            List<Health> correctTargetPool = new List<Health>();
            if (buffStore.IsCharmed())
            {
                correctTargetPool = GetAlivePlayers();
            }
            else
            {
                correctTargetPool = fightScheduler.GetAliveEnemies();
            }

            Health correctTarget = GetLowestHealthTarget(GetTargetsInRange(correctTargetPool, healingSkill.GetDistance(), true, false), true, maxHealthFractionToHeal);
            if (correctTarget != null)
            {
                healingSkill.Use(gameObject, correctTarget.gameObject);
                StartCoroutine(AfterSkillDelay());
            }
            else
            {
                AttemptBuff();
            }
        }

        void AttemptDamage()
        {
            if (HealingRequired())
            {
                return;
            }

            Ability damagingSkill = aiSkillStore.GetAvailableDamagingSpell();
            if (damagingSkill == null)
            {
                WeaponAttack();
                return;
            }

            if (buffStore.IsTaunted() && !tank.IsDead())
            {
                if (IsTankInRange(damagingSkill.GetDistance()))
                {
                    damagingSkill.Use(gameObject, tank.gameObject);
                    StartCoroutine(AfterSkillDelay());
                }
                else
                {
                    WeaponAttack();
                }
                return;
            }

            List<Health> correctTargetPool = new List<Health>();
            if (buffStore.IsCharmed())
            {
                correctTargetPool = fightScheduler.GetAliveEnemies();
            }
            else
            {
                correctTargetPool = GetAlivePlayers();
            }

            Health correctTarget = GetLowestHealthTarget(GetTargetsInRange(correctTargetPool, damagingSkill.GetDistance(), false), false);
            if (correctTarget != null)
            {
                damagingSkill.Use(gameObject, correctTarget.gameObject);
                StartCoroutine(AfterSkillDelay());
            }
            else
            {
                WeaponAttack();
            }
        }

        bool HealingRequired()
        {
            Ability healingSkill = aiSkillStore.GetHealingSpell();
            if (healingSkill == null || cooldownStore.GetTimeRemaining(healingSkill) > 0)
            {
                return false;
            }

            List<Health> correctTargetPool = new List<Health>();
            if (buffStore.IsCharmed())
            {
                correctTargetPool = GetAlivePlayers();
            }
            else
            {
                correctTargetPool = fightScheduler.GetAliveEnemies();
            }

            Health correctTarget = GetLowestHealthTarget(GetTargetsInRange(correctTargetPool, healingSkill.GetDistance(), true, false), true, maxHealthFractionToHeal);
            if (correctTarget != null)
            {
                healingSkill.Use(gameObject, correctTarget.gameObject);
                StartCoroutine(AfterSkillDelay());
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IsTankInRange(float distance)
        {
            return Vector3.Distance(transform.position, tank.transform.position) <= distance;
        }    

        void AttemptBuff()
        {
            Ability availableBuff = aiSkillStore.GetAvailableBuff();
            if (availableBuff != null)
            {
                availableBuff.Use(gameObject, gameObject);
                StartCoroutine(AfterSkillDelay());
            }
            else
            {
                EndTurn();
            }
        }

        void WeaponAttack()
        {
            player = ChooseTarget();

            if (player == null)
            {
                fighter.enabled = false;
                EndTurn();
            }
            else
            {
                fighter.enabled = true;
                EnableControl(true);
            }
        }

        GameObject ChooseTarget()
        {
            if (buffStore.IsCharmed())
            {
                return GetCorrectTarget(fightScheduler.GetAliveEnemies());
            }
            else if (buffStore.IsTaunted() && !tank.IsDead())
            {
                return tank.gameObject;
            }
            else
            {
                return GetCorrectTarget(GetAlivePlayers());
            }
        }

        GameObject GetCorrectTarget(List<Health> targetPool)
        {
            Health correctTarget;

            var targetsInRange = GetTargetsInRange(targetPool, fighter.GetAttackRange(), false);
            correctTarget = GetLowestHealthTarget(targetsInRange, false);           

            if (correctTarget == null)
            {
                correctTarget = GetClosestTarget(targetPool);
            }

            return correctTarget == null ? null : correctTarget.gameObject;
        }

        List<Health> GetTargetsInRange(List<Health> targetPool, float range, bool healing, bool undeadApplied = true)
        {
            List<Health> targetsInRange = new List<Health>();

            foreach (Health targetToChoose in targetPool)
            {
                if (!undeadApplied && targetToChoose.IsUndead()) continue;

                if (healing && object.ReferenceEquals(targetToChoose, health))
                {
                    targetsInRange.Add(targetToChoose);
                    continue;
                }

                if (object.ReferenceEquals(targetToChoose, health)) continue;
                if (!fighter.IsMelee() && !Scanner.IsTargetVisible(gameObject, targetToChoose.gameObject)) continue;

                if (Vector3.Distance(targetToChoose.transform.position, transform.position) <= range)
                {
                    targetsInRange.Add(targetToChoose);
                }
            }
            return targetsInRange;
        }

        Health GetClosestTarget(List<Health> targetPool)
        {
            Health correctTarget = null;
            float min = 10000f;
            foreach (Health targetToChoose in targetPool)
            {
                if (object.ReferenceEquals(targetToChoose, health)) continue;
                if (!fighter.IsMelee() && !Scanner.IsTargetVisible(gameObject, targetToChoose.gameObject)) continue;

                float distance = Vector3.Distance(targetToChoose.transform.position, transform.position);
                if (distance < min)
                {
                    correctTarget = targetToChoose;
                    min = distance;
                }
            }
            return correctTarget;
        }

        Health GetLowestHealthTarget(List<Health> targetPool, bool healing, float maxHealthFraction = 1f)
        {
            Health correctTarget = null;
            float min = 10000f;
            foreach (Health targetToChoose in targetPool)
            {
                if (!healing && object.ReferenceEquals(targetToChoose, health)) continue;
                if (targetToChoose.GetFraction() > maxHealthFraction) continue;

                float healthPoints = targetToChoose.GetHealth();
                if (healthPoints < min)
                {
                    correctTarget = targetToChoose;
                    min = healthPoints;
                }
            }
            return correctTarget;
        }

        List<Health> GetAlivePlayers()
        {
            List<Health> alivePlayers = new List<Health>();
            foreach (PlayerController playerController in players)
            {
                Health health = playerController.GetComponent<Health>();
                if (!health.IsDead())
                {
                    alivePlayers.Add(health);
                }
            }
            return alivePlayers;
        }

        IEnumerator AfterSkillDelay()
        {
            fighter.enabled = false;
            player = null;
            yield return new WaitForSeconds(afterSkillDelay);
            ActionFinished();
        }    

        void SetMileage(float mileage)
        {
            this.mileage = mileage;
        }

        void FightMode()
        {
            if (player == null) return;

            navMeshAgent.speed = runSpeed;
            AttackBehaviour();
        }

        void CivilMode()
        {
            fightMode = false;
            navMeshAgent.speed = walkSpeed;
            PatrolBehaviour();
        }

        void AttackBehaviour()
        {
            if (player == null) return;

            fighter.StartAttackAction(player, actionPoints, UseActionPoints, ActionFinished, mileage, SetMileage);
            EnableControl(false);
        }

        void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.Value;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }

            if (timeAtWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition);
            }
        }

        bool AtWaypoint()
        {
            return Vector3.Distance(transform.position, GetCurrentWaypoint()) < waypointTolerance;
        }

        void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        bool ScanForPlayer()
        {
            foreach (PlayerController candidate in players)
            {
                if (IsPlayerClose(candidate.transform.position) && Scanner.IsTargetVisible(gameObject, candidate.gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsPlayerClose(Vector3 position)
        {
            if (aggrevated) return true;

            return Vector3.Distance(position, transform.position) <= scanDistance;
        }

        bool IsEnemyDead()
        {
            if (health.IsDead())
            {
                EndTurn();
                return true;
            }
            return false;
        }

        void UpdateTimers()
        {
            timeAtWaypoint += Time.deltaTime;
        }

        Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        // TESTING

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, scanDistance);
        }

        // ANIMATION

        void FootR()
        {
            audioController.PlayFootstep(true);
        }

        void FootL()
        {
            audioController.PlayFootstep(false);
        }
    }
}