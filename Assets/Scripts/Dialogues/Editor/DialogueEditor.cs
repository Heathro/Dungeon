using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Stats;

namespace Dialogues.Editor
{
    public class DialogueEditor : EditorWindow
    {
        // CACHE

        Dialogue selectedDialogue = null;
        [NonSerialized] DialogueNode draggingNode = null;
        [NonSerialized] DialogueNode creatingNode = null;
        [NonSerialized] DialogueNode deletingNode = null;
        [NonSerialized] DialogueNode linkingParentNode = null;

        // STATE

        [NonSerialized] GUIStyle nodeStyle = null;
        [NonSerialized] bool draggingCanvas = false;
        [NonSerialized] Vector2 draggingCanvasOffset;
        Vector2 draggingNodeOffset;
        Vector2 scrollPosition;
        Texture2D texture;
        
        const float bezierCurveFraction = 0.8f;
        const float canvasSize = 4000f;
        const float textureSize = 50f;

        // PUBLIC

        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Dialogue dialog = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if (dialog != null)
            {
                ShowEditorWindow();
                return true;
            }    
            return false;
        }

        // LIFECYCLE

        void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;

            nodeStyle = new GUIStyle();
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
            SwitchNodeColor(false);

            texture = Resources.Load("background") as Texture2D;
        }

        void OnGUI()
        {
            if (selectedDialogue == null)
            {
                EditorGUILayout.LabelField("No Dialogue Selected");
            }
            else
            {
                ProcessEvents();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
                Rect tile = new Rect(0, 0, canvasSize / textureSize, canvasSize / textureSize);
                GUI.DrawTextureWithTexCoords(canvas, texture, tile);

                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if (creatingNode != null)
                {
                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }

                if (deletingNode != null)
                {
                    selectedDialogue.DeleteNode(deletingNode);
                    deletingNode = null;
                }
            }
        }

        // PRIVATE

        void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if (draggingNode != null)
                {
                    draggingNodeOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = draggingNode;
                }
                else
                {
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    Selection.activeObject = selectedDialogue;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                draggingNode.SetPosition(Event.current.mousePosition + draggingNodeOffset);
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }
        }

        void DrawNode(DialogueNode node)
        {
            SwitchNodeColor(node.IsPlayerSpeaking());

            GUILayout.BeginArea(node.GetRect(), nodeStyle);

            node.SetText(EditorGUILayout.TextField(node.GetText()));

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("-"))
            {
                deletingNode = node;
            }

            DrawLinkButtons(node);

            if (GUILayout.Button("+"))
            {
                creatingNode = node;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        void DrawLinkButtons(DialogueNode node)
        {
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("link"))
                {
                    linkingParentNode = node;
                }
            }
            else if (linkingParentNode == node)
            {
                if (GUILayout.Button("cancel"))
                {
                    linkingParentNode = null;
                }
            }
            else if (linkingParentNode.GetChildren().Contains(node.name))
            {
                if (GUILayout.Button("unlink"))
                {
                    linkingParentNode.RemoveChild(node.name);
                    linkingParentNode = null;
                }
            }
            else
            {
                if (GUILayout.Button("child"))
                {
                    linkingParentNode.AddChild(node.name);
                    linkingParentNode = null;
                }
            }
        }

        void DrawConnections(DialogueNode parentNode)
        {
            Vector3 start = new Vector2(parentNode.GetRect().xMax, parentNode.GetRect().center.y);

            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(parentNode))
            {
                Vector3 end = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
                Vector3 offset = end - start;
                offset.y = 0;
                offset.x *= bezierCurveFraction;

                Handles.DrawBezier(start, end, start + offset, end - offset, Color.white, null, 4f);
            }
        }

        DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null;

            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if (node.GetRect().Contains(point))
                {
                    foundNode = node;
                }
            }

            return foundNode;
        }

        void OnSelectionChanged()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                selectedDialogue = newDialogue;
                Repaint();
            }
        }

        void SwitchNodeColor(bool isPlayerSpeaking)
        {
            if (isPlayerSpeaking)
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node3") as Texture2D;
            }
            else
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            }
        }

        //void PrepareNodeStyle(CharacterClass style)
        //{
        //    nodeStyle = new GUIStyle();
        //    nodeStyle.padding = new RectOffset(20, 20, 20, 20);
        //    nodeStyle.border = new RectOffset(12, 12, 12, 12);
            
        //    switch (style)
        //    {
        //        case CharacterClass.Priest:
        //            nodeStyle.normal.background = EditorGUIUtility.Load("node3") as Texture2D;
        //            break;
        //        case CharacterClass.Paladin:
        //            nodeStyle.normal.background = EditorGUIUtility.Load("node5") as Texture2D;
        //            break;
        //        case CharacterClass.Hunter:
        //            nodeStyle.normal.background = EditorGUIUtility.Load("node4") as Texture2D;
        //            break;
        //        case CharacterClass.Mage:
        //            nodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        //            break;
        //        default:
        //            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
        //            break;
        //    }
        //}
    }
}