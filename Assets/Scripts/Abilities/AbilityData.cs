using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Abilities
{
    public class AbilityData : IAction
    {
        // STATE

        GameObject user;
        Vector3 targetPoint;
        IEnumerable<GameObject> targets;
        IEnumerable<GameObject> missedTargets;
        bool cancelled = false;
        float distance = 0f;
        int cost = 0;

        // CONSTRUCT

        public AbilityData(GameObject user)
        {
            this.user = user;
        }

        // PUBLIC

        public GameObject GetUser()
        {
            return user;
        }

        public void SetTargets(IEnumerable<GameObject> targets)
        {
            this.targets = targets;
        }

        public IEnumerable<GameObject> GetTargets()
        {
            return targets;
        }

        public void SetMissedTargets(IEnumerable<GameObject> missedTargets)
        {
            this.missedTargets = missedTargets;
        }

        public IEnumerable<GameObject> GetMissedTargets()
        {
            return missedTargets;
        }

        public void SetTargetPoint(Vector3 targetPoint)
        {
            this.targetPoint = targetPoint;
        }

        public Vector3 GetTargetPoint()
        {
            return targetPoint;
        }

        public void SetDistance(float distance)
        {
            this.distance = distance;
        }

        public float GetDistance()
        {
            return distance;
        }

        public void SetCost(int cost)
        {
            this.cost = cost;
        }

        public int GetCost()
        {
            return cost;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            user.GetComponent<MonoBehaviour>().StartCoroutine(coroutine);
        }

        public bool IsCancelled()
        {
            return cancelled;
        }

        public void CancelAction()
        {
            cancelled = true;
        }
    }
}