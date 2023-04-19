using System.Collections;
using System.Collections.Generic;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class DialogBehaviourNode : BehaviourNode
    {
        public const string VAR_PROCEED = "_proceed";

        public enum ExecutionType
        {
            None,
            ShowDialog = 10,
            ShowChoices = 11,
            HideCanvas = 1000
        }

        [SerializeField]
        [OnValueChanged("OnChangeType")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [ShowIf("ShowParamActor")]
        [OnInspectorGUI("@MarkPropertyDirty(actor)")]
        [InlineProperty]
        private StringProperty actor;

        [SerializeField]
        [ShowIf("ShowParamDialog")]
        [OnInspectorGUI("@MarkPropertyDirty(dialog)")]
        [InlineProperty]
        private StringProperty dialog;

        private string proceedKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(proceedKey))
                proceedKey = guid + VAR_PROCEED;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Dialog Behaviour node in " + context.gameObject.name);
            }
            else if (executionType is ExecutionType.ShowDialog)
            {
                if (string.IsNullOrEmpty(dialog))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Dialog Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InitVariables();
                    execution.variables.SetInt(proceedKey, 0);

                    if (DialogCanvas.instance == null)
                    {
                        GameObject go = (GameObject) Object.Instantiate(GraphManager.instance.frameworkSettings.dialogCanvas);
                        go.name = GraphManager.instance.frameworkSettings.dialogCanvas.name;
                    }

                    var dialogNodes = new List<BehaviourNode>();
                    for (int i = 0; i < children.Count; ++i)
                    {
                        if (children[i] is DialogBehaviourNode)
                        {
                            var behave = (DialogBehaviourNode) children[i];
                            if (behave.executionType is ExecutionType.ShowDialog or ExecutionType.ShowChoices)
                                dialogNodes.Add(behave);
                        }
                    }

                    DialogCanvas.ShowMessagePanel(actor, dialog, dialogNodes.Count > 0);

                    if (DialogCanvas.instance != null)
                    {
                        DialogCanvas.instance.onKeyDialogProceed += OnKeyEnter;
                    }
                }
            }
            else if (executionType is ExecutionType.ShowChoices)
            {
                var choices = new List<string>();
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i] is ChoiceConditionNode)
                    {
                        var child = (ChoiceConditionNode) children[i];
                        if (!string.IsNullOrEmpty(child.choice))
                            choices.Add(child.choice);
                    }
                }

                if (choices.Count <= 0)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Dialog Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    InitVariables();
                    execution.variables.SetInt(proceedKey, 0);
                    
                    if (DialogCanvas.instance == null)
                    {
                        GameObject go = (GameObject) Object.Instantiate(GraphManager.instance.frameworkSettings.dialogCanvas);
                        go.name = GraphManager.instance.frameworkSettings.dialogCanvas.name;
                    }
                    
                    DialogCanvas.ShowChoicePanel(choices);
                }
            }
            else if (executionType == ExecutionType.HideCanvas)
            {
                if (DialogCanvas.instance == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Dialog Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    DialogCanvas.HidePanel();
                }
            }

            base.OnStart(execution, updateId);

            void OnKeyEnter ()
            {
                DialogCanvas.instance.onKeyDialogProceed -= OnKeyEnter;
                if (execution != null && execution.variables.GetInt(proceedKey) == 0)
                {
                    execution.variables.SetInt(proceedKey, 1);
                }
            }
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.ShowDialog)
            {
                int key = execution.variables.GetInt(proceedKey);
                if (key > 0)
                {
                    if (DialogCanvas.IsPanelHide())
                        return State.Failure;
                    return base.OnUpdate(execution, updateId);
                }

                return State.Running;
            }
            else if (executionType is ExecutionType.ShowChoices)
            {
                int key = execution.variables.GetInt(proceedKey);
                if (key == 0)
                {
                    string selectedChoice = DialogCanvas.GetChosenChoice();
                    bool foundChoice = false;
                    for (var i = 0; i < children.Count; ++i)
                    {
                        if (children[i] is ChoiceConditionNode)
                        {
                            var cNode = children[i] as ChoiceConditionNode;
                            if (string.Equals(cNode.choice, selectedChoice))
                            {
                                cNode.MarkExecute(execution, updateId, true);
                                foundChoice = true;
                            }
                        }
                    }

                    if (!foundChoice)
                    {
                        return State.Running;
                    }
                    else
                    {
                        DialogCanvas.HideChoicePanel();
                        execution.variables.SetInt(proceedKey, 1);
                    }
                }
            }
            
            return base.OnUpdate(execution, updateId);
        }

        protected override void OnStop (GraphExecution execution, int updateId)
        {
            /*bool started = execution.variables.GetStarted(guid, false);
            if (started)
            {
                if (executionType is ExecutionType.ShowDialog)
                {
                    if (DialogCanvas.instance != null)
                    {
                        DialogCanvas.instance.onKeyDialogProceed -= OnKeyEnter;
                    }
                }
            }*/
            base.OnStop(execution, updateId);
        }

        public override bool IsRequireUpdate ()
        {
            return enabled;
        }

#if UNITY_EDITOR
        private bool ShowParamActor ()
        {
            if (executionType is ExecutionType.ShowDialog)
                return true;
            return false;
        }

        private bool ShowParamDialog ()
        {
            if (executionType is ExecutionType.ShowDialog)
                return true;
            return false;
        }

        private void OnChangeType ()
        {
            MarkDirty();
            MarkRepaint();
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Show Dialog", ExecutionType.ShowDialog},
            {"Show Choices", ExecutionType.ShowChoices},
            {"Hide Canvas", ExecutionType.HideCanvas}
        };

        public static string displayName = "Dialog Behaviour Node";
        public static string nodeName = "Dialog";
        
        public override bool IsPortReachable (GraphNode node)
        {
            if (node is ChoiceConditionNode)
            {
                if (executionType != ExecutionType.ShowChoices)
                    return false;
            }
            else if (node is YesConditionNode or NoConditionNode)
            {
                return false;
            }
            return true;
        }
        
        public bool AcceptConditionNode ()
        {
            if (executionType == ExecutionType.ShowChoices)
                return true;
            return false;
        }

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
            if (executionType == ExecutionType.ShowDialog && !string.IsNullOrEmpty(dialog))
            {
                if (!string.IsNullOrEmpty(actor))
                    return actor + " : " + dialog + "\n<color=#FFF600>Continue at dialog end";
                return dialog;
            }

            if (executionType == ExecutionType.ShowChoices)
                return "Show Choices";
            if (executionType == ExecutionType.HideCanvas)
                return "Hide Canvas";
            return string.Empty;
        }
#endif
    }
}