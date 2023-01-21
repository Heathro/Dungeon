using System;
using System.Collections.Generic;
using UnityEngine;
using Control;
using Stats;
using UI.Ingame;
using Utils;
using Saving;
using Core;
using Movement;

namespace Abilities
{
    public class BuffStore : MonoBehaviour, IModifierProvider, ISaveable, IAction
    {
        // CONFIG

        [SerializeField] float timerMultiply = 5f;
        [SerializeField] BuffEffect overweight;
        [SerializeField] BuffEffect charmed;
        [SerializeField] BuffEffect taunted;

        // CACHE

        FightScheduler fightScheduler;
        OverheadUI overheadUI;
        ActionScheduler actionScheduler;
        Mover mover;
        PlayerController playerController;

        // STATE

        public event Action buffStoreUpdated;
        public event Action buffTimersUpdated;

        Dictionary<BuffEffect, float> timers = new Dictionary<BuffEffect, float>();
        bool fightRunning = false;

        // LIFECYCLE

        void Awake()
        {
            overheadUI = GetComponentInChildren<OverheadUI>();
            actionScheduler = GetComponent<ActionScheduler>();
            mover = GetComponent<Mover>();
            playerController = GetComponent<PlayerController>();
        }

        void Start()
        {
            fightScheduler = FindObjectOfType<FightScheduler>();
            fightScheduler.onEnemyAggro += StartFightMode;
            fightScheduler.onFightFinish += ReturnToCivilMode;
        }

        void Update()
        {
            if (!fightRunning)
            {
                CivilMode();
            }
        }

        // PUBLIC

        public int GetSize()
        {
            return timers.Count;
        }

        public void ClearBuffStore()
        {
            timers.Clear();
            UpdateBuffStore();
            UpdateBuffTimers();
        }

        public void StartEffect(BuffEffect buffEffect, float time = -1)
        {
            if (buffEffect == null) return;

            RemoveSimilar(buffEffect);

            DamageType damageType = buffEffect.GetDamageType();
            string title = buffEffect.GetDisplayName();

            if (time == -1)
            {
                overheadUI.AddPopUp(0f, damageType, false, title);
                timers[buffEffect] = buffEffect.GetDuration();
            }
            else
            {
                timers[buffEffect] = time;
            }

            if (fightRunning)
            {
                if (!buffEffect.IsSkippingFirst() && buffEffect.IsPeriodic())
                {
                    buffEffect.ApplyEffect(gameObject);
                }
            }

            ReplaceEffect(buffEffect);

            UpdateBuffTimers();
            UpdateBuffStore();
        }

        public void SetOverweight(bool isEnable)
        {
            if (isEnable)
            {
                if (timers.ContainsKey(overweight)) return;
                StartEffect(overweight);
                UpdateBuffStore();
                UpdateBuffTimers();
                actionScheduler.StartAction(this);
                mover.CancelAction();
                playerController.DisableTargetMarker();
            }
            else
            {
                if (!timers.ContainsKey(overweight)) return;
                timers.Remove(overweight);
                UpdateBuffStore();
                UpdateBuffTimers();
            }
        }

        public bool IsOverweight()
        {
            return timers.ContainsKey(overweight);
        }

        public bool IsCharmed()
        {
            return timers.ContainsKey(charmed);
        }

        public bool IsTaunted()
        {
            return timers.ContainsKey(taunted);
        }

        public Dictionary<BuffEffect, float> GetAllBuffEffects()
        {
            return timers;
        }

        public void UpdateFightTimers()
        {
            List<BuffEffect> keys = new List<BuffEffect>(timers.Keys);

            bool updateStore = false;

            foreach (BuffEffect buffEffect in keys)
            {
                if (buffEffect.IsPermanent()) continue;

                timers[buffEffect] -= 1f;

                if (timers.ContainsKey(buffEffect) && timers[buffEffect] <= 0)
                {
                    timers.Remove(buffEffect);
                    updateStore = true;
                }
            }

            UpdateBuffTimers();
            if (updateStore) UpdateBuffStore();
        }

        public void ApplyBuffEffect()
        {
            List<BuffEffect> keys = new List<BuffEffect>(timers.Keys);

            foreach (BuffEffect buffEffect in keys)
            {
                if (buffEffect.IsPermanent()) continue;
                
                if (buffEffect.IsPeriodic())
                {
                    buffEffect.ApplyEffect(gameObject);
                }
            }
        }

        public IEnumerable<float> GetAdditiveModifiers(CharacterStat stat)
        {
            foreach (BuffEffect buffEffect in timers.Keys)
            {
                foreach (float modifier in buffEffect.GetAdditiveModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }

        public IEnumerable<float> GetPercentageModifiers(CharacterStat stat)
        {
            foreach (BuffEffect buffEffect in timers.Keys)
            {
                foreach (float modifier in buffEffect.GetPercentageModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }

        public void CancelAction()
        {
            
        }

        // PRIVATE

        void CivilMode()
        {
            List<BuffEffect> keys = new List<BuffEffect>(timers.Keys);

            bool updateStore = false;
            bool updateTimers = false;

            foreach (BuffEffect buffEffect in keys)
            {
                if (buffEffect.IsPermanent()) continue;

                float before = Mathf.Floor(timers[buffEffect]);
                timers[buffEffect] -= Time.deltaTime / timerMultiply;
                float after = Mathf.Floor(timers[buffEffect]);

                if (after < before)
                {
                    if (before != buffEffect.GetDuration())
                    {
                        buffEffect.ApplyEffect(gameObject);
                    }                    
                    updateTimers = true;
                }

                if (timers.ContainsKey(buffEffect) && timers[buffEffect] <= 0)
                {
                    timers.Remove(buffEffect);
                    updateStore = true;
                }
            }

            if (updateStore) UpdateBuffStore();
            if (updateTimers) UpdateBuffTimers();
        }

        void StartFightMode()
        {
            fightRunning = true;
        }

        void ReturnToCivilMode()
        {
            fightRunning = false;
        }

        void UpdateBuffStore()
        {
            if (buffStoreUpdated != null)
            {
                buffStoreUpdated();
            }
        }

        void UpdateBuffTimers()
        {
            if (buffTimersUpdated != null)
            {
                buffTimersUpdated();
            }
        }

        void RemoveSimilar(BuffEffect buffEffect)
        {
            List<BuffEffect> keys = new List<BuffEffect>(timers.Keys);

            foreach (BuffEffect effect in keys)
            {
                if (buffEffect.GetDisplayName() == effect.GetDisplayName())
                {
                    timers.Remove(effect);
                }
            }
        }

        void ReplaceEffect(BuffEffect newEffect)
        {
            if (newEffect.GetReplacing() == null) return;

            RemoveSimilar(newEffect.GetReplacing());
        }

        [System.Serializable]
        struct BuffRecord
        {
            public string id;
            public float time;
        }

        object ISaveable.CaptureState()
        {
            BuffRecord[] record = new BuffRecord[timers.Count];

            int i = 0;

            foreach (KeyValuePair<BuffEffect, float> buff in timers)
            {
                record[i].id = buff.Key.GetBuffID();
                record[i].time = buff.Value;
                i++;
            }

            return record;
        }

        void ISaveable.RestoreState(object state)
        {
            BuffRecord[] record = (BuffRecord[])state;

            foreach (BuffRecord buff in record)
            {
                StartEffect(BuffEffect.GetFromID(buff.id), buff.time);
            }
        }
    }
}