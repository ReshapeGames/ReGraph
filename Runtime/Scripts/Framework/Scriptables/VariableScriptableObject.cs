using System;
using Reshape.ReGraph;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [Serializable]
    public class VariableScriptableObject : ScriptableObject
    {
        public delegate void CommonDelegate ();

        [PropertyOrder(100)]
        [Tooltip("Persistent variable will continue keep it value, while non-persistent will reset its value during any scene unloaded.")]
        [InfoBox("Scene Object not able to be persistent because object will be null after scene get unloaded.", "ShowPropertyPersistentInfo", GUIAlwaysEnabled = true, InfoMessageType = InfoMessageType.Info)]
        [DisableIf("DisablePropertyPersistent")]
        public bool persistent;

        [HideInInspector] public event CommonDelegate onEarlyChange;

        public event CommonDelegate onChange;

        public event CommonDelegate onReset;

        private bool linked;

        protected virtual void OnEarlyChanged ()
        {
            onEarlyChange?.Invoke();
        }

        protected virtual void OnChanged ()
        {
            if (!linked && !persistent)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }

            linked = true;
            onChange?.Invoke();
        }

        public virtual void OnReset ()
        {
            linked = false;
        }

        private void OnSceneUnloaded (Scene scene)
        {
            onReset?.Invoke();
        }
        
        public virtual object GetObject ()
        {
            return default;
        }
        
        public virtual void SetObject (object obj)
        {
            
        }

#if UNITY_EDITOR
        private bool ShowPropertyPersistentInfo ()
        {
            if (this is SceneObjectVariable)
                return true;
            return false;
        }

        private bool DisablePropertyPersistent ()
        {
            if (this is SceneObjectVariable)
                return true;
            return false;
        }
        
        public static void OpenCreateVariableMenu (VariableScriptableObject variable)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Word Variable"), false, CreateWordVariable, variable);
            menu.AddItem(new GUIContent("Number Variable"), false, CreateNumberVariable, variable);
            menu.ShowAsContext(); 
            
            void CreateNumberVariable (object varObj)
            {
                if (varObj != null)
                {
                    VariableScriptableObject preVar = (VariableScriptableObject)varObj;
                    var created = NumberVariable.CreateNew(preVar);
                    if (created != null && created != preVar)
                    {
                        GameObject go = (GameObject)Selection.activeObject;
                        GraphEditorVariable.SetString(go.GetInstanceID().ToString(), "createVariable", AssetDatabase.GetAssetPath(created));
                    }
                }
                else
                {
                    NumberVariable.CreateNew(null);
                }
            }
        
            void CreateWordVariable (object varObj)
            {
                if (varObj != null)
                {
                    VariableScriptableObject preVar = (VariableScriptableObject) varObj;
                    var created = WordVariable.CreateNew(preVar);
                    if (created != null && created != preVar)
                    {
                        GameObject go = (GameObject) Selection.activeObject;
                        GraphEditorVariable.SetString(go.GetInstanceID().ToString(), "createVariable", AssetDatabase.GetAssetPath(created));
                    }
                }
                else
                {
                    WordVariable.CreateNew(null);
                }
            }
        }
#endif
    }
}