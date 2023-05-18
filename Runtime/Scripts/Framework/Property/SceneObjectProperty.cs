using System;
using Reshape.ReGraph;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    [Serializable]
    public class SceneObjectProperty : ReProperty, IClone<SceneObjectProperty>
    {
        [SerializeField]
        [HideLabel]
        [ShowIf("@type == 0")]
        [InlineButton("SwitchToVariable", "▼")]
        [OnValueChanged("MarkDirty")]
        [InlineProperty]
        [OnInspectorGUI("CheckSceneObjectDirty")]
        public SceneObject objectValue;

        [SerializeField]
        [LabelText("@objectValue.SceneObjectName()")]
        [ShowIf("@type == 1")]
        [InlineButton("SwitchToObject", "▼")]
        [OnValueChanged("MarkDirty")]
        public SceneObjectVariable variableValue;

        [HideInInspector]
        public int type = 0;

        public SceneObjectProperty () { }

        public SceneObjectProperty (SceneObject.ObjectType type)
        {
            objectValue = new SceneObject();
            objectValue.type = type;
            objectValue.showType = false;
            objectValue.showAsNodeProperty = true;
        }
        
        public SceneObjectProperty (SceneObject.ObjectType type, string name)
        {
            objectValue = new SceneObject();
            objectValue.type = type;
            objectValue.showType = false;
            objectValue.showAsNodeProperty = true;
            objectValue.displayName = name;
        }

        public SceneObjectProperty ShallowCopy ()
        {
            var copy = new SceneObjectProperty();
            copy.type = type;
            copy.objectValue = objectValue.ShallowCopy();
            copy.variableValue = variableValue;
            return copy;
        }

        public static implicit operator GameObject (SceneObjectProperty f)
        {
            if (f.type == 0)
            {
                if (f.objectValue != null)
                {
                    if (f.objectValue.IsGameObject())
                    {
                        GameObject go = null;
                        if (f.objectValue.TryGetValue(ref go))
                            return go;
                    }
                }

                return default;
            }

            if (f.variableValue == null)
                return default;
            return f.variableValue.GetGameObject();
        }
        
        public static implicit operator Material (SceneObjectProperty f)
        {
            if (f.type == 0)
            {
                if (f.objectValue != null)
                {
                    if (f.objectValue.IsMaterial())
                    {
                        Material mat = null;
                        if (f.objectValue.TryGetValue(ref mat))
                            return mat;
                    }
                }

                return default;
            }

            if (f.variableValue == null)
                return default;
            return f.variableValue.GetMaterial();
        }
        
        public static implicit operator AudioMixer (SceneObjectProperty f)
        {
            if (f.type == 0)
            {
                if (f.objectValue != null)
                {
                    if (f.objectValue.IsAudioMixer())
                    {
                        AudioMixer mixer = null;
                        if (f.objectValue.TryGetValue(ref mixer))
                            return mixer;
                    }
                }

                return default;
            }

            if (f.variableValue == null)
                return default;
            return f.variableValue.GetAudioMixer();
        }

        public static implicit operator Component (SceneObjectProperty f)
        {
            if (f.type == 0)
            {
                if (f.objectValue != null)
                {
                    if (f.objectValue.IsComponent())
                    {
                        Component comp = null;
                        if (f.objectValue.TryGetValue(ref comp))
                            return comp;
                    }
                }

                return default;
            }

            if (f.variableValue == null)
                return default;
            return f.variableValue.GetComponent();
        }

        public void SetObjectValue (Component comp)
        {
            objectValue.TrySetValue(comp);
        }

        public void SetObjectValue (GameObject go)
        {
            objectValue.TrySetValue(go);
        }
        
        public void SetObjectValue (AudioMixer mixer)
        {
            objectValue.TrySetValue(mixer);
        }
        
        public void SetObjectValue (Material mat)
        {
            objectValue.TrySetValue(mat);
        }

        public bool IsObjectValueType ()
        {
            if (type == 0)
                return true;
            return false;
        }

        public bool IsNull
        {
            get
            {
                GameObject go = null;
                if (type == 0)
                {
                    if (objectValue != null)
                    {
                        if (objectValue.IsGameObject())
                        {
                            if (objectValue.TryGetValue(ref go))
                                return false;
                        }
                        else if (objectValue.IsComponent())
                        {
                            Component comp = null;
                            if (objectValue.TryGetValue(ref comp))
                                return false;
                        }
                        else if (objectValue.IsMaterial())
                        {
                            Material mat = null;
                            if (objectValue.TryGetValue(ref mat))
                                return false;
                        }
                        else if (objectValue.IsAudioMixer())
                        {
                            AudioMixer mixer = null;
                            if (objectValue.TryGetValue(ref mixer))
                                return false;
                        }
                    }
                }
                else
                {
                    if (variableValue != null)
                    {
                        if (objectValue.IsGameObject())
                        {
                            go = variableValue.GetGameObject();
                            if (go != null)
                                return false;
                        }
                        else if (objectValue.IsComponent())
                        {
                            Component comp = variableValue.GetComponent();
                            if (comp != null)
                                return false;
                        }
                        else if (objectValue.IsMaterial())
                        {
                            Material mat = variableValue.GetMaterial();
                            if (mat != null)
                                return false;
                        }
                        else if (objectValue.IsAudioMixer())
                        {
                            AudioMixer mixer = variableValue.GetAudioMixer();
                            if (mixer != null)
                                return false;
                        }
                    }
                }

                return true;
            }
        }

        public string name
        {
            get
            {
                GameObject go = null;
                if (type == 0)
                {
                    if (objectValue != null)
                    {
                        if (objectValue.IsGameObject())
                        {
                            if (objectValue.TryGetValue(ref go))
                                return go.name;
                        }
                        else if (objectValue.IsComponent())
                        {
                            Component comp = null;
                            if (objectValue.TryGetValue(ref comp))
                                return comp.gameObject.name;
                        }
                        else if (objectValue.IsMaterial())
                        {
                            Material mat = null;
                            if (objectValue.TryGetValue(ref mat))
                                return mat.name;
                        }
                        else if (objectValue.IsAudioMixer())
                        {
                            AudioMixer mixer = null;
                            if (objectValue.TryGetValue(ref mixer))
                                return mixer.name;
                        }
                    }
                }
                else
                {
                    if (variableValue != null)
                    {
                        if (objectValue.IsGameObject())
                        {
                            go = variableValue.GetGameObject();
                            if (go != null)
                                return go.name;
                        }
                        else if (objectValue.IsComponent())
                        {
                            Component comp = variableValue.GetComponent();
                            if (comp != null)
                                return comp.gameObject.name;
                        }
                        else if (objectValue.IsMaterial())
                        {
                            Material mat = variableValue.GetMaterial();
                            if (mat != null)
                                return mat.name;
                        }
                        else if (objectValue.IsAudioMixer())
                        {
                            AudioMixer mixer = variableValue.GetAudioMixer();
                            if (mixer != null)
                                return mixer.name;
                        }
                    }
                }

                return string.Empty;
            }
        }

        public override string ToString ()
        {
            if (type == 0)
                return objectValue.ToString();
            if (variableValue != null)
                return variableValue.ToString();
            return string.Empty;
        }

#if UNITY_EDITOR
        private void CheckSceneObjectDirty ()
        {
            if (objectValue.dirty)
            {
                objectValue.dirty = false;
                dirty = true;
            }
        }

        private void MarkDirty ()
        {
            dirty = true;
        }

        private void SwitchToVariable ()
        {
            dirty = true;
            type = 1;
        }

        private void SwitchToObject ()
        {
            dirty = true;
            type = 0;
        }
#endif
    }
}