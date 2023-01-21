using System.Collections.Generic;
using UnityEngine;
using Inventories;
using Control;
using Saving;
using Utils;
using Stats;
using Core;

namespace Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        // CONFIG

        [SerializeField] Quest[] startingQuests;

        // STATE

        List<QuestStatus> allStatuses = new List<QuestStatus>();
        List<QuestStatus> priestStatuses = new List<QuestStatus>();
        List<QuestStatus> paladinStatuses = new List<QuestStatus>();
        List<QuestStatus> hunterStatuses = new List<QuestStatus>();
        List<QuestStatus> mageStatuses = new List<QuestStatus>();

        // LIFECYCLE

        void Awake()
        {
            Quest.FillQuestLookup();
        }

        void Start()
        {
            foreach (Quest quest in startingQuests)
            {
                AddQuest(quest);
            }
        }

        // PUBLIC

        public IEnumerable<QuestStatus> GetStatuses(QuestOwner owner)
        {
            switch (owner)
            {
                case QuestOwner.Priest: return priestStatuses;
                case QuestOwner.Paladin: return paladinStatuses;
                case QuestOwner.Hunter: return hunterStatuses;
                case QuestOwner.Mage: return mageStatuses;
                default: return allStatuses;
            }
        }

        public string GetOwnerText(QuestOwner questOwner)
        {
            switch (questOwner)
            {
                case QuestOwner.Priest: return "Lorraine";
                case QuestOwner.Paladin: return "Tyrion";
                case QuestOwner.Hunter: return "Sylock";
                case QuestOwner.Mage: return "Zakhari";
                default: return "Dungeon";
            }
        }

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest)) return;

            switch (quest.GetOwner())
            {
                case QuestOwner.Priest:
                    priestStatuses.Add(new QuestStatus(quest));
                    break;
                case QuestOwner.Paladin:
                    paladinStatuses.Add(new QuestStatus(quest));
                    break;
                case QuestOwner.Hunter:
                    hunterStatuses.Add(new QuestStatus(quest));
                    break;
                case QuestOwner.Mage:
                    mageStatuses.Add(new QuestStatus(quest));
                    break;
                default:
                    allStatuses.Add(new QuestStatus(quest));
                    break;
            }
        }

        public void AddStatus(QuestStatus status)
        {
            switch (status.GetQuest().GetOwner())
            {
                case QuestOwner.Priest:
                    priestStatuses.Add(status);
                    break;
                case QuestOwner.Paladin:
                    paladinStatuses.Add(status);
                    break;
                case QuestOwner.Hunter:
                    hunterStatuses.Add(status);
                    break;
                case QuestOwner.Mage:
                    mageStatuses.Add(status);
                    break;
                default:
                    allStatuses.Add(status);
                    break;
            }
        }

        public void CompleteObjective(Quest quest, string objective)
        {
            QuestStatus status = GetQuestStatus(quest);
            if (status == null) return;

            status.CompleteObjective(objective);
            if (status.IsQuestComplete())
            {
                GiveReward(quest);
            }
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        // PRIVATE

        void GiveReward(Quest quest)
        {
            Inventory inventory = GetComponent<ControlSwitcher>().GetActivePlayer().GetComponent<Inventory>();
            
            foreach (Quest.Reward reward in quest.GetRewards())
            {
                inventory.AddToFirstEmptySlot(reward.item, reward.number, false);
            }
            inventory.UpdateInventory();

            foreach (PlayerController playerController in GetComponent<ControlSwitcher>().GetPlayers())
            {
                playerController.GetComponent<Experience>().GainExperience(quest.GetExperience());
            }
        }

        QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in GetStatuses(quest.GetOwner()))
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        bool? IPredicateEvaluator.Evaluate(string predicate, string[] parameters)
        {
            switch (predicate)
            {
                case "HasQuest": 
                    return HasQuest(Quest.GetByName(parameters[0]));
                case "HasObjective":
                    return GetQuestStatus(Quest.GetByName(parameters[0])).IsObjectiveComplete(parameters[1]);
                case "CompletedQuest":
                    return GetQuestStatus(Quest.GetByName(parameters[0])).IsQuestComplete();
            }

            return null;
        }

        object ISaveable.CaptureState()
        {
            List<object> state = new List<object>();
            foreach (QuestStatus status in allStatuses)
            {
                state.Add(status.CaptureState());
            }
            foreach (QuestStatus status in priestStatuses)
            {
                state.Add(status.CaptureState());
            }
            foreach (QuestStatus status in paladinStatuses)
            {
                state.Add(status.CaptureState());
            }
            foreach (QuestStatus status in hunterStatuses)
            {
                state.Add(status.CaptureState());
            }
            foreach (QuestStatus status in mageStatuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        void ISaveable.RestoreState(object saving)
        {
            List<object> states = saving as List<object>;
            if (states == null) return;

            allStatuses.Clear();
            priestStatuses.Clear();
            paladinStatuses.Clear();
            hunterStatuses.Clear();
            mageStatuses.Clear();

            foreach (object state in states)
            {
                AddStatus(new QuestStatus(state));
            }    
        }
    }
}