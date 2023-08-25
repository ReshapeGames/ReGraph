using System.Collections.Generic;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class RootNode : GraphNode
    {
        public const string VAR_CURRENT = "_current";

        private string currentKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(currentKey))
                currentKey = guid + VAR_CURRENT;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            InitVariables();
            execution.variables.SetInt(currentKey, 0);
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            if (children != null)
            {
                int current = execution.variables.GetInt(currentKey);
                for (int i = current; i < children.Count; ++i)
                {
                    current = i;
                    execution.variables.SetInt(currentKey, current);
                    var child = children[current];
                    if (child != null)
                    {
                        switch (child.Update(execution, updateId))
                        {
                            case State.Running:
                                return State.Running;
                            case State.Success:
                                return State.Success;
                            case State.Failure:
                                continue;
                        }
                    }
                }
            }

            return State.Failure;
        }

        protected override void OnStop (GraphExecution execution, int updateId) { }
        protected override void OnInit () { }
        protected override void OnReset () { }

        protected override void OnPause (GraphExecution execution)
        {
            if (children != null)
                for (int i = 0; i < children.Count; ++i)
                    children[i].Pause(execution);
        }
        
        protected override void OnUnpause (GraphExecution execution)
        {
            if (children != null)
                for (int i = 0; i < children.Count; ++i)
                    children[i].Unpause(execution);
        }

        protected override State OnDisabled (GraphExecution execution, int updateId)
        {
            return OnUpdate(execution, updateId);
        }

        public override ChildrenType GetChildrenType ()
        {
            return ChildrenType.Multiple;
        }

        public override void GetChildren (ref List<GraphNode> list)
        {
            if (children != null)
                for (var i = 0; i < children.Count; i++)
                    list.Add(children[i]);
        }

        public override bool IsRequireUpdate ()
        {
            return false;
        }
        
        public override bool IsRequireInit ()
        {
            return false;
        }
        
        public override bool IsRequireBegin ()
        {
            return false;
        }

#if UNITY_EDITOR
        public static string displayName = "Start Node";
        public static string nodeName = "Start";
        public static string nodeDesc = "This is starting point of the graph";

        public override string GetNodeInspectorTitle() { return displayName; }

        public override string GetNodeViewTitle() { return nodeName; }
        
        public override string GetNodeViewDescription () { return nodeDesc; }
        
        public override string GetNodeMenuDisplayName () { return string.Empty; }
#endif
    }
}