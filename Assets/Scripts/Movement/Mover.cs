using System;
using UnityEngine;
using UnityEngine.AI;
using Attributes;
using Core;
using Saving;
using Stats;

namespace Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        // CACHE

        NavMeshAgent navMeshAgent;
        Animator animator;
        ActionScheduler actionScheduler;
        BaseStats baseStats;
        Health health;

        // STATE

        Vector3 destination = new Vector3(-100000f, -1000000f, -1000000f);
        float interactRange = 0f;
        bool fightMode = false;
        int actionPoints = 0;
        Action<int> useActionPoints = null;
        Action actionFinished = null;
        Action<int, float> attackReady = null;
        Action<float> setMileage = null;

        bool movementStarted = false;
        float mileage = 0f;

        Transform companionPosition = null;
        bool companionMode = true;

        Action leaderControlEnable = null;

        // LIFECYCLE

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionScheduler>();
            baseStats = GetComponent<BaseStats>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            if (health.IsDead()) return;

            UpdateAnimator();

            if (fightMode)
            {
                FightMode();
            }
            else if (companionMode && companionPosition != null)
            {
                MoveTo(companionPosition.position);
            }
        }

        // PUBLIC

        public void StartMoveAction(Vector3 destination)
        {
            if (leaderControlEnable != null)
            {
                leaderControlEnable();
            }

            fightMode = false;

            actionScheduler.StartAction(this);
            MoveTo(destination);
        }

        public void StartMoveAction(Vector3 destination, int actionPoints, Action<int> useActionPoints, Action actionFinished, Action<int, float> attackReady, float mileage, Action<float> setMileage, float interactRange)
        {
            fightMode = true;
            movementStarted = false;

            this.destination = destination;
            this.interactRange = interactRange;
            this.mileage = mileage;
            this.actionPoints = actionPoints;
            this.useActionPoints = useActionPoints;
            this.setMileage = setMileage;

            this.actionFinished = actionFinished;
            this.attackReady = attackReady;              
            
            if (mileage == -1)
            {
                this.actionPoints--;
                this.useActionPoints(1);
                this.mileage = 0f;
            }
            
            actionScheduler.StartAction(this);
            MoveTo(destination);
        }

        public void CancelAction()
        {
            destination = new Vector3(-100000f, -1000000f, -1000000f);

            if (navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
            }

            if (fightMode)
            {
                fightMode = false;             
                setMileage(mileage);
                setMileage = null;
                useActionPoints = null;

                if (attackReady != null)
                {
                    attackReady(actionPoints, mileage);
                    attackReady = null;
                }

                actionPoints = 0;
                
                if (actionFinished != null)
                {
                    actionFinished();
                    actionFinished = null;
                }
            }
        }

        public void SetLeaderControlEnable(Action leaderControlEnable)
        {
            this.leaderControlEnable = leaderControlEnable;
        }

        public void SetCompanionPosition(Transform companionPosition)
        {
            CancelAction();
            this.companionPosition = companionPosition;
        }

        public void EnableCompanionMode(bool companionMode)
        {
            this.companionMode = companionMode;
        }

        public void MoveTo(Vector3 destination)
        {
            if (navMeshAgent.enabled)
            {
                navMeshAgent.destination = destination;
                navMeshAgent.isStopped = false;
            }
        }

        public bool IsIdle()
        {
            return navMeshAgent.velocity.magnitude == 0;
        }

        // PRIVATE

        void FightMode()
        {
            if (IsTargetInRange())
            {
                movementStarted = false;
                CancelAction();
            }

            if (navMeshAgent.velocity.magnitude > 0)
            {
                movementStarted = true;
            }

            mileage += navMeshAgent.velocity.magnitude * Time.deltaTime;

            if (mileage > baseStats.GetStat(CharacterStat.Speed))
            {
                mileage = 0f;
                actionPoints--;
                if (useActionPoints != null) useActionPoints(1);
            }

            if (movementStarted == false) return;
            if (navMeshAgent.velocity.magnitude == 0 || actionPoints <= -1)
            {
                movementStarted = false;
                CancelAction();
            }
        }

        bool IsTargetInRange()
        {
            return Vector3.Distance(destination, transform.position) <= interactRange;
        }

        void UpdateAnimator()
        {
            animator.SetFloat("speed", navMeshAgent.velocity.magnitude);
        }

        object ISaveable.CaptureState()
        {
            return new SerializableTransform(transform);
        }

        void ISaveable.RestoreState(object state)
        {
            SerializableTransform form = (SerializableTransform)state;
            navMeshAgent.enabled = false;
            transform.position = form.GetPosition();
            transform.rotation = form.GetRotation();
            navMeshAgent.enabled = true;
            actionScheduler.CancelCurrentAction();
        }
    }
}