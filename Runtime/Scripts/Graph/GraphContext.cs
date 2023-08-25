using System.Collections.Generic;
using UnityEngine;

namespace Reshape.ReGraph
{
    public struct GraphContext
    {
        public GameObject gameObject;
        public Transform transform;
        public GraphRunner runner;
        public Graph graph;
        public Dictionary<string, Component> compList;
        public Dictionary<string, object> cacheList;
        
        public GraphContext(GraphRunner runner)
        {
            this.runner = runner;
            graph = runner.graph;
            gameObject = runner.gameObject;
            transform = gameObject.transform;
            compList = new Dictionary<string, Component>();
            cacheList = new Dictionary<string, object>();
        }
        
        public Component GetComp (string varId)
        {
            if (compList.ContainsKey(varId))
                if (compList.TryGetValue(varId, out Component outComp))
                    return outComp;
            return null;
        }

        public void SetComp (string varId, Component value)
        {
            if (!compList.TryAdd(varId, value))
                compList[varId] = value;
        }
        
        public object GetCache (string cacheId)
        {
            if (cacheList.ContainsKey(cacheId))
                if (cacheList.TryGetValue(cacheId, out object outCache))
                    return outCache;
            return null;
        }

        public void SetCache (string cacheId, object value)
        {
            if (!cacheList.TryAdd(cacheId, value))
                cacheList[cacheId] = value;
        }
    }
}