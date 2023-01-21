using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Core;

namespace Dialogues
{
    public class DialogueNode : ScriptableObject
    {
        // CONFIG

        [SerializeField] bool isPlayerSpeaking = false;
        [SerializeField] string text = "(next)";
        [SerializeField] List<string> children = new List<string>();
        [SerializeField] Rect rect = new Rect(0, 0, 200, 100);

        [SerializeField] string onSelectAction = "";
        [SerializeField] Condition condition;

        // PUBLIC

        public bool IsPlayerSpeaking()
        {
            return isPlayerSpeaking;
        }

        public string GetText()
        {
            return text;
        }

        public List<string> GetChildren()
        {
            return children;
        }

        public Rect GetRect()
        {
            return rect;
        }

        public string GetSelectAction()
        {
            return onSelectAction;
        }

        public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
        {
            return condition.Check(evaluators);
        }

#if UNITY_EDITOR
        public void SetPlayerSpeaking(bool isPlayerSpeaking)
        {
            Undo.RecordObject(this, "Change Dialogue Speaker");

            this.isPlayerSpeaking = isPlayerSpeaking;
        }

        public void SetText(string text)
        {
            if (this.text != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");

                this.text = text;

                EditorUtility.SetDirty(this);
            }
        }

        public void SetPosition(Vector2 position)
        {
            Undo.RecordObject(this, "Move Dialogue Node");

            rect.position = position;

            EditorUtility.SetDirty(this);
        }

        public void AddChild(string child)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");

            children.Add(child);

            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string child)
        {
            Undo.RecordObject(this, "Add Dialogue Link");

            children.Remove(child);

            EditorUtility.SetDirty(this);
        }
#endif
    }
}