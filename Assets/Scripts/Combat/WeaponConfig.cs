using UnityEngine;
using Attributes;
using Inventories;
using Utils;
using Stats;

namespace Combat
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Items/WeaponConfig", order = 0)]
    public class WeaponConfig : EquipableItem
    {
        // CONFIG

        [SerializeField] WeaponType weaponType = WeaponType.None;
        [SerializeField] Weapon equippedPrefab = null;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] bool isMelee = true;
        [SerializeField] bool isRanged = false;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] float weaponDamage = 0f;
        [SerializeField] DamageType damageType = DamageType.Health;
        [SerializeField] CharacterStat damageBase = CharacterStat.StrengthDamage;
        [SerializeField] int attackCost = 2;
        [SerializeField] Projectile projectilePrefab = null;
        [SerializeField] RangedProjectileType projectileType = RangedProjectileType.None;

        // PUBLIC

        public Weapon SpawnWeapon(Transform rightHand, Transform leftHand, Animator animator)
        {
            DestroyOldWeapon(rightHand, leftHand);

            Weapon weapon = null;

            if (equippedPrefab != null)
            {
                weapon = Instantiate(equippedPrefab, GetHandTransform(rightHand, leftHand));
            }

            HideWeapon(rightHand, leftHand);

            return weapon;
        }

        public void LaunchProjectile(Health target, float damage, BaseStats targetStats, Transform rightHand, Transform leftHand)
        {
            Projectile projectile = Instantiate(projectilePrefab, GetHandTransform(rightHand, leftHand).position, Quaternion.identity);
            projectile.SetTarget(target, damage, damageType, targetStats);
        }

        public bool HasProjectile()
        {
            return projectilePrefab != null;
        }

        public float GetRange()
        {
            return weaponRange;
        }

        public float GetDamage()
        {
            return weaponDamage;
        }

        public DamageType GetDamageType()
        {
            return damageType;
        }

        public CharacterStat GetDamageBase()
        {
            return damageBase;
        }

        public string GetScaleWith()
        {
            switch (damageBase)
            {
                case CharacterStat.StrengthDamage: return "Strength";
                case CharacterStat.FinesseDamage: return "Finesse";
                case CharacterStat.IntelligenceDamage: return "Intelligence";
                default: return "Strength";
            }
        }

        public int GetCost()
        {
            return attackCost;
        }

        public bool IsMelee()
        {
            return isMelee;
        }

        public bool IsRanged()
        {
            return isRanged;
        }

        public RangedProjectileType GetProjectileType()
        {
            return projectileType;
        }

        public WeaponType GetWeaponType()
        {
            return weaponType;
        }

        public bool IsRightHanded()
        {
            return isRightHanded;
        }

        // PRIVATE

        void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            foreach (Transform equip in rightHand)
            {
                Destroy(equip.gameObject);
            }
            foreach (Transform equip in leftHand)
            {
                Destroy(equip.gameObject);
            }
        }

        void HideWeapon(Transform rightHand, Transform leftHand)
        {
            foreach (Transform equip in rightHand)
            {
                equip.gameObject.SetActive(false);
            }
            foreach (Transform equip in leftHand)
            {
                equip.gameObject.SetActive(false);
            }
        }

        Transform GetHandTransform(Transform rightHand, Transform leftHand)
        {
            return isRightHanded ? rightHand : leftHand;
        }
    }
}