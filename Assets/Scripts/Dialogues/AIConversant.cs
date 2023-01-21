using UnityEngine;
using UnityEngine.Events;

namespace Dialogues
{
    public class AIConversant : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Sprite aiIcon;
        [SerializeField] string aiName;
        [SerializeField] Dialogue dialogue = null;
        [SerializeField] DialogueTrigger[] dialogueTriggers;

        [System.Serializable]
        struct DialogueTrigger
        {
            public string actionName;
            public UnityEvent unityEvent;
        }

        // PUBLIC

        public Sprite GetAIIcon()
        {
            return aiIcon;
        }

        public string GetAIName()
        {
            return aiName;
        }

        public Dialogue GetDialogue()
        {
            return dialogue;
        }

        public void TriggerEvents(string action)
        {
            foreach (DialogueTrigger trigger in dialogueTriggers)
            {
                if (trigger.actionName == action)
                {
                    trigger.unityEvent.Invoke();
                }    
            }
        }    
    }
}