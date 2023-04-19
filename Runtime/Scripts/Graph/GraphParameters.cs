using System;
using Reshape.Unity;
using UnityEngine;

namespace Reshape.ReGraph
{
    [Serializable]
    public class GraphParameters
    {
        public string actionName;
        public GameObject interactedGo;
        public ReMonoBehaviour interactedMono;

        public GraphParameters ()
        {
            actionName = string.Empty;
        }
    }
}