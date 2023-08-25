using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.ReFramework;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class ListBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            Insert = 11,
            Push = 12,
            Take = 21,
            Pop = 22,
            Random = 23,
            Remove = 31,
            Clear = 32,
            GetCount = 210
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        private ExecutionType executionType;

        [SerializeField]
        [OnValueChanged("OnChangeList")]
        [InlineButton("@SceneObjectList.OpenCreateMenu(list)", "✚")]
        [OnInspectorGUI("CheckSceneObjectListDirty")]
        [ShowIf("@executionType != ExecutionType.None")]
        [InfoBox("The assigned list have not specific type!", InfoMessageType.Warning, "ShowListWarning", GUIAlwaysEnabled = true)]
        private SceneObjectList list;

        [SerializeField]
        [ListDrawerSettings(CustomAddFunction = "CreateNewListItem", Expanded = true)]
        [ShowIf("ShowAddObjects")]
        [OnInspectorGUI("OnUpdateObjects")]
        private List<SceneObjectProperty> objects;

        [SerializeField]
        [LabelText("Store To")]
        [OnValueChanged("MarkDirty")]
        [ShowIf("ShowObjectVariable")]
        [InfoBox("The assigned variable is not match type!", InfoMessageType.Warning, "ShowObjectVariableWarning", GUIAlwaysEnabled = true)]
        public SceneObjectVariable objectVariable;

        [SerializeField]
        [ShowIf("ShowNumberVariable")]
        [LabelText("Store To")]
        [InlineButton("CreateNumberVariable", "✚")]
        [OnValueChanged("MarkDirty")]
        private NumberVariable numberVariable;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            bool error = false;
            if (executionType is ExecutionType.Pop or ExecutionType.Take or ExecutionType.Random or ExecutionType.Remove)
            {
                if (list == null || list.IsNoneType() || objectVariable == null || list.type != objectVariable.sceneObject.type)
                    error = true;
            }
            else if (executionType is ExecutionType.Clear)
            {
                if (list == null || list.IsNoneType())
                    error = true;
            }
            else if (executionType is ExecutionType.GetCount)
            {
                if (list == null || list.IsNoneType() || numberVariable == null)
                    error = true;
            }
            else if (executionType is ExecutionType.Insert or ExecutionType.Push)
            {
                if (list == null || list.IsNoneType() || objects.Count == 0)
                    error = true;
                bool found = false;
                for (int i = 0; i < objects.Count; i++)
                {
                    if (!objects[i].IsNull)
                    {
                        found = true;
                    }
                    else
                    {
                        objects.RemoveAt(i);
                        i--;
                    }
                }
                if (!found)
                    error = true;
            }
            
            if (error)
            {
                LogWarning("Found an empty List Behaviour node in " + context.gameObject.name);
            }
            else if (executionType is ExecutionType.Pop )
            {
                objectVariable.SetValue((SceneObject)list.PopObject());
            }
            else if (executionType is ExecutionType.Take )
            {
                objectVariable.SetValue((SceneObject)list.TakeObject());
            }
            else if (executionType is ExecutionType.Random)
            {
                objectVariable.SetValue((SceneObject)list.RandomObject());
            }
            else if (executionType is ExecutionType.Remove)
            {
                list.RemoveObject(objectVariable);
            }
            else if (executionType is ExecutionType.Clear)
            {
                list.ClearObject();
            }
            else if (executionType is ExecutionType.GetCount)
            {
                numberVariable.SetValue(list.GetCount());
            }
            else if (executionType is ExecutionType.Insert)
            {
                list.InsertObject(objects);
            }
            else if (executionType is ExecutionType.Push)
            {
                list.PushObject(objects);
            }
            
            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private void OnChangeList ()
        {
            objects = new List<SceneObjectProperty>();
            MarkDirty();
        }

        private void CheckSceneObjectListDirty ()
        {
            string createVarPath = GraphEditorVariable.GetString(runner.gameObject.GetInstanceID().ToString(), "createVariable");
            if (!string.IsNullOrEmpty(createVarPath))
            {
                GraphEditorVariable.SetString(runner.gameObject.GetInstanceID().ToString(), "createVariable", string.Empty);
                var createVar = (SceneObjectList) AssetDatabase.LoadAssetAtPath(createVarPath, typeof(SceneObjectList));
                list = createVar;
                MarkDirty();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        private bool ShowListWarning ()
        {
            if (list != null && !list.IsNoneType())
                return false;
            return true;
        }

        private void OnUpdateObjects ()
        {
            if (objects != null)
            {
                bool found = false;
                bool save = false;
                for (int i = 0; i < objects.Count; i++)
                {
                    if (!objects[i].IsObjectValueType() && objects[i].variableValue != null)
                    {
                        if (list.type != objects[i].variableValue.sceneObject.type)
                        {
                            objects[i].variableValue = null;
                            found = true;
                            save = true;
                        }
                    }

                    if (MarkPropertyDirty(objects[i]))
                        save = true;
                }

                if (found)
                {
                    EditorApplication.delayCall += () =>
                    {
                        EditorUtility.DisplayDialog("List Behaviour Node Warning", "Found one or multiple variable have not match type!", "OK");
                    };
                }

                if (save)
                {
                    //-- Force scene dirty here due to property dirty is not working in this situation
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }

        private bool ShowAddObjects ()
        {
            if (executionType is ExecutionType.Insert or ExecutionType.Push)
                if (list != null && !list.IsNoneType())
                    return true;
            return false;
        }

        public void CreateNewListItem ()
        {
            if (list != null && !list.IsNoneType())
            {
                SceneObjectProperty so = new SceneObjectProperty(list.type);
                objects.Add(so);
                MarkDirty();
            }
        }

        private bool ShowObjectVariableWarning ()
        {
            if (objectVariable != null)
            {
                if (list.type != objectVariable.sceneObject.type)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShowObjectVariable ()
        {
            if (executionType is ExecutionType.Pop or ExecutionType.Take or ExecutionType.Remove or ExecutionType.Random)
                if (list != null && !list.IsNoneType())
                    return true;
            return false;
        }

        public bool ShowNumberVariable ()
        {
            if (executionType is ExecutionType.GetCount)
                if (list != null && !list.IsNoneType())
                    return true;
            return false;
        }

        private void CreateNumberVariable ()
        {
            numberVariable = NumberVariable.CreateNew(numberVariable);
            dirty = true;
        }

        public static string displayName = "List Behaviour Node";
        public static string nodeName = "List";

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
            return $"Logic/{nodeName}";
        }

        public override string GetNodeViewDescription ()
        {
            if (executionType is ExecutionType.Pop)
            {
                if (list != null && !list.IsNoneType() && objectVariable != null && list.type == objectVariable.sceneObject.type)
                    return "Pop a list item";
            }
            else if (executionType is ExecutionType.Take)
            {
                if (list != null && !list.IsNoneType() && objectVariable != null && list.type == objectVariable.sceneObject.type)
                    return "Take a list item";
            }
            else if (executionType is ExecutionType.Random)
            {
                if (list != null && !list.IsNoneType() && objectVariable != null && list.type == objectVariable.sceneObject.type)
                    return "Random a list item";
            }
            else if (executionType is ExecutionType.Remove)
            {
                if (list != null && !list.IsNoneType() && objectVariable != null && list.type == objectVariable.sceneObject.type)
                    return "Remove a list item";
            }
            else if (executionType is ExecutionType.Clear)
            {
                if (list != null && !list.IsNoneType())
                    return "Clear the list";
            }
            else if (executionType is ExecutionType.GetCount)
            {
                if (list != null && !list.IsNoneType() && numberVariable != null)
                    return "Get the list count";
            }
            else if (executionType is ExecutionType.Insert)
            {
                if (list != null && !list.IsNoneType() && objects.Count > 0)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (!objects[i].IsNull)
                            return "$Insert item(s) into list";
                    }
                }
            }
            else if (executionType is ExecutionType.Push)
            {
                if (list != null && !list.IsNoneType() && objects.Count > 0)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (!objects[i].IsNull)
                            return "Push item(s) into list";
                    }
                }
            }

            return string.Empty;
        }
#endif
    }
}