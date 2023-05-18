using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reshape.ReGraph
{
    [Serializable]
    public class GraphVariables
    {
        public Dictionary<string, Node.State> state;
        public Dictionary<string, bool> started;
        public Dictionary<string, int> intList;
        public Dictionary<string, float> floatList;

        public GraphVariables ()
        {
            state = new Dictionary<string, Node.State>();
            started = new Dictionary<string, bool>();
            intList = new Dictionary<string, int>();
            floatList = new Dictionary<string, float>();
        }

        public Node.State GetState (string nodeId, Node.State defaultValue)
        {
            if (state.TryGetValue(nodeId, out Node.State outState))
                return outState;
            return defaultValue;
        }

        public void SetState (string nodeId, Node.State value)
        {
            if (!state.TryAdd(nodeId, value))
                state[nodeId] = value;
        }

        public bool GetStarted (string nodeId, bool defaultValue)
        {
            if (started.TryGetValue(nodeId, out bool outStarted))
                return outStarted;
            return defaultValue;
        }

        public void SetStarted (string nodeId, bool value)
        {
            if (!started.TryAdd(nodeId, value))
                started[nodeId] = value;
        }

        public int GetInt (string varId, int defaultValue = 0)
        {
            if (intList.TryGetValue(varId, out int outInt))
                return outInt;
            return defaultValue;
        }

        public void SetInt (string varId, int value)
        {
            if (!intList.TryAdd(varId, value))
                intList[varId] = value;
        }

        public float GetFloat (string varId, float defaultValue = 0f)
        {
            if (floatList.TryGetValue(varId, out float outFloat))
                return outFloat;
            return defaultValue;
        }

        public void SetFloat (string varId, float value)
        {
            if (!floatList.TryAdd(varId, value))
                floatList[varId] = value;
        }
    }
}