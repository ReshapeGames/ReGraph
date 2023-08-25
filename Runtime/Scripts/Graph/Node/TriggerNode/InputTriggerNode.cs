using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class InputTriggerNode : TriggerNode
    {
        [ValueDropdown("TriggerTypeChoice")]
        [OnValueChanged("MarkDirty")]
        [InfoBox("Please import Input System at Package Manager", InfoMessageType.Warning, "@!InputSystemEnabled()")]
        public Type triggerType;
        
#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@triggerType==Type.None")]
        private InputActionAsset inputAction;

        private InputAction inputType;
#endif
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@triggerType==Type.None || InputSystemEnabled()==false")]
        [ValueDropdown("InputActionNameChoice", ExpandAllMenuItems = false, AppendNextDrawer = true)]
        private string inputName;

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            State state = execution.variables.GetState(guid, State.Running);
            if (state == State.Running)
            {
                if (execution.type is Type.InputPress or Type.InputRelease && execution.type == triggerType)
                {
                    if (execution.parameters.actionName == TriggerId)
                    {
                        execution.variables.SetState(guid, State.Success);
                        state = State.Success;
                    }
                }
                else if (execution.type == Type.All)
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
#if ENABLE_INPUT_SYSTEM
            if (inputAction != null && triggerType != Type.None)
            {
                inputType = inputAction.FindAction(inputName);
                if (inputType != null)
                {
                    inputAction.Enable();
                    if (triggerType == Type.InputPress)
                    {
                        inputType.performed += OnPressed;
                    }
                    else if (triggerType == Type.InputRelease)
                    {
                        inputType.canceled += OnReleased;
                    }
                }
                else
                {
                    LogWarning("Found an empty Input Trigger node in " + context.gameObject.name);
                }
            }
            else
            {
                LogWarning("Found an empty Input Trigger node in " + context.gameObject.name);
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnPressed (InputAction.CallbackContext callbackContext)
        {
            context.runner.TriggerInput(Type.InputPress, TriggerId);
        }
        
        private void OnReleased (InputAction.CallbackContext callbackContext)
        {
            context.runner.TriggerInput(Type.InputRelease, TriggerId);
        }
#endif
        
        protected override void OnReset ()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputType != null)
            {
                if (triggerType == Type.InputPress)
                    inputType.performed -= OnPressed;
                else if (triggerType == Type.InputRelease)
                    inputType.canceled -= OnReleased;
            }
            inputAction.Disable();
#endif
        }

        public override bool IsRequireInit ()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputAction == null || string.IsNullOrEmpty(inputName) || triggerType == Type.None)
                return false;
#endif
            return true;
        }

        private bool InputSystemEnabled ()
        {
            bool result = false;
#if ENABLE_INPUT_SYSTEM
            result = true;
#endif
            return result;
        }

#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        private IEnumerable InputActionNameChoice ()
        {
            ValueDropdownList<string> menu = new ValueDropdownList<string>();
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

            return menu;
        }
#endif

#if UNITY_EDITOR
        private IEnumerable TriggerTypeChoice ()
        {
            ValueDropdownList<Type> menu = new ValueDropdownList<Type>();
            menu.Add("Input Press", Type.InputPress);
            menu.Add("Input Release", Type.InputRelease);
            return menu;
        }

        public static string displayName = "Input Trigger Node";
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
            if (inputAction != null && !string.IsNullOrEmpty(inputName) && triggerType != Type.None)
            {
                if (triggerType == Type.InputPress)
                    return "Press " + inputName;
                if (triggerType == Type.InputRelease)
                    return "Release " + inputName;
            }
#endif
            return string.Empty;
        }
#endif
    }
}