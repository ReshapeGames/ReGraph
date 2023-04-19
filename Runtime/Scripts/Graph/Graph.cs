using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
#endif

namespace Reshape.ReGraph
{
    [Serializable]
    public class Graph
    {
        public enum GraphType
        {
            None,
            BehaviourGraph = 10
        }

        [SerializeField]
        [ValueDropdown("TypeChoice")]
        [DisableIf("@Created || IsApplicationPlaying()")]
        [HideIf("HavePreviewNode")]
        private GraphType type;

        [SerializeReference]
        [HideInInspector]
        private RootNode rootNode;

        [SerializeReference]
        [HideIf("HideNodesList")]
        [DisableIf("@HavePreviewNode() == false")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, Expanded = false, ShowIndexLabels = true, ListElementLabelName = "GetNodeInspectorTitle")]
        public List<GraphNode> nodes = new List<GraphNode>();

        [SerializeField, ReadOnly, HideLabel]
        [HideIf("@IsApplicationPlaying() == false")]
        private GraphExecutes executes;

        private GraphContext context;
        private bool terminated;

#if UNITY_EDITOR
        [HideInInspector]
        public Vector3 viewPosition = new Vector3(300, 200);

        [HideInInspector]
        public Vector3 viewScale = Vector3.one;

        [SerializeReference]
        [HideIf("HidePreviewNode")]
        [HideDuplicateReferenceBox]
        [HideLabel]
        [BoxGroup("PreviewNode", GroupName = "@GetPreviewNodeName()")]
        public GraphNode previewNode;

        [HideInInspector]
        public bool previewSelected;

        [ShowIfGroup("SelectionWarning", Condition = "ShowSelectionWarning")]
        [BoxGroup("SelectionWarning/Hide", ShowLabel = false)]
        [HideLabel]
        [DisplayAsString]
        public string multipleSelectionWarning = "Multiple Dialog Tree selected!";

        [ShowIfGroup("PrefabWarning", Condition = "ShowPrefabWarning")]
        [BoxGroup("PrefabWarning/Hide", ShowLabel = false)]
        [HideLabel]
        [DisplayAsString]
        [PropertyOrder(-1)]
        [InfoBox("Prefab Mode Warning", InfoMessageType.Warning)]
        public string editInPrefabModeWarning;

        [HideInInspector]
        public List<ISelectable> selectedViewNode;

        private static IEnumerable TypeChoice = new ValueDropdownList<GraphType>()
        {
            {"Behaviour Graph", GraphType.BehaviourGraph},
        };

        [ShowIf("@Created == false && !IsApplicationPlaying()")]
        [Button]
        public void CreateGraph ()
        {
            if (type == GraphType.None)
            {
                EditorUtility.DisplayDialog("Create Graph", "Please select a graph type before click on Create Graph button", "OK");
            }
            else
            {
                var found = Object.FindObjectOfType<GraphManager>();
                if (found == null)
                {
                    EditorUtility.DisplayDialog("Create Graph", "Please add Graph Manager to your scene before click on Create Graph button", "OK");
                }
                else
                {
                    Create();
                }
            }
        }

