using System.Collections.Generic;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using Reshape.Unity.Editor;
using Sirenix.Utilities.Editor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class SaveBehaviourNode : BehaviourNode
    {
        private const string SAVE_VAR = "_variables";

        public enum ExecutionType
        {
            None,
            SaveVariables = 11,
            LoadVariables = 12,
        }

        public enum SaveType
        {
            None,
            Overwrite = 11,
            Append = 21
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(saveFile)")]
        [InlineProperty]
        [ShowIf("@executionType != ExecutionType.None")]
        private StringProperty saveFile;

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(password)")]
        [InlineProperty]
        [ShowIf("@executionType != ExecutionType.None")]
        private StringProperty password;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Save Method")]
        [ValueDropdown("SaveTypeChoice")]
        [ShowIf("@executionType == ExecutionType.SaveVariables")]
        private SaveType saveType = SaveType.Overwrite;

        [OnValueChanged("MarkDirty")]
        [ShowIf("@executionType != ExecutionType.None")]
        [ListDrawerSettings(OnTitleBarGUI = "DrawCreateButton")]
        public VariableScriptableObject[] variables;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Save Behaviour node in " + context.gameObject.name);
            }
            else if (executionType is ExecutionType.SaveVariables or ExecutionType.LoadVariables)
            {
                if (string.IsNullOrEmpty(saveFile) || variables == null || variables.Length <= 0)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Save Behaviour node in " + context.gameObject.name);
                }
                else if (executionType is ExecutionType.SaveVariables)
                {
                    if (saveType == SaveType.Append)
                    {
                        SaveOperation op = ReSave.Load(saveFile + SAVE_VAR, password);
                        if (!op.success)
                        {
                            ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success append save in {context.gameObject.name}.");
                        }
                        else
                        {
                            var dict = op.receivedDict;
                            for (int i = 0; i < variables.Length; i++)
                            {
                                bool found = false;
                                foreach (KeyValuePair<string, object> pair in dict)
                                {
                                    if (variables[i].name == pair.Key)
                                    {
                                        dict[pair.Key] = variables[i].GetObject();
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    dict.Add(variables[i].name, variables[i].GetObject());
                                }
                            }

                            op = ReSave.Save(saveFile + SAVE_VAR, dict, password);
                            if (!op.success)
                                ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success overwrite save in {context.gameObject.name}.");
                        }
                    }
                    else
                    {
                        var dict = new Dictionary<string, object>();
                        for (int i = 0; i < variables.Length; i++)
                            dict.Add(variables[i].name, variables[i].GetObject());
                        SaveOperation op = ReSave.Save(saveFile + SAVE_VAR, dict, password);
                        if (!op.success)
                            ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success overwrite save in {context.gameObject.name}.");
                    }
                }
                else if (executionType is ExecutionType.LoadVariables)
                {
                    SaveOperation op = ReSave.Load(saveFile + SAVE_VAR, password);
                    if (!op.success)
                    {
                        ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success load in {context.gameObject.name}.");
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> save in op.receivedDict)
                        {
                            for (int i = 0; i < variables.Length; i++)
                            {
                                if (variables[i].name == save.Key)
                                {
                                    variables[i].SetObject(save.Value);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        [Button]
        [ShowIf("showAdvanceSettings"), BoxGroup("Show Debug Info")]
        private void CheckSaveFileContent ()
        {
            if (string.IsNullOrEmpty(saveFile))
                return;
            SaveOperation op = ReSave.Load(saveFile + SAVE_VAR, password);
            if (!op.success)
            {
                ReDebug.LogWarning("Graph Warning", $"{saveFile} variables not success load.");
            }
            else
            {
                ReDebug.Log("Graph Save", $"{saveFile} save content : {op.savedString}");
            }
        }

        private static IEnumerable SaveTypeChoice = new ValueDropdownList<SaveType>()
        {
            {"Overwrite Entire Save", SaveType.Overwrite},
            {"Append The Save", SaveType.Append}
        };

        private void DrawCreateButton ()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.File))
            {
                VariableScriptableObject.OpenCreateVariableMenu(null);
            }

            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Folder))
            {
                ReEditorHelper.OpenPersistentDataPath();
            }
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Save Variables", ExecutionType.SaveVariables},
            {"Load Variables", ExecutionType.LoadVariables}
        };

        public static string displayName = "Save Behaviour Node";
        public static string nodeName = "Save";

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
            if (executionType == ExecutionType.SaveVariables && !string.IsNullOrEmpty(saveFile) && variables != null && variables.Length > 0)
            {
                return $"Save {variables.Length} variables";
            }
            else if (executionType == ExecutionType.LoadVariables && !string.IsNullOrEmpty(saveFile) && variables != null && variables.Length > 0)
            {
                return $"Load {variables.Length} variables";
            }

            return string.Empty;
        }
#endif
    }
}