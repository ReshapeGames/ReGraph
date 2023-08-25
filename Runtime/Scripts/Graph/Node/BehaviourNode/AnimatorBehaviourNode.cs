using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class AnimatorBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            SetBoolTrue = 10,
            SetBoolFalse = 11,
            SetTrigger = 20,
            SetFloat = 30,
            SetInt = 31
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideIf("@executionType == ExecutionType.None")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(animator)")]
        [InlineButton("@animator.SetObjectValue(AssignComponent<Animator>())", "♺", ShowIf = "@animator.IsObjectValueType()")]
        private SceneObjectProperty animator = new SceneObjectProperty(SceneObject.ObjectType.Animator);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@executionType == ExecutionType.None")]
        [ValueDropdown("ParameterChoice")]
        private int parameter;

        [SerializeField]
        [LabelText("Value")]
        [ShowIf("@executionType == ExecutionType.SetFloat || executionType == ExecutionType.SetInt")]
        [InlineProperty]
        [OnInspectorGUI("@MarkPropertyDirty(paramValue)")]
        private FloatProperty paramValue;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (animator.IsEmpty || executionType is ExecutionType.None || parameter == 0)
            {
                LogWarning("Found an empty Animator Behaviour node in " + context.gameObject.name);
            }
            else
            {
                var anim = (Animator)animator;
                if (executionType == ExecutionType.SetBoolTrue)
                {
                    anim.SetBool(parameter, true);
                }
                else if (executionType == ExecutionType.SetBoolFalse)
                {
                    anim.SetBool(parameter, false);
                }
                else if (executionType == ExecutionType.SetTrigger)
                {
                    anim.SetTrigger(parameter);
                }
                else if (executionType == ExecutionType.SetInt)
                {
                    anim.SetInteger(parameter, paramValue);
                }
                else if (executionType == ExecutionType.SetFloat)
                {
                    anim.SetFloat(parameter, paramValue);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        public ValueDropdownList<int> ParameterChoice ()
        {
            var listDropdown = new ValueDropdownList<int>();
            listDropdown.Add("Yet Select", 0);
            if (!animator.IsEmpty)
            {
                var paramList = ((Animator) animator).parameters;
                for (var i = 0; i < paramList.Length; i++)
                {
                    AnimatorControllerParameter param = paramList[i];
                    if (executionType is ExecutionType.SetBoolTrue or ExecutionType.SetBoolFalse)
                    {
                        if (param.type == AnimatorControllerParameterType.Bool)
                            listDropdown.Add(param.name, param.nameHash);
                    }
                    else if (executionType == ExecutionType.SetTrigger)
                    {
                        if (param.type == AnimatorControllerParameterType.Trigger)
                            listDropdown.Add(param.name, param.nameHash);
                    }
                    else if (executionType == ExecutionType.SetInt)
                    {
                        if (param.type == AnimatorControllerParameterType.Int)
                            listDropdown.Add(param.name, param.nameHash);
                    }
                    else if (executionType == ExecutionType.SetFloat)
                    {
                        if (param.type == AnimatorControllerParameterType.Float)
                            listDropdown.Add(param.name, param.nameHash);
                    }
                }
            }

            return listDropdown;
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Tick Bool", ExecutionType.SetBoolTrue},
            {"Untick Bool", ExecutionType.SetBoolFalse},
            {"Call Trigger", ExecutionType.SetTrigger},
            {"Set Int", ExecutionType.SetInt},
            {"Set Float", ExecutionType.SetFloat}
        };

        public static string displayName = "Animator Behaviour Node";
        public static string nodeName = "Animator";

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
            return $"Animation/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (!animator.IsEmpty && executionType is ExecutionType.None == false && parameter != 0)
            {
                string parameterName = string.Empty;
                var paramList = ((Animator) animator).parameters;
                for (var i = 0; i < paramList.Length; i++)
                {
                    AnimatorControllerParameter param = paramList[i];
                    if (param.nameHash == parameter)
                    {
                        parameterName = param.name;
                        break;
                    }
                }

                if (executionType is ExecutionType.SetBoolTrue)
                    return "Tick " + parameterName + " Bool on " + animator.name;
                if (executionType is ExecutionType.SetBoolFalse)
                    return "Untick " + parameterName + " Bool on " + animator.name;
                if (executionType is ExecutionType.SetTrigger)
                    return "Call " + parameterName + " Trigger on " + animator.name;
                if (executionType is ExecutionType.SetFloat)
                    return "Set " + paramValue + " to " + parameterName + " Float on " + animator.name;
                if (executionType is ExecutionType.SetInt)
                    return "Set " + paramValue + " to " + parameterName + " Int on " + animator.name;
            }

            return string.Empty;
        }
#endif
    }
}