        private bool ShowPrefabWarning ()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                editInPrefabModeWarning = "When editing graph in Prefab mode, make sure not drag any GameObject / Component from scene!";
                return true;
            }

            editInPrefabModeWarning = string.Empty;
            return false;
        }

        public bool IsApplicationPlaying ()
        {
            return EditorApplication.isPlaying;
        }

        public bool HavePreviewNode ()
        {
            return previewSelected;
        }

        public bool ShowSelectionWarning ()
        {
            if (selectedViewNode != null)
            {
                if (selectedViewNode.Count > 1)
                {
                    multipleSelectionWarning = "Multiple Graph Nodes Selected!";
                    return true;
                }
                else if (selectedViewNode.Count == 1)
                {
                    if (previewSelected && previewNode == null)
                    {
                        multipleSelectionWarning = "Error Graph Nodes Selected!";
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HidePreviewNode ()
        {
            if (selectedViewNode != null && selectedViewNode.Count > 1)
                return true;
            if (previewNode == null)
                return true;
            return !previewSelected;
        }

        public bool HideNodesList ()
        {
            if (rootNode == null)
                return true;
            return HavePreviewNode();
        }

        public string GetPreviewNodeName ()
        {
            if (previewNode == null)
                return String.Empty;
            return previewNode.GetNodeInspectorTitle();
        }

        public void InitPreviewNode ()
        {
            previewNode = null;
            previewSelected = false;
        }
#endif

        public Graph ()
        {
            executes = new GraphExecutes();
        }

        public GraphNode RootNode => rootNode;

        public GraphType Type => type;

        public bool Created => rootNode != null;

        public bool Terminated => terminated;

        public void Create ()
        {
            rootNode = new RootNode();
            nodes.Add(rootNode);
        }

        public void Bind (GraphContext c)
        {
            context = c;
            Traverse(rootNode, node => { node.context = context; });
        }

        public GraphContext Context => context;

        public GraphExecution InitExecute (long id, TriggerNode.Type triggerType)
        {
            if (!Created)
                return null;
            var execution = executes.Add(id, triggerType);
            return execution;
        }

        public void RunExecute (GraphExecution execution, int updateId)
        {
            if (!Created)
                return;
            if (execution != null)
                StartExecute(execution, updateId);
        }

        public GraphExecution FindExecute (long executionId)
        {
            if (!Created)
                return null;
            return executes.Find(executionId);
        }

        public void ResumeExecute (GraphExecution execution, int updateId)
        {
            if (!Created)
                return;
            if (execution != null)
                StartExecute(execution, updateId);
        }

        public void StopExecute (GraphExecution execution, int updateId)
        {
            if (!Created)
                return;
            if (execution != null)
            {
                execution.Stop();
            }
        }

        public void StopExecutes ()
        {
            if (!Created)
                return;
            if (executes != null)
            {
                executes.Stop();
            }
        }

        public void PauseExecutes ()
        {
            if (!Created || executes == null)
                return;
            for (int i = 0; i < executes.Count; i++)
            {
                var execution = executes.Get(i);
                if (execution.state == Node.State.Running)
                    rootNode.Pause(execution);
            }
        }

        public void UnpauseExecutes ()
        {
            if (!Created || executes == null)
                return;
            for (int i = 0; i < executes.Count; i++)
            {
                var execution = executes.Get(i);
                if (execution.state == Node.State.Running)
                    rootNode.Unpause(execution);
            }
        }

        private bool StartExecute (GraphExecution execution, int updateId)
        {
            execution.state = rootNode.Update(execution, updateId);
            if (execution.state is Node.State.Failure or Node.State.Success)
                return true;
            return false;
        }

        public void Update (int updateId)
        {
            if (!Created || executes == null)
                return;
            for (int i = 0; i < executes.Count; i++)
            {
                var execution = executes.Get(i);
                if (execution.state == Node.State.Running)
                    StartExecute(execution, updateId);
            }
            for (int i = 0; i < executes.Count; i++)
            {
                var execution = executes.Get(i);
                if (execution.state is Node.State.Failure or Node.State.Success)
                {
                    executes.Remove(i);
                    i--;
                }
            }
        }

        public void Reset ()
        {
            if (executes != null)
                executes.Clear();
            Traverse(rootNode, node => { node.Reset(); });
        }

        public void Stop ()
        {
            if (executes != null)
                for (int i = 0; i < executes.Count; i++)
                    rootNode?.Abort(executes.Get(i));
            Reset();
            terminated = true;
        }

        public void Start ()
        {
            Traverse(rootNode, node => { node.Init(); });
        }

        public bool HaveRequireUpdate ()
        {
            if (!Created || executes == null)
                return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].IsRequireUpdate())
                    return true;
            }

            return false;
        }
        
        public bool HaveRequireBegin ()
        {
            if (!Created || executes == null)
                return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].IsRequireBegin())
                    return true;
            }

            return false;
        }

        public bool HaveRequireInit ()
        {
            if (!Created || executes == null)
                return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].IsRequireInit())
                    return true;
            }

            return false;
        }

        public static List<GraphNode> GetChildren (GraphNode parent)
        {
            var children = new List<GraphNode>();
            parent?.GetChildren(ref children);
            return children;
        }

        public static void Traverse (GraphNode node, Action<GraphNode> visitor)
        {
            if (node != null)
            {
                visitor.Invoke(node);
                var children = GetChildren(node);
                foreach (var n in children)
                    Traverse(n, visitor);
            }
        }
    }
}