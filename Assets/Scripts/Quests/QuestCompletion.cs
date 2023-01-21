using UnityEngine;

namespace Quests
{
    public class QuestCompletion : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Quest quest;
        [SerializeField] string objective;
        [SerializeField] bool toKill = false;

        // PUBLIC

        public Quest GetQuest()
        {
            return quest;
        }

        public bool IsTargetToKill()
        {
            return toKill;
        }

        public void CompleteObjective()
        {
            FindObjectOfType<QuestList>().CompleteObjective(quest, objective);
        }
    }
}