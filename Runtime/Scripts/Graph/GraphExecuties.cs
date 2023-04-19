using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reshape.ReGraph
{
    [Serializable]
    public class GraphExecutes
    {
        [SerializeField]
        private List<GraphExecution> executionList;

        public int Count
        {
            get
            {
                if (executionList == null)
                    return 0;
                return executionList.Count;
            }
        }
        
        public GraphExecution Add (long id, TriggerNode.Type triggerType)
        {
            if (executionList == null)
                executionList = new List<GraphExecution>();
            var execution = new GraphExecution(id, triggerType);
            executionList.Add(execution);
            return execution;
        }
        
        public void Remove (int index)
        {
            if (executionList == null)
                return;
            if (index >= executionList.Count)
                return;
            executionList.RemoveAt(index);
        }
        
        public void Remove (GraphExecution execution)
        {
            if (executionList == null || execution == null)
                return;
            for (int i = 0; i < executionList.Count; i++)
            {
                if (executionList[i].id == execution.id)
                {
                    executionList.RemoveAt(i);
                    break;
                }
            }
        }

        public GraphExecution Get (int index)
        {
            if (executionList == null)
                return null;
            if (index >= executionList.Count)
                return null;
            return executionList[index];
        }
        
        public GraphExecution Find (long id)
        {
            if (executionList == null)
                return null;
            for (int i = 0; i < executionList.Count; i++)
            {
                if (id == executionList[i].id)
                    return executionList[i];
            }
            return null;
        }
        
        public void Stop ()
        {
            if (executionList == null)
                return;
            for (int i = 0; i < executionList.Count; i++)
            {
                executionList[i].Stop();
            }
        }

        public void Clear ()
        {
            executionList.Clear();
        }
    }
}