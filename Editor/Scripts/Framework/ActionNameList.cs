using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Action Name List", fileName = "ReshapeActionNameList", order = 101)]
    public class ActionNameList : ScriptableObject
    {
        [Searchable]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
        [ListDrawerSettings(ShowPaging = false)]
        public string[] actionName;
        
        [Button("Generate Action Name")]
        [HideIf("@this.GetInstanceID() != 23462")]
        public void GenerateActionNameChoice ()
        {
            var assets = AssetDatabase.FindAssets("t:ActionNameList");
            for (var i = 0; i < assets.Length; i++)
            {
                bool processed = false;
                var listPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(listPath))
                {
                    if (this.GetInstanceID() != obj.GetInstanceID()) continue;
                    var availableAction = new List<string>();
                    for (var j = 0; j < actionName.Length; j++)
                        if (!string.IsNullOrWhiteSpace(actionName[j]))
                            if (!availableAction.Contains(actionName[j]))
                                availableAction.Add(actionName[j]);
                    var abandonAction = new List<string>();
                    var choiceAssets = AssetDatabase.FindAssets("t:ActionNameChoice");
                    for (var j = 0; j < choiceAssets.Length; j++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(choiceAssets[j]);
                        var actionNameChoice = AssetDatabase.LoadAssetAtPath<ActionNameChoice>(path);
                        if (availableAction.Contains(actionNameChoice.actionName))
                            availableAction.Remove(actionNameChoice.actionName);
                        else
                            abandonAction.Add(path);
                    }

                    var listFolderPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(assets[i]));
#if UNITY_EDITOR_WIN
                    var choicesPath = listFolderPath + "\\ActionName\\";
#elif UNITY_EDITOR_OSX
                    var choicesPath = listFolderPath + "/ActionName/";
#endif
                    if (!Directory.Exists(choicesPath))
                        Directory.CreateDirectory(choicesPath);

                    for (var j = 0; j < availableAction.Count; j++)
                    {
                        ActionNameChoice actionNameChoice = ScriptableObject.CreateInstance<ActionNameChoice>();
                        actionNameChoice.actionName = availableAction[j];
                        AssetDatabase.CreateAsset(actionNameChoice, choicesPath + availableAction[j] + ".asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.FocusProjectWindow();
                    }

                    for (int j = 0; j < abandonAction.Count; j++)
                    {
                        var directoryPath = Path.GetDirectoryName(abandonAction[j]);
                        if (directoryPath.Length > 6)
                            directoryPath = Path.GetDirectoryName(directoryPath);
                        if (listFolderPath == directoryPath)
                            AssetDatabase.DeleteAsset(abandonAction[j]);
                    }

                    processed = true;
                    break;
                }

                if (processed)
                    break;
            }
        }
    }
}