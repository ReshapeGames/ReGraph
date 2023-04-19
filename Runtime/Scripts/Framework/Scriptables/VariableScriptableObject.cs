using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

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
#endif
    }
}