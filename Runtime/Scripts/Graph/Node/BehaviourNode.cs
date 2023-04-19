using System.Collections.Generic;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public abstract class BehaviourNode : GraphNode
    {
        private string childKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(childKey))
                childKey = guid + VAR_CHILD;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            ExecuteStart(execution, updateId);
        }

        private void ExecuteStart (GraphExecution execution, int updateId)
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
            return ExecuteUpdate(execution, updateId);
        }

        private State ExecuteUpdate (GraphExecution execution, int updateId)
        {
            if (children == null) return State.Success;

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
            bool started = execution.variables.GetStarted(guid, false);
            if (!started)
            {
                ExecuteStart(execution, updateId);
                execution.variables.SetStarted(guid, true);
            }

            return ExecuteUpdate(execution, updateId);
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
    }
}