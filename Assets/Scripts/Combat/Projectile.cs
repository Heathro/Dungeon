using UnityEngine;
using Attributes;
using Utils;
using Stats;

namespace Combat
{
    public class Projectile : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject impact = null;
        [SerializeField] float effectDestroyDelay = 10f;
        [SerializeField] float speed = 20f;
        [SerializeField] float heightOffset = 1.3f;
        [SerializeField] float lifeTime = 10f;

        // STATE

        Health target;
        float damage;
        DamageType damageType;
        BaseStats targetStats;

        // LIFECYCLE

        void Start()
        {
            transform.LookAt(target.transform.position + Vector3.up * heightOffset);
        }

        void Update()
        {
            transform.LookAt(target.transform.position + Vector3.up * heightOffset);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target) return;
            
            target.TakeDamage(damage, damageType, targetStats, true);

            if (impact != null)
            {
                GameObject effect = Instantiate(impact, transform.position, Quaternion.identity);
                Destroy(effect, effectDestroyDelay);
            }

            Destroy(gameObject);
        }

        // PUBLIC

        public void SetTarget(Health target, float damage, DamageType damageType, BaseStats targetStats)
        {
            this.target = target;
            this.damage = damage;
            this.damageType = damageType;
            this.targetStats = targetStats;

            Destroy(gameObject, lifeTime);
        }
    }
}