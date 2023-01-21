using System;
using UnityEngine;
using Core;
using Movement;

namespace Control
{
    public class Interactor : MonoBehaviour, IAction
    {
        // CACHE

        ActionScheduler actionScheduler;
        PlayerController playerController;
        Mover mover;

        // STATE

        IRaycastable interactTarget = null;
        IRaycastable cachedTarget = null;
        bool fightMode = false;
        int actionPoints = 0;
        Action<int> useActionPoints = null;
        Action actionFinished = null;
        float mileage = 0f;
        Action<float> setMileage = null;
        bool movementStarted = false;

        Action leaderControlEnable = null;

        // LIFECYCLE

        void Start()
        {
            actionScheduler = GetComponent<ActionScheduler>();
            playerController = GetComponent<PlayerController>();
            mover = GetComponent<Mover>();
        }

        void Update()
        {
            if (interactTarget == null) return;

            if (fightMode)
            {
                FightBehaviour();
            }
            else
            {
                CivilBehaviour();
            }
        }

        public void StartInteractAction(IRaycastable interactTarget, bool isDialogue = false)
        {
            if (!isDialogue && leaderControlEnable != null)
            {
                leaderControlEnable();
            }

            fightMode = false;

            actionScheduler.StartAction(this);
            this.interactTarget = interactTarget;
        }

        public void StartInteractAction(IRaycastable interactTarget, int actionPoints, Action<int> useActionPoints, Action actionFinished, float mileage, Action<float> setMileage)
        {
            fightMode = true;
            movementStarted = false;

            actionScheduler.StartAction(this);
            this.interactTarget = interactTarget;

            this.actionPoints = actionPoints;
            this.useActionPoints = useActionPoints;
            this.actionFinished = actionFinished;
            this.mileage = mileage;
            this.setMileage = setMileage;

            cachedTarget = interactTarget;
        }

        public void CancelAction()
        {
            interactTarget = null;
        }

        public void SetLeaderControlEnable(Action leaderControlEnable)
        {
            this.leaderControlEnable = leaderControlEnable;
        }

        // PRIVATE

        void FightBehaviour()
        {
            if (IsTargetInRange())
            {
                Interact(actionPoints, mileage);
            }
            else if (!movementStarted)
            {
                movementStarted = true;
                mover.StartMoveAction(interactTarget.GetPosition(), actionPoints, useActionPoints, null, Interact, mileage, setMileage, interactTarget.GetInteractRange(playerController));
            }
        }

        void CivilBehaviour()
        {
            if (IsTargetInRange())
            {
                mover.CancelAction();
                Interact();
            }
            else
            {
                mover.MoveTo(interactTarget.GetPosition());
            }
        }

        void Interact()
        {
            interactTarget.Interact(playerController);
            CancelAction();
        }

        void Interact(int actionPoints, float mileage)
        {
            interactTarget = cachedTarget;

            this.actionPoints = actionPoints;
            this.mileage = mileage;

            int interactCost = interactTarget.GetInteractCost(playerController);

            if (this.actionPoints <= 0 || this.actionPoints < interactCost)
            {
                FinishFightMode();
                return;
            }

            this.actionPoints -= interactCost;
            useActionPoints(interactCost);

            interactTarget.Interact(playerController);
            FinishFightMode();
        }

        void FinishFightMode()
        {
            fightMode = false;
            setMileage(mileage);
            actionFinished();
            CancelAction();
        }

        bool IsTargetInRange()
        {
            return Vector3.Distance(transform.position, interactTarget.GetPosition()) <= interactTarget.GetInteractRange(playerController);
        }
    }
}