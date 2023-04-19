using UnityEngine;

namespace Reshape.ReGraph
{
    public struct GraphContext
    {
        public GameObject gameObject;
        public Transform transform;
        public GraphRunner runner;
        public Graph graph;
        
        public GraphContext(GraphRunner runner)
        {
            this.runner = runner;
            graph = runner.graph;
            gameObject = runner.gameObject;
            transform = gameObject.transform;
        }
    }
}