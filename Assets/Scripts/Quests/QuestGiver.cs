using UnityEngine;

namespace Quests
{
    public class QuestGiver : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Quest quest;

        // PUBLIC

        public void GiveQuest()
        {
            FindObjectOfType<QuestList>().AddQuest(quest);
        }
    }
}