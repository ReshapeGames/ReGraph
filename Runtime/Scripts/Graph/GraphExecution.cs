using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [Serializable]
    public class GraphExecution
    {
        [LabelText("@lastExecutedUpdateId")]
        public Node.State state;

        [HideLabel, FoldoutGroup("Variables")]
        public GraphVariables variables;

        [HideLabel, FoldoutGroup("Parameters")]
        public GraphParameters parameters;

        [HideInInspector]
        public int lastExecutedUpdateId;

        private long executionId;
        private TriggerNode.Type triggerType;

        public long id => executionId;

        [ShowInInspector]
        [PropertyOrder(-1)]
        [LabelText("@executionId")]
        public TriggerNode.Type type => triggerType;

        public bool isFailed => state == Node.State.Failure;

        public GraphExecution (long id, TriggerNode.Type type)
        {
            executionId = id;
            triggerType = type;
            variables = new GraphVariables();
            parameters = new GraphParameters();
            state = Node.State.Running;
        }

        public void Stop ()
        {
            state = Node.State.Stop;
        }
    }
}