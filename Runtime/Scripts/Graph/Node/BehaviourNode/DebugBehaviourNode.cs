using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class DebugBehaviourNode : BehaviourNode
    {
        private const string DEBUG_PREFIX = "Graph Debug";

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(message)")]
        [InlineProperty]
        private StringProperty message;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        private bool breakPoint;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (!GraphManager.instance.frameworkSettings.skipDebugNode)
            {
                if (string.IsNullOrEmpty(message) && !breakPoint)
                {
                    ReDebug.LogWarning(DEBUG_PREFIX, "Found an invalid Debug Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    if (!string.IsNullOrEmpty(message))
                        ReDebug.Log(DEBUG_PREFIX, message);
#if UNITY_EDITOR
                    if (breakPoint)
                        EditorApplication.isPaused = true;
#endif
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        public static string displayName = "Debug Behaviour Node";
        public static string nodeName = "Debug";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (!string.IsNullOrEmpty(message))
            {
                string header = "[Log] ";
                if (breakPoint)
                    header = "[Log+Break] ";
                return header + message;
            }
            else if (breakPoint)
            {
                return "[Break]";
            }

            return string.Empty;
        }
#endif
    }
}