using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Control;

namespace Dialogues
{
    public class PlayerConversant : MonoBehaviour
    {
        // CACHE

        Dialogue currentDialogue = null;
        DialogueNode currentNode = null;
        AIConversant currentConversant = null;

        // CACHE

        ControlSwitcher controlSwitcher;

        // STATE

        public event Action onDialogueStart;

        Quaternion playerRotation = new Quaternion();
        Quaternion conversantRotation = new Quaternion();

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = GetComponentInParent<ControlSwitcher>();
        }

        // PUBLIC

        public void StartDialogue(Dialogue dialogue, AIConversant conversant)
        {
            currentConversant = conversant;

            controlSwitcher.DisableLeaderPositionUpdate();

            conversantRotation = currentConversant.transform.rotation;
            playerRotation = transform.rotation;
            currentConversant.transform.LookAt(transform);
            transform.LookAt(currentConversant.transform);

            currentDialogue = dialogue;
            currentNode = currentDialogue.GetRootNode();

            if (onDialogueStart != null)
            {
                onDialogueStart();
            }
        }

        public void QuitDialogue()
        {
            currentConversant.transform.rotation = conversantRotation;
            transform.rotation = playerRotation;

            currentConversant = null;
            currentDialogue = null;
            currentNode = null;
        }

        public string GetAIText()
        {
            return currentNode.GetText();
        }

        public IEnumerable<DialogueNode> GetChoices()
        {
            return FilterChoices(currentDialogue.GetAllChildren(currentNode));
        }

        public void SelectChoice(DialogueNode chosenNode)
        {
            currentNode = currentDialogue.GetAllChildren(chosenNode).First();
        }

        public bool HasNext(DialogueNode chosenNode)
        {
            return currentDialogue.GetAllChildren(chosenNode).Count() > 0;
        }

        public void TriggerSelectAction(DialogueNode node)
        {
            currentConversant.TriggerEvents(node.GetSelectAction());
        }

        public Sprite GetAIIcon()
        {
            return currentConversant.GetAIIcon();
        }

        public string GetAIName()
        {
            return currentConversant.GetAIName();
        }

        public Sprite GetPlayerIcon()
        {
            return GetComponent<AIConversant>().GetAIIcon();
        }

        // PRIVATE

        IEnumerable<DialogueNode> FilterChoices(IEnumerable<DialogueNode> input)
        {
            foreach (DialogueNode node in input)
            {
                if (node.CheckCondition(GetEvaluators()))
                {
                    yield return node;
                }
            }
        }

        IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return controlSwitcher.GetComponents<IPredicateEvaluator>();
        }
    }
}