using UnityEngine;
using Quests;

namespace Inventories
{
    [CreateAssetMenu(fileName = "Quest Item", menuName = "Items/Quest Item", order = 0)]
    public class QuestItem : InventoryItem
    {
        // CONFIG

        [SerializeField] Quest quest;

        // PUBLIC

        public Quest GetQuest()
        {
            return quest;
        }
    }
}