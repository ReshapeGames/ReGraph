using System;
using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class OutlineBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            Highlight = 10,
            Unhighlight = 11,
        }

        [SerializeField]
        [LabelText("Execution")]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(gameObject)")]
        [InlineButton("@gameObject.SetObjectValue(AssignGameObject())", "♺", ShowIf = "@gameObject.IsObjectValueType()")]
        private SceneObjectProperty gameObject = new SceneObjectProperty(SceneObject.ObjectType.GameObject);

        [SerializeField]
        [HideIf("@!IsHighlightExecution()")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(camera)")]
        [InlineButton("@camera.SetObjectValue(AssignCamera())", "♺", ShowIf = "@camera.IsObjectValueType()")]
        private SceneObjectProperty camera = new SceneObjectProperty(SceneObject.ObjectType.Camera);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@!IsHighlightExecution()")]
        private Color color = Color.black;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None || gameObject.IsNull)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Outline Behaviour node in " + context.gameObject.name);
            }
            else
            {
                if (executionType == ExecutionType.Highlight)
                {
                    if (camera.IsNull)
                    {
                        ReDebug.LogWarning("Graph Warning", "Found an empty Outline Behaviour node in " + context.gameObject.name);
                    }
                    else
                    {
                        Camera cam = (Camera)camera;
                        OutlineEffect effect = cam.gameObject.GetComponent<OutlineEffect>();
                        if (effect == null)
                        {
                            effect = cam.gameObject.AddComponent<OutlineEffect>();
                            effect.sourceCamera = cam;
                            effect.shader = GraphManager.instance.frameworkSettings.outlineShader;
                            effect.bufferShader = GraphManager.instance.frameworkSettings.outlineBufferShader;
                        }

                        effect.lineColor0 = color;

                        GameObject go = gameObject;
                        Outline outline = go.GetComponent<Outline>();
                        if (outline == null)
                        {
                            outline = go.AddComponent<Outline>();
                        }

                        outline.color = 0;
                        outline.enabled = true;
                    }
                }
                else if (executionType == ExecutionType.Unhighlight)
                {
                    GameObject go = gameObject;
                    Outline outline = go.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private bool IsHighlightExecution ()
        {
            return executionType == ExecutionType.Highlight;
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Highlight", ExecutionType.Highlight},
            {"Unhighlight", ExecutionType.Unhighlight},
        };

        public static string displayName = "Outline Trigger Node";
        public static string nodeName = "Outline";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            string desc = String.Empty;
            if (!gameObject.IsNull)
            {
                if (executionType == ExecutionType.Highlight)
                    desc = "Highlight " + gameObject.name;
                else if (executionType == ExecutionType.Unhighlight)
                    desc = "Unhighlight " + gameObject.name;
            }

            return desc;
        }
#endif
    }
}