using System;
using UnityEngine;

namespace Abilities
{
    public class AreaProjectile : MonoBehaviour
    {
        // CONFIG

        [SerializeField] float speed = 20f;
        [SerializeField] float tolerance = 0.1f;

        // STATE

        AbilityData data;
        EffectStrategy[] impactEffects;
        Action finished;
        float distance = 0f;

        // LIFECYCLE

        void Start()
        {
            transform.LookAt(data.GetTargetPoint());
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, data.GetUser().transform.position) >= distance - tolerance)
            {
                LaunchImpact();
                Destroy(gameObject);
            }
        }

        // PUBLIC

        public void SetTarget(AbilityData data, EffectStrategy[] impactEffects, Action finished)
        {
            this.data = data;
            this.impactEffects = impactEffects;
            this.finished = finished;
            this.distance = data.GetDistance();
        }

        // PRIVATE

        void LaunchImpact()
        {
            foreach (EffectStrategy effect in impactEffects)
            {
                effect.StartEffect(data, finished);
            }
        }
    }
}