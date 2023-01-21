using System;
using UnityEngine;

namespace Control
{
    public class Scanner : MonoBehaviour
    {
        public static bool IsTargetVisible(GameObject player, GameObject target)
        {
            float visualOffset = 1.5f;
            float visualDistance = 40f;

            Vector3 playerPosition = player.transform.position + visualOffset * Vector3.up;
            Vector3 targetPosition = target.transform.position + visualOffset * Vector3.up;
            Vector3 direction = (targetPosition - playerPosition).normalized;

            RaycastHit[] hits = SortByDistance(Physics.RaycastAll(playerPosition, direction, visualDistance));
            foreach (RaycastHit hit in hits)
            {
                CombatTarget combatTarget = hit.transform.GetComponent<CombatTarget>();
                if (combatTarget == null) return false;

                if (object.ReferenceEquals(combatTarget, player.GetComponent<CombatTarget>())) continue;
                if (object.ReferenceEquals(target, hit.transform.gameObject)) return true;
            }

            return true;
        }

        public static bool IsTargetVisible(GameObject player, Vector3 targetPoint, float distanceToTarget)
        {
            float visualOffset = 1.5f;

            Vector3 playerPosition = player.transform.position + visualOffset * Vector3.up;
            Vector3 targetPosition = targetPoint + visualOffset * Vector3.up;
            Vector3 direction = (targetPosition - playerPosition).normalized;

            RaycastHit[] hits = SortByDistance(Physics.RaycastAll(playerPosition, direction, distanceToTarget - 0.05f));
            foreach (RaycastHit hit in hits)
            {
                CombatTarget combatTarget = hit.transform.GetComponent<CombatTarget>();
                if (combatTarget == null) return false;
            }

            return true;
        }

        static RaycastHit[] SortByDistance(RaycastHit[] hits)
        {
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }
    }
}