using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class VariableTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        public Type triggerType;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@triggerType==Type.None")]
        [InlineButton("@VariableScriptableObject.OpenCreateVariableMenu(variable)", "✚")]
        [OnInspectorGUI("CheckVariableDirty")]
        public VariableScriptableObject variable;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if ((execution.type is Type.VariableChange && execution.type == triggerType) || execution.type == Type.All)
                {
                    if (execution.parameters.actionName != null && execution.parameters.actionName.Equals(TriggerId))
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
                    }
                }

                if (state != State.Success)
                {
                    execution.variables.SetState(guid, State.Failure);
                    state = State.Failure;
                }
            }

            if (state == State.Success)
                return base.OnUpdate(execution, updateId);
            return State.Failure;
        }

        protected override void OnInit ()
        {
            if (variable != null && triggerType == Type.VariableChange)
            {
                if (variable is WordVariable)
                {
                    var sVar = (WordVariable) variable;
                    sVar.Init();
                }
                variable.onChange += OnValueChanged;
            }
            else
            {
                LogWarning("Found an empty Variable Trigger node in " + context.gameObject.name);
            }
        }

        private void OnValueChanged ()
        {
            context.runner.TriggerVariable(Type.VariableChange, TriggerId);
        }

        protected override void OnReset ()
        {
            if (variable != null)
                variable.onChange -= OnValueChanged;
        }

        public override bool IsRequireInit ()
        {
            if (variable == null || triggerType == Type.None)
                return false;
            return true;
        }

#if UNITY_EDITOR
        private void CheckVariableDirty ()
        {
            string createVarPath = GraphEditorVariable.GetString(runner.gameObject.GetInstanceID().ToString(), "createVariable");
            if (!string.IsNullOrEmpty(createVarPath))
            {
                GraphEditorVariable.SetString(runner.gameObject.GetInstanceID().ToString(), "createVariable", string.Empty);
                var createVar = (VariableScriptableObject)AssetDatabase.LoadAssetAtPath(createVarPath, typeof(VariableScriptableObject));
                variable = createVar;
                MarkDirty();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        
        private IEnumerable TriggerTypeChoice ()
        {
            ValueDropdownList<Type> menu = new ValueDropdownList<Type>();
            menu.Add("Value Change", Type.VariableChange);
            return menu;
        }

        public static string displayName = "Variable Trigger Node";
        public static string nodeName = "Variable";

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
            if (triggerType == Type.VariableChange && variable != null)
                return variable.name + "'s value changed";
            return string.Empty;
        }
#endif
    }
}