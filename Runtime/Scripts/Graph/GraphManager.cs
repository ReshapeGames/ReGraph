using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Reshape.Unity;
using Reshape.ReFramework;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using Reshape.Unity.Editor;
#endif

namespace Reshape.ReGraph
{
    [Serializable]
    public struct FlowAction
    {
        [HorizontalGroup]
        [HideLabel]
        public GraphRunner runner;

        [HorizontalGroup]
        [HideLabel]
        [ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
        public ActionNameChoice actionName;

        public static void ExecuteList (FlowAction[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].runner != null && actions[i].actionName != null)
                {
                    actions[i].runner.TriggerAction(actions[i].actionName);
                }
            }
        }

#if UNITY_EDITOR
        private static IEnumerable DrawActionNameListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }
#endif
    }

    [AddComponentMenu("")]
    [HideMonoScript]
    public class GraphManager : ReSinglonBehaviour<GraphManager>
    {
        [HideIf("@frameworkSettings!=null")]
        public FrameworkSettings frameworkSettings;

        [LabelText("Init Actions")]
        public FlowAction[] initActions;

        [LabelText("Begin Actions")]
        public FlowAction[] beginActions;

        [LabelText("Tick Actions")]
        public FlowAction[] tickActions;

        [LabelText("Uninit Actions")]
        public FlowAction[] uninitActions;

        private delegate void UpdateDelegate ();

        private UpdateDelegate updateDelegate;

        private string exitSceneName;

        public void Exit (string sceneName)
        {
            exitSceneName = sceneName;

            StartSystemUninitFlow();
            updateDelegate = UpdateUninitFlow;
            UpdateUninitFlow();
        }

        protected override void Awake ()
        {
            base.Awake();
            InitSystemFlow();
        }

        protected void Start ()
        {
            updateDelegate = UpdateSystemInit;
        }

        protected void Update ()
        {
            if (updateDelegate != null)
                updateDelegate();
        }

        private void UpdateSystemInit ()
        {
            if (IsSystemFlowInited())
            {
                StartSystemInitFlow();
                updateDelegate = UpdateInitFlow;
                UpdateInitFlow();
            }
        }

        private void UpdateInitFlow ()
        {
            UpdateSystemFlow();
            if (IsSystemInitFlowCompleted())
            {
                FlowAction.ExecuteList(initActions);
                ReDebug.Log("GraphManager", "System Init Flow Completed");
                StartSystemBeginFlow();
                updateDelegate = UpdateBeginFlow;
                UpdateBeginFlow();
            }
        }

        private void UpdateBeginFlow ()
        {
            UpdateSystemFlow();
            if (IsSystemBeginFlowCompleted())
            {
                FlowAction.ExecuteList(beginActions);
                ReDebug.Log("GraphManager", "System Begin Flow Completed");
                updateDelegate = UpdateTickFlow;
            }
        }

        private void UpdateTickFlow ()
        {
            StartSystemTickFlow();
            if (!IsSystemTickFlowCompleted())
            {
                ReDebug.LogError("GraphManager", "System Update Flow Not Completed Within A Frame");
            }

            FlowAction.ExecuteList(tickActions);
        }

        private void UpdateUninitFlow ()
        {
            UpdateSystemFlow();
            if (IsSystemUninitFlowCompleted())
            {
                FlowAction.ExecuteList(uninitActions);
                ReDebug.Log("GraphManager", "System Uninit Flow Completed");
                updateDelegate = null;
                ClearInstance();
                ClearSystemFlow();

                SceneManager.LoadScene(exitSceneName);
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Reshape/Graph Manager", false, -120)]
        public static void AddGraphManager ()
        {
            if (ReEditorHelper.IsInPrefabStage())
            {
                ReDebug.LogWarning("Not able to do add Graph Manager when you are editing a prefab!");
                EditorUtility.DisplayDialog("Graph Manager", "Not able to do add Graph Manager when you are editing a prefab!", "OK");
                return;
            }

            var manager = FindObjectOfType<GraphManager>();
            if (manager != null)
            {
                ReDebug.LogWarning("Not able to do add Graph Manager since there is already Graph Manager in the scene!");
                EditorUtility.DisplayDialog("Graph Manager", "Not able to do add Graph Manager since there is already Graph Manager in the scene!", "OK");
                return;
            }

            var settings = FrameworkSettings.GetSettings();
            GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(settings.graphManager);
            go.name = "Graph Manager";
            ReDebug.Log("Created Graph Manager GameObject!");
            EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}