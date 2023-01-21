using System.Linq;
using System.Collections.Generic;

namespace Quests
{
    public class QuestStatus
    {
        // STATE

        Quest quest;
        List<string> completedObjectives = new List<string>();

        [System.Serializable]
        class QuestStatusRecord
        {
            public string name;
            public List<string> objectives;
        }

        // CONSTRUCT

        public QuestStatus(Quest quest)
        {
            this.quest = quest;
        }

        public QuestStatus(object state)
        {
            QuestStatusRecord record = state as QuestStatusRecord;
            quest = Quest.GetByName(record.name);
            completedObjectives = record.objectives;
        }

        // PUBLIC

        public Quest GetQuest()
        {
            return quest;
        }

        public void CompleteObjective(string objective)
        {
            if (!quest.HasObjective(objective)) return;
            if (IsObjectiveComplete(objective)) return;

            completedObjectives.Add(objective);
        }

        public bool IsObjectiveComplete(string objective)
        {
            return completedObjectives.Contains(objective);
        }

        public bool IsQuestComplete()
        {
            foreach (Quest.Objective objective in quest.GetObjectives())
            {
                if (!completedObjectives.Contains(objective.reference))
                {
                    return false;
                }
            }
            return true;
        }

        public object CaptureState()
        {
            QuestStatusRecord record = new QuestStatusRecord();
            record.name = quest.GetName();
            record.objectives = completedObjectives;
            return record;
        }
    }
}