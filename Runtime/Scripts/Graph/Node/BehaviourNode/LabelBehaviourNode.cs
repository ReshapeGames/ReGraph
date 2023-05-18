using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class LabelBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            StringToLabel = 10,
            VariableToLabel = 11,
            StringToTextMesh = 100,
            VariableToTextMesh = 101,
            StringToTextMeshPro = 200,
            VariableToTextMeshPro = 201
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideIf("@executionType != ExecutionType.StringToLabel && executionType != ExecutionType.VariableToLabel")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(textLabel)")]
        [InlineButton("@textLabel.SetObjectValue(AssignComponent<Text>())", "♺", ShowIf = "@textLabel.IsObjectValueType()")]
        private SceneObjectProperty textLabel = new SceneObjectProperty(SceneObject.ObjectType.Text);

        [SerializeField]
        [HideIf("@executionType != ExecutionType.StringToTextMesh && executionType != ExecutionType.VariableToTextMesh")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(textMeshLabel)")]
        [InlineButton("@textMeshLabel.SetObjectValue(AssignComponent<TextMesh>())", "♺", ShowIf = "@textMeshLabel.IsObjectValueType()")]
        private SceneObjectProperty textMeshLabel = new SceneObjectProperty(SceneObject.ObjectType.TextMesh);

        [SerializeField]
        [HideIf("@executionType != ExecutionType.StringToTextMeshPro && executionType != ExecutionType.VariableToTextMeshPro")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(textMeshProLabel)")]
        [InlineButton("@textMeshProLabel.SetObjectValue(AssignComponent<TMP_Text>())", "♺", ShowIf = "@textMeshProLabel.IsObjectValueType()")]
        private SceneObjectProperty textMeshProLabel = new SceneObjectProperty(SceneObject.ObjectType.TextMeshProText);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@executionType != ExecutionType.VariableToLabel && executionType != ExecutionType.VariableToTextMesh && executionType != ExecutionType.VariableToTextMeshPro")]
        private VariableScriptableObject variable;

        [SerializeField]
        [MultiLineProperty]
        [OnValueChanged("MarkDirty")]
        [HideIf("@executionType != ExecutionType.StringToLabel && executionType != ExecutionType.StringToTextMesh && executionType != ExecutionType.StringToTextMeshPro")]
        [LabelText("String")]
        private string message;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            bool error = false;
            if (executionType is ExecutionType.StringToLabel or ExecutionType.VariableToLabel)
            {
                if (textLabel.IsNull)
                    error = true;
            }
            else if (executionType is ExecutionType.StringToTextMesh or ExecutionType.VariableToTextMesh)
            {
                if (textMeshLabel.IsNull)
                    error = true;
            }
            else if (executionType is ExecutionType.StringToTextMeshPro or ExecutionType.VariableToTextMeshPro)
            {
                if (textMeshProLabel.IsNull)
                    error = true;
            }

            if (executionType is ExecutionType.StringToLabel or ExecutionType.StringToTextMesh or ExecutionType.StringToTextMeshPro)
            {
                if (message == null)
                    error = true;
            }
            else if (executionType is ExecutionType.VariableToLabel or ExecutionType.VariableToTextMesh or ExecutionType.VariableToTextMeshPro)
            {
                if (variable == null)
                    error = true;
            }

            if (error)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Label Behaviour node in " + context.gameObject.name);
            }
            else
            {
                string outputString = string.Empty;
                if (executionType is ExecutionType.StringToLabel or ExecutionType.StringToTextMesh or ExecutionType.StringToTextMeshPro)
                    outputString = message;
                else
                    outputString = variable.ToString();

                if (executionType is ExecutionType.StringToLabel or ExecutionType.VariableToLabel)
                    ((Text) textLabel).text = outputString;
                else if (executionType is ExecutionType.StringToTextMesh or ExecutionType.VariableToTextMesh)
                    ((TextMesh) textMeshLabel).text = outputString;
                else if (executionType is ExecutionType.StringToTextMeshPro or ExecutionType.VariableToTextMeshPro)
                    ((TMP_Text) textMeshProLabel).text = outputString;
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"String To Text", ExecutionType.StringToLabel},
            {"Variable To Text", ExecutionType.VariableToLabel},
            {"String To TextMesh", ExecutionType.StringToTextMesh},
            {"Variable To TextMesh", ExecutionType.VariableToTextMesh},
            {"String To TMPro", ExecutionType.StringToTextMeshPro},
            {"Variable To TMPro", ExecutionType.VariableToTextMeshPro}
        };

        public static string displayName = "Label Behaviour Node";
        public static string nodeName = "Label";

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
            if (executionType is ExecutionType.None)
                return string.Empty;
            if (executionType is ExecutionType.StringToLabel or ExecutionType.VariableToLabel)
                if (textLabel.IsNull)
                    return string.Empty;
            if (executionType is ExecutionType.StringToTextMesh or ExecutionType.VariableToTextMesh)
                if (textMeshLabel.IsNull)
                    return string.Empty;
            if (executionType is ExecutionType.StringToTextMeshPro or ExecutionType.VariableToTextMeshPro)
                if (textMeshProLabel.IsNull)
                    return string.Empty;

            string message = "Set variable to ";
            if (executionType is ExecutionType.StringToLabel or ExecutionType.StringToTextMesh or ExecutionType.StringToTextMeshPro)
                message = "Set string to ";
            if (executionType is ExecutionType.StringToLabel or ExecutionType.VariableToLabel)
                message += textLabel.name + " (Text)";
            else if (executionType is ExecutionType.StringToTextMesh or ExecutionType.VariableToTextMesh)
                message += textMeshLabel.name + " (Text Mesh)";
            else if (executionType is ExecutionType.StringToTextMeshPro or ExecutionType.VariableToTextMeshPro)
                message += textMeshProLabel.name + " (TMPro)";
            return message;
        }
#endif
    }
}