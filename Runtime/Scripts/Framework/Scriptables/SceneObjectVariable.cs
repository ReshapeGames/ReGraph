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
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    //[CreateAssetMenu(menuName = "Reshape/Scene Object Variable", order = 12)]
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
        
        public static implicit operator string (SceneObjectVariable s)
        {
            return s.ToString();
        }

        public override string ToString ()
        {
            return sceneObject.ToString();
        }
    }
}