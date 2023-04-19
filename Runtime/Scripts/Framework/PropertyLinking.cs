using Reshape.ReGraph;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class PropertyLinking
    {
        public static T GetComponent<T>(GraphRunner runner)
        {
            return runner.GetComponent<T>();
        }
        
        public static GameObject GetGameObject(GraphRunner runner)
        {
            return runner.gameObject;
        }

        public static Camera GetCamera ()
        {
            return Camera.main;
        }
    }
}