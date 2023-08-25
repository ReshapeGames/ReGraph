using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.ReGraph;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Scene Object List", order = 14)]
    [Serializable]
    public class SceneObjectList : ScriptableObject
    {
        [DisableIf("@type != SceneObject.ObjectType.None")]
        public SceneObject.ObjectType type;

        [ReadOnly]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, Expanded = true)]
        public List<SceneObjectProperty> objects;

        public void InsertObject (List<SceneObjectProperty> addon)
        {
            if (addon == null || objects == null) return;
            for (int i = 0; i < addon.Count; i++)
            {
                objects.Add(addon[i]);
            }
        }

        public void PushObject (List<SceneObjectProperty> addon)
        {
            if (addon == null || objects == null) return;
            for (int i = addon.Count - 1; i >= 0; i--)
            {
                objects.Insert(0, addon[i]);
            }
        }

        public SceneObjectProperty TakeObject ()
        {
            if (objects == null) return null;
            return objects[^1];
        }

        public SceneObjectProperty PopObject ()
        {
            if (objects == null) return null;
            return objects[0];
        }

        public SceneObjectProperty RandomObject ()
        {
            if (objects == null) return null;
            return objects[Random.Range(0, objects.Count)];
        }

        public void ClearObject ()
        {
            if (objects != null)
                objects.Clear();
        }

        public int GetCount ()
        {
            if (objects == null) return 0;
            return objects.Count;
        }

        public bool RemoveObject (SceneObjectProperty obj)
        {
            if (objects == null || obj == null) return false;
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Equals(obj))
                {
                    objects.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveObject (SceneObjectVariable obj)
        {
            if (objects == null || obj == null) return false;
            for (int i = 0; i < objects.Count; i++)
            {
                SceneObject so = objects[i];
                if (so.Equals(obj.sceneObject))
                {
                    objects.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        
        public bool IsNoneType ()
        {
            if (type == SceneObject.ObjectType.None)
                return true;
            return false;
        }

#if UNITY_EDITOR
        public static void OpenCreateMenu (SceneObjectList variable)
        {
            var menu = new GenericMenu();
            IEnumerable objectChoices = SceneObject.ObjectTypeChoice;
            var choices = (ValueDropdownList<SceneObject.ObjectType>) objectChoices;
            foreach (ValueDropdownItem<SceneObject.ObjectType> choice in choices)
                menu.AddItem(new GUIContent(choice.Text), false, CreateSceneObjectList, choice.Value);
            menu.ShowAsContext();

            void CreateSceneObjectList (object varObj)
            {
                SceneObject.ObjectType type = (SceneObject.ObjectType) varObj;
                if (type != SceneObject.ObjectType.None)
                {
                    var created = CreateNew(variable, type);
                    if (created != null && created != variable)
                        SetGraphEditorVariable(created);
                }
            }
        }

        private static void SetGraphEditorVariable (SceneObjectList created)
        {
            GameObject go = (GameObject) Selection.activeObject;
            GraphEditorVariable.SetString(go.GetInstanceID().ToString(), "createVariable", AssetDatabase.GetAssetPath(created));
        }

        public static SceneObjectList CreateNew (SceneObjectList variable, SceneObject.ObjectType type)
        {
            if (variable == null)
                return CreateNew(type);

            bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
            if (proceed)
            {
                var list = CreateNew(type);
                if (list != null)
                    return list;
            }

            return variable;
        }

        public static SceneObjectList CreateNew (SceneObject.ObjectType type)
        {
            var path = EditorUtility.SaveFilePanelInProject("Graph Variable", "New Scene Object List", "asset", "Select a location to create variable");
            if (path.Length == 0)
                return null;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                return null;

            SceneObjectList list = ScriptableObject.CreateInstance<SceneObjectList>();
            list.type = type;
            AssetDatabase.CreateAsset(list, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return list;
        }

        [InitializeOnLoad]
        public static class SceneObjectListResetOnPlay
        {
            static SceneObjectListResetOnPlay ()
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
            }

            private static void OnPlayModeChanged (PlayModeStateChange state)
            {
                bool update = false;
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                        update = true;
                }
                else if (state == PlayModeStateChange.EnteredEditMode)
                {
                    if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        update = true;
                }

                if (update)
                {
                    string[] guids = AssetDatabase.FindAssets("t:SceneObjectList");
                    if (guids.Length > 0)
                    {
                        for (int i = 0; i < guids.Length; i++)
                        {
                            SceneObjectList list = (SceneObjectList) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(UnityEngine.Object));
                            if (list != null)
                            {
                                list.ClearObject();
                            }
                        }

                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
#endif
    }
}