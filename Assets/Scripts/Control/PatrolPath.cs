using UnityEngine;

namespace Control
{
    public class PatrolPath : MonoBehaviour
    {
        // CONFIG

        [SerializeField] float waypointGizmoRadius = 0.2f;

        // PUBLIC
        
        public Vector3 GetWaypoint(int index)
        {
            return transform.GetChild(index).position;
        }

        public int GetNextIndex(int index)
        {
            return index == transform.childCount - 1 ? 0 : index + 1;
        }

        // TESTING

        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < transform.childCount; i++)
            {
                Gizmos.DrawSphere(GetWaypoint(i), waypointGizmoRadius);
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(GetNextIndex(i)));
            }
        }
    }
}