using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    [HideReferenceObjectPicker]
    public abstract class GraphNode : Node
    {
        public const string VAR_CHILD = "_child";
        
        public enum ChildrenType
        {
            Single,
            Multiple,
            None
        }

        [SerializeReference]
        [ShowIf("ShowChildren"), BoxGroup("Show Debug Info")]
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

        [HideInInspector]
        public bool drawGizmos = false;
        
#if UNITY_EDITOR
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
#endif

        public abstract ChildrenType GetChildrenType ();
        public abstract void GetChildren (ref List<GraphNode> list);
        public abstract bool IsRequireUpdate ();
        public abstract bool IsRequireInit ();
        public abstract bool IsRequireBegin ();
    }
}