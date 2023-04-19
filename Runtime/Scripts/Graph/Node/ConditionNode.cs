using System.Collections.Generic;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public abstract class ConditionNode : BehaviourNode
    {
        public const string VAR_EXECUTE = "_execute";

        private string executeKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(executeKey))
                executeKey = guid + VAR_EXECUTE;
        }

        protected void MarkExecute (GraphExecution execution, int updateId)
        {
            if (children == null) return;
            InitVariables();
            execution.variables.SetInt(executeKey, 1);
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            InitVariables();
            if (execution.variables.GetInt(executeKey) == 0) return State.Success;
            return base.OnUpdate(execution, updateId);
        }

        public abstract void MarkExecute (GraphExecution execution, int updateId, bool condition);
    }
}