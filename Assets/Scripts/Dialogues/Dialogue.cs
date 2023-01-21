using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dialogues
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {
        // CONFIG

        [SerializeField] Vector2 newNodeOffset = new Vector2(250, 0);
        [SerializeField] List<DialogueNode> nodes = new List<DialogueNode>();

        // STATE

        Dictionary<string, DialogueNode> lookUp = new Dictionary<string, DialogueNode>();

        // LIFECYCLE

        void Awake()
        {
            OnValidate();
        }

        void OnValidate()
        {
            lookUp.Clear();

            foreach (DialogueNode node in GetAllNodes())
            {
                lookUp[node.name] = node;
            }
        }

        // PUBLIC

        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (string childID in parentNode.GetChildren())
            {
                if (lookUp.ContainsKey(childID))
                {   
                    yield return lookUp[childID];
                }
            }
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

#if UNITY_EDITOR
        public void CreateNode(DialogueNode parentNode)
        {
            DialogueNode newNode = MakeNode(parentNode);

            Undo.RegisterCreatedObjectUndo(newNode, "Create Dialogue Node");
            Undo.RecordObject(this, "Add Dialogue Node");

            AddNode(newNode);
        }

        public void DeleteNode(DialogueNode nodeToDelete)
        {
            Undo.RecordObject(this, "Delete Dialogue Node");

            nodes.Remove(nodeToDelete);
            foreach (DialogueNode node in GetAllNodes())
            {
                node.RemoveChild(nodeToDelete.name);
            }
            OnValidate();
            Undo.DestroyObjectImmediate(nodeToDelete);
        }
#endif

        // PRIVATE

#if UNITY_EDITOR
        DialogueNode MakeNode(DialogueNode parentNode)
        {
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = Guid.NewGuid().ToString();

            if (parentNode != null)
            {
                parentNode.AddChild(newNode.name);
                newNode.SetPlayerSpeaking(!parentNode.IsPlayerSpeaking());
                newNode.SetPosition(parentNode.GetRect().position + newNodeOffset);
            }

            return newNode;
        }

        void AddNode(DialogueNode newNode)
        {
            nodes.Add(newNode);
            OnValidate();
        }
#endif

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                AddNode(MakeNode(null));
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (DialogueNode node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }
    }
}