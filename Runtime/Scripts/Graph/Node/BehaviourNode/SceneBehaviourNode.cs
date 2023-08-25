using System.Collections;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class SceneBehaviourNode : BehaviourNode
    {
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawSceneListDropdown", ExpandAllMenuItems = true)]
        private string sceneName;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Additive Load")]
        private bool additive;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                LogWarning("Found an empty Scene Behaviour node in " + context.gameObject.name);
            }
            else
            {
                if (additive)
                {
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                }
                else
                {
                    if (GraphManager.instance == null)
                        SceneManager.LoadScene(sceneName);
                    else
                        GraphManager.instance.Exit(sceneName);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private IEnumerable DrawSceneListDropdown ()
        {
            var actionNameListDropdown = new ValueDropdownList<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    actionNameListDropdown.Add(scene.path, scene.path);
            }

            return actionNameListDropdown;
        }

        public static string displayName = "Scene Behaviour Node";
        public static string nodeName = "Scene";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeMenuDisplayName ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (additive)
                    return "Add " + sceneName;
                else
                    return "Load " + sceneName;
            }

            return string.Empty;
        }
#endif
    }
}