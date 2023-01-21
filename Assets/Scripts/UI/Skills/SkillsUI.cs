using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Skills;
using Abilities;

namespace UI.Skills
{
    public class SkillsUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] SkillStore skillStore;
        [SerializeField] Transform skillList;
        [SerializeField] SkillSlotUI skillSlotPrefab;
        [SerializeField] int startingPoolSize = 16;
        [SerializeField] int extensionSize = 8;

        // STATE

        List<SkillSlotUI> slotPool = new List<SkillSlotUI>();

        // LIFECYCLE

        void Awake()
        {
            PopulatePool();
            skillStore.storeUpdated += RedrawSkillDeck;
        }

        void OnEnable()
        {
            RedrawSkillDeck();
        }

        void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                FindObjectOfType<ShowHideUI>().CloseSkillDeck();
            }
        }

        // PRIVATE

        void RedrawSkillDeck()
        {
            int skillsCount = skillStore.GetSize();

            while (skillsCount > slotPool.Count)
            {
                ExtendPool();
            }

            for (int i = 0; i < slotPool.Count; i++)
            {
                if (i < skillsCount)
                {
                    slotPool[i].gameObject.SetActive(true);
                    slotPool[i].Setup(i, skillStore);
                }
                else
                {
                    slotPool[i].gameObject.SetActive(false);
                }
            }
        }

        void PopulatePool()
        {
            for (int i = 0; i < startingPoolSize; i++)
            {
                slotPool.Add(Instantiate(skillSlotPrefab, skillList));
            }
        }

        void ExtendPool()
        {
            for (int i = 0; i < extensionSize; i++)
            {
                slotPool.Add(Instantiate(skillSlotPrefab, skillList));
            }
        }
    }
}