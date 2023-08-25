using System.Collections.Generic;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public abstract class TriggerNode : GraphNode
    {
        public enum Type
        {
            None,
            CollisionEnter = 11,
            CollisionExit = 12,
            CollisionStepIn = 13,
            CollisionStepOut = 14,
            ActionTrigger = 21,
            GameObjectSpawn = 31,
            InputPress = 61,
            InputRelease = 62,
            VariableChange = 91,
            RayAccepted = 150,
            RayMissed = 151,
            RayHit = 152,
            RayReceived = 153,
            RayLeave = 154,
            OnStart = 200,
            InventoryQuantityChange = 251,
            All = 99999
        }
        
        private string childKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(childKey))
                childKey = guid + VAR_CHILD;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (children == null) return;
            InitVariables();
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    execution.variables.SetInt(childKey + i, (int) State.Success);
                else
                    execution.variables.SetInt(childKey + i, (int) State.Running);
            }
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            if (children == null) return State.Failure;

            var stillRunning = false;
            var containFailure = false;
            for (int i = 0; i < children.Count; ++i)
            {
                var state = execution.variables.GetInt(childKey + i);
                if (state == (int) State.Running)
                {
                    var status = children[i].Update(execution, updateId);
                    execution.variables.SetInt(childKey + i, (int) status);
                    if (status == State.Failure)
                        containFailure = true;
                    else if (status == State.Running)
                        stillRunning = true;
                }
                else if (state == (int) State.Failure)
                {
                    containFailure = true;
                }
            }

            if (stillRunning)
                return State.Running;
            if (containFailure)
                return State.Failure;
            return State.Success;
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
            return State.Failure;
        }

        public string TriggerId => guid;

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
        public override string GetNodeMenuDisplayName ()
        {
            return string.Empty;
        }
#endif
    }
}