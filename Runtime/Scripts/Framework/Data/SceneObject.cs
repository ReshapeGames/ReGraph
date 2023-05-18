using System;
using Reshape.Unity;
using System.Collections;
using Reshape.ReGraph;
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
    [Serializable]
    public class SceneObject : IClone<SceneObject>
    {
        public enum ObjectType
        {
            None = 0,
            GameObject = 100,
            Transform = 1000,
            RectTransform = 1003,
            Camera = 1010,
            Rigidbody = 1020,
            Rigidbody2D = 1025,
            NavMeshAgent = 1070,
            AudioSource = 1110,
            Animator = 1130,
            Light = 1150,
            ParticleSystem = 1170,
            SpriteRenderer = 1200,
            LineRenderer = 1210,
            TrailRenderer = 1220,
            MeshRenderer = 1230,
            Graphic = 1300,
            Image = 1310,
            Slider = 1320,
            CanvasGroup = 1330,
            LayoutElement = 1340,
            ScrollRect = 1350,
            GraphRunner = 4900,
            Text = 5000,
            TextMesh = 5001,
            TextMeshPro = 5002,
            TextMeshProGUI = 5003,
            TextMeshProText = 5004,
            Material = 6000,
            AudioMixer = 6010
        }

        [SerializeField]
        [ValueDropdown("ObjectTypeChoice")]
        [OnInspectorGUI("DrawComp", true)]
        [OnValueChanged("TypeChanged")]
        [ShowIf("showType")]
        [EnableIf("EnableType")]
        public ObjectType type;

        [SerializeField]
        [LabelText("Game Object")]
        [ShowIf("ShowGo")]
        [OnValueChanged("MarkDirty")]
        [EnableIf("EnableType")]
        private GameObject gameObject;
        
        [SerializeField]
        [ShowIf("ShowAudioMixer")]
        [OnValueChanged("MarkDirty")]
        [EnableIf("EnableType")]
        private AudioMixer audioMixer;
        
        [SerializeField]
        [ShowIf("ShowMaterial")]
        [OnValueChanged("MarkDirty")]
        [EnableIf("EnableType")]
        private Material material;
            
        [SerializeField]
        [HideInInspector]
        [OnValueChanged("MarkDirty")]
        public Component component;

        [HideInInspector]
        public bool showType = true;

        [HideInInspector]
        public bool showAsNodeProperty = false;

        [HideInInspector]
        public string displayName;

        [HideInInspector]
        public bool dirty;

        public SceneObject ShallowCopy ()
        {
            return (SceneObject) this.MemberwiseClone();
        }

        public bool TryGetValue (ref GameObject go)
        {
            if (IsGameObject() && gameObject != null)
            {
                go = gameObject;
                return true;
            }

            return false;
        }

        public bool TryGetValue (ref Component comp)
        {
            if (IsComponent() && component != null)
            {
                comp = component;
                return true;
            }

            return false;
        }
        
        public bool TryGetValue (ref Material mat)
        {
            if (IsMaterial() && material != null)
            {
                mat = material;
                return true;
            }

            return false;
        }
        
        public bool TryGetValue (ref AudioMixer mixer)
        {
            if (IsAudioMixer() && audioMixer != null)
            {
                mixer = audioMixer;
                return true;
            }

            return false;
        }

        public bool TrySetValue (GameObject go)
        {
            if (IsGameObject() && go != null)
            {
                gameObject = go;
                MarkDirty();
                return true;
            }

            return false;
        }

        public bool TrySetValue (Component comp)
        {
            if (IsComponent() && comp != null)
            {
                if (ComponentType() == comp.GetType())
                {
                    component = comp;
                    MarkDirty();
                    return true;
                }
                else if ((comp.GetType() == typeof(TMPro.TextMeshProUGUI) || comp.GetType() == typeof(TMPro.TextMeshPro)) && ComponentType() == typeof(TMPro.TMP_Text))
                {
                    component = (TMP_Text) comp;
                    MarkDirty();
                    return true;
                }
            }

            return false;
        }
        
        public bool TrySetValue (Material mat)
        {
            if (IsMaterial() && mat != null)
            {
                material = mat;
                MarkDirty();
                return true;
            }

            return false;
        }
        
        public bool TrySetValue (AudioMixer mixer)
        {
            if (IsAudioMixer() && mixer != null)
            {
                audioMixer = mixer;
                MarkDirty();
                return true;
            }

            return false;
        }

        public override string ToString ()
        {
            if (IsGameObject() && gameObject != null)
                return gameObject.ToString();
            if (IsMaterial() && material != null)
                return material.ToString();
            if (IsAudioMixer() && audioMixer != null)
                return audioMixer.ToString();
            if (IsComponent() && component != null)
                return component.ToString();
            return string.Empty;
        }

        public bool IsGameObject ()
        {
            if (type == ObjectType.GameObject)
                return true;
            return false;
        }
        
        public bool IsMaterial ()
        {
            if (type == ObjectType.Material)
                return true;
            return false;
        }
        
        public bool IsAudioMixer ()
        {
            if (type == ObjectType.AudioMixer)
                return true;
            return false;
        }

        public bool IsComponent ()
        {
            if (type != ObjectType.None && type != ObjectType.GameObject && type != ObjectType.Material && type != ObjectType.AudioMixer)
                return true;
            return false;
        }

        private bool HaveDisplayName ()
        {
            return !string.IsNullOrEmpty(displayName);
        }

        private string GetDisplayName ()
        {
            if (displayName.Equals("GameObjectLocation"))
                return "Location";
            return string.Empty;
        }

        public string SceneObjectName ()
        {
            if (HaveDisplayName())
                return GetDisplayName();
            if (type == ObjectType.GameObject)
                return "Game Object";
            if (type == ObjectType.Material)
                return "Material";
            if (type == ObjectType.AudioMixer)
                return "Audio Mixer";
            if (type == ObjectType.None)
                return "None";
            return ComponentName();
        }

        public Type ComponentType ()
        {
            if (type == ObjectType.Transform)
                return typeof(Transform);
            if (type == ObjectType.RectTransform)
                return typeof(RectTransform);
            if (type == ObjectType.Camera)
                return typeof(Camera);
            if (type == ObjectType.Rigidbody)
                return typeof(Rigidbody);
            if (type == ObjectType.Rigidbody2D)
                return typeof(Rigidbody2D);
            if (type == ObjectType.NavMeshAgent)
                return typeof(NavMeshAgent);
            if (type == ObjectType.AudioSource)
                return typeof(AudioSource);
            if (type == ObjectType.Animator)
                return typeof(Animator);
            if (type == ObjectType.Light)
                return typeof(Light);
            if (type == ObjectType.ParticleSystem)
                return typeof(ParticleSystem);
            if (type == ObjectType.SpriteRenderer)
                return typeof(SpriteRenderer);
            if (type == ObjectType.LineRenderer)
                return typeof(LineRenderer);
            if (type == ObjectType.TrailRenderer)
                return typeof(TrailRenderer);
            if (type == ObjectType.MeshRenderer)
                return typeof(MeshRenderer);
            if (type == ObjectType.Graphic)
                return typeof(Graphic);
            if (type == ObjectType.Image)
                return typeof(Image);
            if (type == ObjectType.Slider)
                return typeof(Slider);
            if (type == ObjectType.CanvasGroup)
                return typeof(CanvasGroup);
            if (type == ObjectType.LayoutElement)
                return typeof(LayoutElement);
            if (type == ObjectType.ScrollRect)
                return typeof(ScrollRect);
            if (type == ObjectType.GraphRunner)
                return typeof(GraphRunner);
            if (type == ObjectType.Text)
                return typeof(Text);
            if (type == ObjectType.TextMesh)
                return typeof(TextMesh);
            if (type == ObjectType.TextMeshPro)
                return typeof(TextMeshPro);
            if (type == ObjectType.TextMeshProGUI)
                return typeof(TextMeshProUGUI);
            if (type == ObjectType.TextMeshProText)
                return typeof(TMP_Text);
            return null;
        }

        public string ComponentName ()
        {
            if (HaveDisplayName())
                return GetDisplayName();
            if (type == ObjectType.Transform)
                return "Transform";
            if (type == ObjectType.RectTransform)
                return "Rect Transform";
            if (type == ObjectType.Camera)
                return "Camera";
            if (type == ObjectType.Rigidbody)
                return "Rigidbody";
            if (type == ObjectType.Rigidbody2D)
                return "Rigidbody 2D";
            if (type == ObjectType.NavMeshAgent)
                return "NavMesh Agent";
            if (type == ObjectType.AudioSource)
                return "Audio Source";
            if (type == ObjectType.Animator)
                return "Animator";
            if (type == ObjectType.Light)
                return "Light";
            if (type == ObjectType.ParticleSystem)
                return "Particle System";
            if (type == ObjectType.SpriteRenderer)
                return "Sprite Renderer";
            if (type == ObjectType.LineRenderer)
                return "Line Renderer";
            if (type == ObjectType.TrailRenderer)
                return "Trail Renderer";
            if (type == ObjectType.MeshRenderer)
                return "Mesh Renderer";
            if (type == ObjectType.Graphic)
                return "Graphic";
            if (type == ObjectType.Image)
                return "Image";
            if (type == ObjectType.Slider)
                return "Slider";
            if (type == ObjectType.CanvasGroup)
                return "Canvas Group";
            if (type == ObjectType.LayoutElement)
                return "Layout Element";
            if (type == ObjectType.ScrollRect)
                return "Scroll Rect";
            if (type == ObjectType.GraphRunner)
                return "Graph Runner";
            if (type == ObjectType.Text)
                return "Text";
            if (type == ObjectType.TextMesh)
                return "Text Mesh";
            if (type == ObjectType.TextMeshPro)
                return "Text Mesh Pro";
            if (type == ObjectType.TextMeshProGUI)
                return "Text Mesh Pro GUI";
            if (type == ObjectType.TextMeshProText)
                return "Text Mesh Pro Text";
            return string.Empty;
        }

        private void MarkDirty ()
        {
            dirty = true;
        }

#if UNITY_EDITOR
        private bool EnableType ()
        {
            if (EditorApplication.isPlaying)
                return false;
            return true;
        }

        private static IEnumerable ObjectTypeChoice = new ValueDropdownList<ObjectType>()
        {
            {"GameObject", ObjectType.GameObject},
            {"Transform", ObjectType.Transform},
            {"Rect Transform", ObjectType.RectTransform},
            {"Camera", ObjectType.Camera},
            {"Rigidbody", ObjectType.Rigidbody},
            {"Rigidbody 2D", ObjectType.Rigidbody2D},
            {"NavMesh Agent", ObjectType.NavMeshAgent},
            {"Audio Source", ObjectType.AudioSource},
            {"Animator", ObjectType.Animator},
            {"Light", ObjectType.Light},
            {"Particle System", ObjectType.ParticleSystem},
            {"Sprite Renderer", ObjectType.SpriteRenderer},
            {"Line Renderer", ObjectType.LineRenderer},
            {"Trail Renderer", ObjectType.TrailRenderer},
            {"Mesh Renderer", ObjectType.MeshRenderer},
            {"Graphic", ObjectType.Graphic},
            {"Image", ObjectType.Image},
            {"Slider", ObjectType.Slider},
            {"Canvas Group", ObjectType.CanvasGroup},
            {"Layout Element", ObjectType.LayoutElement},
            {"Scroll Rect", ObjectType.ScrollRect},
            {"Graph Runner", ObjectType.GraphRunner},
            {"Text", ObjectType.Text},
            {"Text Mesh", ObjectType.TextMesh},
            {"Text Mesh Pro", ObjectType.TextMeshPro},
            {"Text Mesh Pro GUI", ObjectType.TextMeshProGUI},
            {"Text Mesh Pro Text", ObjectType.TextMeshProText},
        };

        private void TypeChanged ()
        {
            component = null;
            MarkDirty();
        }

        public bool ShowGo ()
        {
            if (showAsNodeProperty)
                return type == ObjectType.GameObject;
            if (!Application.isPlaying)
                return false;
            return type == ObjectType.GameObject;
        }
        
        public bool ShowAudioMixer ()
        {
            if (showAsNodeProperty)
                return type == ObjectType.AudioMixer;
            if (!Application.isPlaying)
                return false;
            return type == ObjectType.AudioMixer;
        }
        
        public bool ShowMaterial ()
        {
            if (showAsNodeProperty)
                return type == ObjectType.Material;
            if (!Application.isPlaying)
                return false;
            return type == ObjectType.Material;
        }

        private void DrawComp ()
        {
            if (ShowComp())
            {
                component = (Component) EditorGUILayout.ObjectField(ComponentName(), component, ComponentType(), false);
            }
        }

        private bool ShowComp ()
        {
            if (showAsNodeProperty)
                return IsComponent();
            if (!Application.isPlaying)
                return false;
            if (!IsComponent())
                return false;
            return true;
        }

        private void DisableGUIAfter ()
        {
            GUI.enabled = false;
        }
#endif
    }
}