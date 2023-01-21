using System.Collections.Generic;
using UnityEngine;
using Stats;
using Utils;
using Saving;

namespace Abilities
{
    public abstract class BuffEffect : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG

        [SerializeField] string buffID = null;
        [SerializeField] Sprite icon = null;
        [SerializeField] string title = "";
        [SerializeField][TextArea] string description = "";
        [SerializeField] float duration = 5f;
        [SerializeField] DamageType damageType = DamageType.Critical;
        [SerializeField] bool isPeriodic = false;
        [SerializeField] bool skipFirst = false;
        [SerializeField] bool isPermanent = false;
        [SerializeField] BuffEffect replacing = null;

        [SerializeField] Modifier[] additiveModifiers = null;
        [SerializeField] Modifier[] percentageModifiers = null;

        [System.Serializable]
        class Modifier
        {
            public CharacterStat stat;
            public float amount;
        }

        static Dictionary<string, BuffEffect> itemLookupCache;

        // PUBLIC

        public static BuffEffect GetFromID(string buffID)
        {
            if (itemLookupCache == null)
            {
                itemLookupCache = new Dictionary<string, BuffEffect>();
                var itemList = Resources.LoadAll<BuffEffect>("");
                foreach (var item in itemList)
                {
                    if (itemLookupCache.ContainsKey(item.buffID))
                    {
                        Debug.LogError(string.Format("Duplicate in BuffEffect lookUp for objects: {0} and {1}", itemLookupCache[item.buffID], item));
                        continue;
                    }
                    itemLookupCache[item.buffID] = item;
                }
            }
            if (buffID == null || !itemLookupCache.ContainsKey(buffID)) return null;
            return itemLookupCache[buffID];
        }

        public string GetBuffID()
        {
            return buffID;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetDisplayName()
        {
            return title;
        }

        public string GetDescription()
        {
            return description;
        }

        public float GetDuration()
        {
            return duration;
        }

        public DamageType GetDamageType()
        {
            return damageType;
        }

        public bool IsPeriodic()
        {
            return isPeriodic;
        }

        public bool IsSkippingFirst()
        {
            return skipFirst;
        }

        public BuffEffect GetReplacing()
        {
            return replacing;
        }

        public bool IsPermanent()
        {
            return isPermanent;
        }

        public abstract void ApplyEffect(GameObject target);

        public IEnumerable<float> GetAdditiveModifiers(CharacterStat stat)
        {
            foreach (Modifier modifier in additiveModifiers)
            {
                if (modifier.stat == stat)
                {
                    yield return modifier.amount;
                }
            }
        }

        public IEnumerable<float> GetPercentageModifiers(CharacterStat stat)
        {
            foreach (Modifier modifier in percentageModifiers)
            {
                if (modifier.stat == stat)
                {
                    yield return modifier.amount;
                }
            }
        }

        // PRIVATE

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (string.IsNullOrWhiteSpace(buffID))
            {
                buffID = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {

        }
    }
}