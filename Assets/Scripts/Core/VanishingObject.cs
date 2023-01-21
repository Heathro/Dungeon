using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class VanishingObject : MonoBehaviour
    {
        // CACHE

        MeshRenderer meshRenderer = null;
        List<MeshRenderer> innerMeshes = new List<MeshRenderer>();
        SkinnedMeshRenderer skinMesh = null;
        ParticleSystem particles = null;

        // STATE

        bool vanished = false;
        int defaultMask = 0;

        static int vanishedMask = 8;

        // LIFECYCLE
        
        void Awake()
        {
            defaultMask = gameObject.layer;
            meshRenderer = GetComponent<MeshRenderer>();
            skinMesh = GetComponentInChildren<SkinnedMeshRenderer>();

            foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
            {
                innerMeshes.Add(mesh);
            }

            particles = GetComponent<ParticleSystem>();
        }

        void Update()
        {
            if (vanished)
            {
                if (meshRenderer != null) meshRenderer.enabled = false;
                if (skinMesh != null) skinMesh.enabled = false;

                foreach (MeshRenderer mesh in innerMeshes)
                {
                    mesh.enabled = false;
                }

                if (particles != null) particles.Stop();

                gameObject.layer = vanishedMask;
            }
            else
            {
                if (meshRenderer != null) meshRenderer.enabled = true;
                if (skinMesh != null) skinMesh.enabled = true;

                foreach (MeshRenderer mesh in innerMeshes)
                {
                    mesh.enabled = true;
                }

                if (particles != null) particles.Play();

                gameObject.layer = defaultMask;
            }

            vanished = false;
        }

        // PUBLIC

        public void Vanish()
        {
            vanished = true;
        }

        public bool IsVanished()
        {
            return gameObject.layer == vanishedMask;
        }
    }
}