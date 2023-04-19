using System;
using Reshape.Unity;
using System.Collections;
using Reshape.ReGraph;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.AI;
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
            Camera = 1010,
            Rigidbody = 1020,
            NavMeshAgent = 1070,
            AudioSource = 1110,
            Animator = 1130,
            GraphRunner = 4900,
            Text = 5000,
            TextMesh = 5001,
            TextMeshPro = 5002
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
            if (type == ObjectType.GameObject && gameObject != null)
            {
                go = gameObject;
                return true;
            }

            return false;
        }

        public bool TryGetValue (ref Component comp)
        {
            if (type != ObjectType.None && type != ObjectType.GameObject && component != null)
            {
                comp = component;
                return true;
            }

            return false;
        }

        public bool TrySetValue (GameObject go)
        {
            if (type == ObjectType.GameObject && go != null)
            {
                gameObject = go;
                MarkDirty();
                return true;
            }

            return false;
        }

        public bool TrySetValue (Component comp)
        {
            if (type != ObjectType.None && type != ObjectType.GameObject && comp != null)
            {
                if (ComponentType() == comp.GetType())
                {
                    component = comp;
                    MarkDirty();
                    return true;
                }
                else if (comp.GetType() == typeof(TMPro.TextMeshProUGUI) && ComponentType() == typeof(TMPro.TMP_Text))
                {
                    component = (TMP_Text) comp;
                    MarkDirty();
                    return true;
                }
            }

            return false;
        }

        public override string ToString ()
        {
            if (IsGameObject() && gameObject != null)
                return gameObject.ToString();
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

        public bool IsComponent ()
        {
            if (type != ObjectType.None && type != ObjectType.GameObject)
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
            if (type == ObjectType.None)
                return "None";
            return ComponentName();
        }

        public Type ComponentType ()
        {
            if (type == ObjectType.Transform)
                return typeof(Transform);
            if (type == ObjectType.Camera)
                return typeof(Camera);
            if (type == ObjectType.Rigidbody)
                return typeof(Rigidbody);
            if (type == ObjectType.NavMeshAgent)
                return typeof(NavMeshAgent);
            if (type == ObjectType.AudioSource)
                return typeof(AudioSource);
            if (type == ObjectType.Animator)
                return typeof(Animator);
            if (type == ObjectType.GraphRunner)
                return typeof(GraphRunner);
            if (type == ObjectType.Text)
                return typeof(Text);
            if (type == ObjectType.TextMesh)
                return typeof(TextMesh);
            if (type == ObjectType.TextMeshPro)
                return typeof(TMP_Text);
            return null;
        }

        public string ComponentName ()
        {
            if (HaveDisplayName())
                return GetDisplayName();
            if (type == ObjectType.Transform)
                return "Transform";
            if (type == ObjectType.Camera)
                return "Camera";
            if (type == ObjectType.Rigidbody)
                return "Rigidbody";
            if (type == ObjectType.NavMeshAgent)
                return "NavMesh Agent";
            if (type == ObjectType.AudioSource)
                return "Audio Source";
            if (type == ObjectType.Animator)
                return "Animator";
            if (type == ObjectType.GraphRunner)
                return "Graph Runner";
            if (type == ObjectType.Text)
                return "Text";
            if (type == ObjectType.TextMesh)
                return "Text Mesh";
            if (type == ObjectType.TextMeshPro)
                return "Text Mesh Pro";
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
            {"Camera", ObjectType.Camera},
            {"Rigidbody", ObjectType.Rigidbody},
            {"NavMesh Agent", ObjectType.NavMeshAgent},
            {"Audio Source", ObjectType.AudioSource},
            {"Animator", ObjectType.Animator},
            {"Graph Runner", ObjectType.GraphRunner},
            {"Text", ObjectType.Text},
            {"Text Mesh", ObjectType.TextMesh},
            {"Text Mesh Pro", ObjectType.TextMeshPro},
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
                return type != ObjectType.None && type != ObjectType.GameObject;
            if (!Application.isPlaying)
                return false;
            if (type is ObjectType.None or ObjectType.GameObject)
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