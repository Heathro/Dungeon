using System.Collections.Generic;
using UnityEngine;
using Abilities;

namespace UI.HUD
{
    public class BuffDisplay : MonoBehaviour
    {
        // CONFIG

        [SerializeField] BuffStore buffStore = null;
        [SerializeField] BuffElement buffElementPrefab = null;
        [SerializeField] int startingPoolSize = 10;
        [SerializeField] int extensionSize = 4;

        // STATE

        List<BuffElement> buffPool = new List<BuffElement>();

        // LIFECYCLE

        void Awake()
        {
            if (buffStore == null) return;

            buffStore.buffTimersUpdated += Redraw;
            PopulatePool();
        }

        void Start()
        {
            if (buffStore == null) return;

            Redraw();
        }

        // PUBLIC

        public void SetBuffStore(BuffStore buffStore)
        {
            if (buffStore == null) return;

            this.buffStore = buffStore;
            buffStore.buffTimersUpdated += Redraw;

            Redraw();
        }
        
        // PRIVATE
        
        void Redraw()
        {   
            int buffCount = buffStore.GetSize();

            while (buffCount > buffPool.Count)
            {
                ExtendPool();
            }

            for (int i = buffCount; i < buffPool.Count; i++)
            {
                buffPool[i].gameObject.SetActive(false);
            }

            int n = 0;
            foreach (KeyValuePair<BuffEffect, float> buffEffect in buffStore.GetAllBuffEffects())
            {
                buffPool[n].gameObject.SetActive(true);
                buffPool[n].Setup(buffEffect.Key, buffEffect.Value);
                n++;
            }
        }

        void PopulatePool()  
        {
            for (int i = 0; i < startingPoolSize; i++)
            {
                buffPool.Add(Instantiate(buffElementPrefab, transform));
            }
        }

        void ExtendPool()
        {
            for (int i = 0; i < extensionSize; i++)
            {
                buffPool.Add(Instantiate(buffElementPrefab, transform));
            }
        }
    }
}