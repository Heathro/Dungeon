using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Utils;

namespace Quests
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Quest", order = 0)]
    public class Quest : ScriptableObject
    {
        // CONFIG

        [SerializeField] QuestOwner questOwner = QuestOwner.All;
        [TextArea][SerializeField] string story;
        [SerializeField] List<Objective> objectives = new List<Objective>();
        [SerializeField] List<Reward> rewards = new List<Reward>();
        [SerializeField] float experience = 1000f;

        [System.Serializable]
        public class Objective
        {
            public string reference;
            public string description;
        }

        [System.Serializable]
        public class Reward
        {
            public int number;
            public InventoryItem item;
        }

        // STATE

        static Dictionary<string, Quest> questLookup;

        // PUBLIC

        public string GetName()
        {
            return name;
        }    

        public QuestOwner GetOwner()
        {
            return questOwner;
        }

        public string GetStory()
        {
            return story;
        }

        public IEnumerable<Objective> GetObjectives()
        {
            return objectives;
        }

        public IEnumerable<Reward> GetRewards()
        {
            return rewards;
        }

        public float GetExperience()
        {
            return experience;
        }    

        public bool HasObjective(string reference)
        {
            foreach (Objective objective in objectives)
            {
                if (objective.reference == reference)
                {
                    return true;
                }
            }
            return false;
        }

        public static void FillQuestLookup()
        {
            GetByName("");
        }

        public static Quest GetByName(string name)
        {
            if (questLookup == null)
            {
                questLookup = new Dictionary<string, Quest>();
                var questList = Resources.LoadAll<Quest>("");

                foreach (var quest in questList)
                {
                    if (questLookup.ContainsKey(quest.GetName()))
                    {
                        Debug.LogError(string.Format("Duplicate in Quest lookUp for objects: {0} and {1}", questLookup[quest.GetName()], quest));
                        continue;
                    }
                    questLookup[quest.GetName()] = quest;
                }
            }

            if (string.IsNullOrEmpty(name) || !questLookup.ContainsKey(name)) return null;

            return questLookup[name];
        }
    }
}