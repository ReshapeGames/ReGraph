using System;
using Reshape.Unity;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Scene Object Variable", order = 13)]
    public class SceneObjectVariable : VariableScriptableObject
    {
        [SerializeField]
        [HideLabel]
        [InlineProperty]
        public SceneObject sceneObject;
        
        public GameObject GetGameObject ()
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsGameObject())
                {
                    GameObject go = null;
                    if (sceneObject.TryGetValue(ref go))
                        return go;
                }
            }
            
            return default;
        }
        
        public Material GetMaterial ()
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsMaterial())
                {
                    Material mat = null;
                    if (sceneObject.TryGetValue(ref mat))
                        return mat;
                }
            }
            
            return default;
        }
        
        public AudioMixer GetAudioMixer ()
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsAudioMixer())
                {
                    AudioMixer mixer = null;
                    if (sceneObject.TryGetValue(ref mixer))
                        return mixer;
                }
            }
            
            return default;
        }
        
        public Component GetComponent ()
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsComponent())
                {
                    Component comp = null;
                    if (sceneObject.TryGetValue(ref comp))
                        return comp;
                }
            }
            
            return default;
        }

        public void SetValue (GameObject go)
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsGameObject())
                    sceneObject.TrySetValue(go);
            }
        }
        
        public void SetValue (Component comp)
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsComponent())
                    sceneObject.TrySetValue(comp);
            }
        }
        
        public void SetValue (Material mat)
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsMaterial())
                    sceneObject.TrySetValue(mat);
            }
        }
        
        public void SetValue (AudioMixer mixer)
        {
            if (sceneObject != null)
            {
                if (sceneObject.IsAudioMixer())
                    sceneObject.TrySetValue(mixer);
            }
        }
        
        public void SetValue (SceneObject so)
        {
            if (sceneObject != null && so != null)
            {
                SceneObject tempSo = sceneObject;
                sceneObject = so.ShallowCopy();
                sceneObject.showType = tempSo.showType;
                sceneObject.showAsNodeProperty = tempSo.showAsNodeProperty;
                sceneObject.displayName = tempSo.displayName;
            }
        }
        
        public override void OnReset()
        {
            sceneObject.Reset();
            base.OnReset();
        }
        
        public static implicit operator string (SceneObjectVariable s)
        {
            return s.ToString();
        }

        public override string ToString ()
        {
            return sceneObject.ToString();
        }
        
        public override bool supportSaveLoad => false;
        
#if UNITY_EDITOR
        public static SceneObjectVariable CreateNew (SceneObjectVariable sceneObject, SceneObject.ObjectType type)
        {
            if (sceneObject != null)
            {
                bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
                if (!proceed)
                    return sceneObject;
            }

            var path = EditorUtility.SaveFilePanelInProject("Graph Variable", "New Scene Object Variable", "asset", "Select a location to create variable");
            if (path.Length == 0)
                return sceneObject;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                return sceneObject;

            SceneObjectVariable variable = ScriptableObject.CreateInstance<SceneObjectVariable>();
            variable.sceneObject = new SceneObject();
            variable.sceneObject.type = type;
            AssetDatabase.CreateAsset(variable, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return variable;
        }
        
        [InitializeOnLoad]
        public static class SceneObjectVariableResetOnPlay
        {
            static SceneObjectVariableResetOnPlay ()
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
                    string[] guids = AssetDatabase.FindAssets("t:SceneObjectVariable");
                    if (guids.Length > 0)
                    {
                        for (int i = 0; i < guids.Length; i++)
                        {
                            SceneObjectVariable list = (SceneObjectVariable) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(UnityEngine.Object));
                            if (list != null)
                            {
                                list.OnReset();
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