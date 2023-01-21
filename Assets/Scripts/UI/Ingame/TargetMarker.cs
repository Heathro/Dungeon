using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace UI.Ingame
{
    public class TargetMarker : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Material movement;
        [SerializeField] Material attack;
        [SerializeField] Material interact;
        [SerializeField] Material notAvailable;
        [SerializeField] float distanceTolerance = 0.2f;
        [SerializeField] float lineWidth = 0.2f;
        
        // CACHE

        MeshRenderer meshRenderer;
        LineRenderer lineRenderer;

        // STATE

        Transform instantiator = null;

        // LIFECYCLE
        
        void Awake()
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            lineRenderer = GetComponentInChildren<LineRenderer>();
        }

        void Start()
        {
            meshRenderer.enabled = false;

            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.enabled = false;
        }

        void Update()
        {
            MoveTowardsToTarget();
        }

        // PUBLIC

        public void SetTargetMarker(MarkerType markerType)
        {
            switch (markerType)
            {
                case MarkerType.None: 
                    meshRenderer.enabled = false; 
                    break;

                case MarkerType.Movement: 
                    meshRenderer.enabled = true; 
                    meshRenderer.material = movement; 
                    break;

                case MarkerType.Attack: 
                    meshRenderer.enabled = true; 
                    meshRenderer.material = attack; 
                    break;

                case MarkerType.Interact:
                    meshRenderer.enabled = true;
                    meshRenderer.material = interact;
                    break;

                case MarkerType.NotAvailable:
                    meshRenderer.enabled = true;
                    meshRenderer.material = notAvailable;
                    break;
            }
        }
        
        public void DrawPathLine(NavMeshPath path)
        {
            lineRenderer.enabled = true;

            if (path != null && path.corners.Length > 1)
            {
                lineRenderer.positionCount = path.corners.Length;

                for (int i = 0; i < path.corners.Length; i++)
                {
                    lineRenderer.SetPosition(i, path.corners[i]);
                }
            }
        }

        public void DisablePathLine()
        {
            lineRenderer.enabled = false;
        }

        public void SetInstantiator(Transform instantiator)
        {
            this.instantiator = instantiator;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        // PRIVATE 

        void MoveTowardsToTarget()
        {
            if (instantiator == null) return;

            if (Vector3.Distance(transform.position, instantiator.position) < distanceTolerance)
            {
                SetTargetMarker(MarkerType.None);
                instantiator = null;
            }
        }
    }
}