using System;
using UnityEngine;
using Attributes;
using Control;

namespace Abilities
{
    public class TargetProjectile : MonoBehaviour
    {
        // CONFIG

        [SerializeField] EffectStrategy popUpMissedEffect;
        [SerializeField] float speed = 20f;
        [SerializeField] float heightOffset = 1.3f;

        // STATE

        AbilityData data;
        EffectStrategy[] impactEffects;
        Action finished;
        Health health;
        GameObject target;
        bool isMissed = false;

        // LIFECYCLE

        void Start()
        {
            transform.LookAt(target.transform.position + Vector3.up * heightOffset);
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        // PUBLIC

        public void SetTarget(AbilityData data, EffectStrategy[] impactEffects, Action finished, GameObject target, bool isMissed)
        {
            this.data = data;
            this.impactEffects = impactEffects;
            this.finished = finished;
            this.health = target.GetComponent<Health>();
            this.target = target;
            this.isMissed = isMissed;
        }

        // PRIVATE

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != health) return;

            if (isMissed)
            {
                popUpMissedEffect.StartEffect(data, other.gameObject);
            }
            else
            {
                foreach (EffectStrategy effect in impactEffects)
                {
                    effect.StartEffect(data, other.gameObject);
                }
            }

            finished();

            Destroy(gameObject);
        }
    }
}