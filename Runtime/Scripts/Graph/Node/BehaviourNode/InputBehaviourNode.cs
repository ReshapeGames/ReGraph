using System;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using Reshape.ReFramework;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class InputBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            MouseRotationEnable = 10,
            MouseRotationDisable = 11,
            InputEnable = 100,
            InputDisable = 101,
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideIf("HideParamGo")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(gameObject)")]
        [InlineButton("@gameObject.SetObjectValue(AssignGameObject())", "♺", ShowIf = "@gameObject.IsObjectValueType()")]
        private SceneObjectProperty gameObject = new SceneObjectProperty(SceneObject.ObjectType.GameObject);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("@ParamVectorTwo1Name()")]
        [HideIf("HideParamVectorTwo1")]
        private Vector2 paramVectorTwo1;

#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("HideParamInputAction")]
        private InputActionAsset inputAction;
#endif

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("@ParamString1Name()")]
        [HideIf("HideParamString1")]
        [ValueDropdown("ParamString1Choice", ExpandAllMenuItems = false, AppendNextDrawer = true)]
        private string paramString1;

        [SerializeField]
        [HideIf("HideParamCameraView")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(cameraView)")]
        [InlineButton("@cameraView.SetObjectValue(AssignCamera())", "♺", ShowIf = "@cameraView.IsObjectValueType()")]
        [InfoBox("Allow object rotation base on the camera view, using world rotation if camera have not assigned")]
        private SceneObjectProperty cameraView = new SceneObjectProperty(SceneObject.ObjectType.Camera);

        protected override void OnStart (GraphExecution execution, int updateId)
        {
#if ENABLE_INPUT_SYSTEM
            if (executionType is ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Input Behaviour node in " + context.gameObject.name);
            }
            else if (executionType is ExecutionType.MouseRotationEnable)
            {

                if (gameObject.IsNull || inputAction == null || string.IsNullOrEmpty(paramString1))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Input Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    var go = (GameObject) gameObject;
                    MouseRotationController inpect = go.GetComponent<MouseRotationController>();
                    if (inpect == null)
                        inpect = go.AddComponent<MouseRotationController>();
                    inpect.Initial(paramVectorTwo1, inputAction, paramString1, (Camera)cameraView);
                }
            }
            else if (executionType is ExecutionType.MouseRotationDisable)
            {
                if (gameObject.IsNull)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Input Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    var go = (GameObject) gameObject;
                    MouseRotationController inpect = go.GetComponent<MouseRotationController>();
                    if (inpect != null)
                        inpect.Terminate();
                }
            }
            else if (executionType is ExecutionType.InputEnable)
            {
                if (inputAction == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Input Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    inputAction.Enable();
                }
            }
            else if (executionType is ExecutionType.InputDisable)
            {
                if (inputAction == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Input Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    inputAction.Disable();
                }
            }

#endif
            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private IEnumerable ParamString1Choice ()
        {
            ValueDropdownList<string> menu = new ValueDropdownList<string>();
            if (executionType is ExecutionType.MouseRotationEnable or ExecutionType.MouseRotationDisable)
            {
#if ENABLE_INPUT_SYSTEM
                if (inputAction != null)
                {
                    for (int i = 0; i < inputAction.actionMaps.Count; i++)
                    {
                        string mapName = inputAction.actionMaps[i].name;
                        for (int j = 0; j < inputAction.actionMaps[i].actions.Count; j++)
                        {
                            menu.Add(mapName + "//" + inputAction.actionMaps[i].actions[j].name, inputAction.actionMaps[i].actions[j].name);
                        }
                    }
                }
#endif
            }

            return menu;
        }

        private string ParamVectorTwo1Name ()
        {
            if (executionType is ExecutionType.MouseRotationEnable)
                return "Rotate Speed";
            return string.Empty;
        }

        private string ParamString1Name ()
        {
            if (executionType is ExecutionType.MouseRotationEnable)
                return "Input Name";
            return string.Empty;
        }

        private bool HideParamGo ()
        {
            if (executionType is ExecutionType.InputEnable)
                return true;
            if (executionType is ExecutionType.InputDisable)
                return true;
            return false;
        }

        private bool HideParamInputAction ()
        {
            if (executionType is ExecutionType.MouseRotationEnable)
                return false;
            if (executionType is ExecutionType.InputEnable)
                return false;
            if (executionType is ExecutionType.InputDisable)
                return false;
            return true;
        }

        private bool HideParamCameraView ()
        {
#if ENABLE_INPUT_SYSTEM
            if (executionType is ExecutionType.MouseRotationEnable)
                return false;
#endif
            return true;
        }

        private bool HideParamVectorTwo1 ()
        {
#if ENABLE_INPUT_SYSTEM
            if (executionType is ExecutionType.MouseRotationEnable)
                return false;
#endif
            return true;
        }

        private bool HideParamString1 ()
        {
#if ENABLE_INPUT_SYSTEM
            if (executionType is ExecutionType.MouseRotationEnable)
                return false;
#endif
            return true;
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Enable Input", ExecutionType.InputEnable},
            {"Disable Input", ExecutionType.InputDisable},
            {"Enable Mouse To Rotation", ExecutionType.MouseRotationEnable},
            {"Disable Mouse To Rotation", ExecutionType.MouseRotationDisable},
        };

        public static string displayName = "Input Behaviour Node";
        public static string nodeName = "Input";

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
#if ENABLE_INPUT_SYSTEM
            if (executionType is ExecutionType.MouseRotationEnable)
            {

                if (inputAction != null && !string.IsNullOrEmpty(paramString1) && !gameObject.IsNull)
                    return "Enable Mouse Control Rotation on " + gameObject.name;
            }
            else if (executionType is ExecutionType.MouseRotationDisable && !gameObject.IsNull)
            {
                return "Disable Mouse Control Rotation on " + gameObject.name;
            }
            else if (executionType is ExecutionType.InputEnable && inputAction != null)
            {
                return "Enable " + inputAction.name;
            }
            else if (executionType is ExecutionType.InputDisable && inputAction != null)
            {
                return "Disable " + inputAction.name;
            }
#endif
            return string.Empty;
        }
#endif
    }
}