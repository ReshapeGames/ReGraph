using System.Collections.Generic;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    [HideReferenceObjectPicker]
    public abstract class GraphNode : Node
    {
        public const string VAR_CHILD = "_child";
        public const string ID_SEPERATOR = ".";

        public enum ChildrenType
        {
            Single,
            Multiple,
            None
        }

        [SerializeReference]
        [ShowIf("ShowChildren"), BoxGroup("Debug Info")]
        [ReadOnly]
        [ListDrawerSettings(ListElementLabelName = "guid")]
        public List<GraphNode> children = new List<GraphNode>();

        [SerializeReference]
        [HideInInspector]
        public GraphNode parent;

        [HideInInspector]
        public GraphContext context;

        public void Abort (GraphExecution execution)
        {
            Graph.Traverse(this, (node) => { node.OnStop(execution, 0); });
        }

        public void Stop (GraphExecution execution)
        {
            Graph.Traverse(this, (node) => { node.OnStop(execution, 0); });
        }

        [HideInInspector]
        public bool drawGizmos = false;

#if UNITY_EDITOR
        public string id
        {
            get
            {
                if (runner.graph.id == 0)
                {
                    LogWarning("Found empty graph id in " + runner.gameObject.name);
                    return string.Empty;
                }

                return GenerateId(runner.graph.id);
            }
        }

        public string GenerateId (int graphId)
        {
            return graphId.ToString() + ID_SEPERATOR + ReUniqueId.GenerateId(guid);
        }

        private bool ShowChildren ()
        {
            if (GetType().ToString().Contains("RootNode"))
                return true;
            return showAdvanceSettings;
        }

        public virtual bool IsPortReachable (GraphNode node)
        {
            return true;
        }

        public abstract string GetNodeInspectorTitle ();
        public abstract string GetNodeViewTitle ();
        public abstract string GetNodeViewDescription ();
        public abstract string GetNodeMenuDisplayName ();
#endif

        public abstract ChildrenType GetChildrenType ();
        public abstract void GetChildren (ref List<GraphNode> list);
        public abstract bool IsRequireUpdate ();
        public abstract bool IsRequireInit ();
        public abstract bool IsRequireBegin ();
    }
}