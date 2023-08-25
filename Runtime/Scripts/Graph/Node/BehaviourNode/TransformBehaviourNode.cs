using System;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class TransformBehaviourNode : BehaviourNode
    {
        [Serializable]
        public class VectorElement : IClone<VectorElement>
        {
            [HideLabel]
            [HorizontalGroup(width: 10)]
            [OnValueChanged("MarkDirty")]
            public bool enabled;

            [HideLabel]
            [HorizontalGroup]
            [DisableIf("DisableValue")]
            [InlineProperty]
            [OnInspectorGUI("@MarkPropertyDirty(value)")]
            public FloatProperty value;

            [HorizontalGroup]
            [DisableIf("DisableLinkedValue")]
            [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(linkedValue)")]
            public SceneObjectProperty linkedValue = new SceneObjectProperty(SceneObject.ObjectType.Transform, "HIDE");
            
            public Transform transform => (Transform) linkedValue;

            public VectorElement ShallowCopy ()
            {
                var e = new VectorElement();
                e.enabled = enabled;
                e.value = value.ShallowCopy();
                e.linkedValue = linkedValue.ShallowCopy();
                return e;
            }

#if UNITY_EDITOR
            [HideInInspector]
            public bool dirty;
            
            private void MarkDirty ()
            {
                dirty = true;
            }
            
            private bool DisableLinkedValue ()
            {
                if (!enabled)
                    return true;
                return false;
            }
            
            private bool DisableValue ()
            {
                if (!enabled)
                    return true;
                if (!linkedValue.IsEmpty)
                    return true;
                return false;
            }
            
            protected void MarkPropertyDirty (SceneObjectProperty p)
            {
                if (p.dirty)
                {
                    p.dirty = false;
                    MarkDirty();
                }
            }
            
            protected void MarkPropertyDirty (FloatProperty f)
            {
                if (f.dirty)
                {
                    f.dirty = false;
                    MarkDirty();
                }
            }
#endif
        }

        public enum ExecutionType
        {
            None,
            SetGlobalPosition = 10,
            SetLocalPosition = 11,
            AddGlobalPosition = 12,
            AddLocalPosition = 13,
            SetGlobalRotation = 50,
            SetLocalRotation = 51,
            AddGlobalRotation = 52,
            AddLocalRotation = 53,
            SetGlobalScale = 90,
            SetLocalScale = 91,
            AddGlobalScale = 92,
            AddLocalScale = 93
        }

        [SerializeField]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(transform)")]
        [InlineButton("@transform.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@transform.IsObjectValueType()")]
        private SceneObjectProperty transform = new SceneObjectProperty(SceneObject.ObjectType.Transform);

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        private ExecutionType executionType;

        [SerializeField]
        [BoxGroup("X", LabelText = "@XLabel()")]
        [HideLabel]
        [OnInspectorGUI("UpdateValueX")]
        [HideIf("@executionType==ExecutionType.None")]
        [InlineButton("@x.linkedValue.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@x.linkedValue.IsObjectValueType()")]
        private VectorElement x;

        [SerializeField]
        [BoxGroup("Y", LabelText = "@YLabel()")]
        [HideLabel]
        [OnInspectorGUI("UpdateValueY")]
        [HideIf("@executionType==ExecutionType.None")]
        [InlineButton("@y.linkedValue.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@y.linkedValue.IsObjectValueType()")]
        private VectorElement y;

        [SerializeField]
        [BoxGroup("Z", LabelText = "@ZLabel()")]
        [HideLabel]
        [OnInspectorGUI("UpdateValueZ")]
        [HideIf("@executionType==ExecutionType.None")]
        [InlineButton("@z.linkedValue.SetObjectValue(AssignComponent<UnityEngine.Transform>())", "♺", ShowIf = "@z.linkedValue.IsObjectValueType()")]
        private VectorElement z;

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (transform.IsEmpty || executionType == ExecutionType.None)
            {
                LogWarning("Found an empty Transform Behaviour node in " + context.gameObject.name);
            }
            else
            {
                var trans = (Transform) transform;
                if (executionType is ExecutionType.SetGlobalPosition or ExecutionType.AddGlobalPosition)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.position.x;
                        value.y = y.transform == null ? y.value : y.transform.position.y;
                        value.z = z.transform == null ? z.value : z.transform.position.z;
                        if (executionType is ExecutionType.SetGlobalPosition)
                            trans.SetPosition(value);
                        else if (executionType is ExecutionType.AddGlobalPosition)
                            trans.SetPosition(trans.position + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.position.x;
                            if (executionType is ExecutionType.SetGlobalPosition)
                                trans.SetPositionX(xValue);
                            else if (executionType is ExecutionType.AddGlobalPosition)
                                trans.SetPositionX(trans.position.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.position.y;
                            if (executionType is ExecutionType.SetGlobalPosition)
                                trans.SetPositionY(yValue);
                            else if (executionType is ExecutionType.AddGlobalPosition)
                                trans.SetPositionY(trans.position.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.position.z;
                            if (executionType is ExecutionType.SetGlobalPosition)
                                trans.SetPositionZ(zValue);
                            else if (executionType is ExecutionType.AddGlobalPosition)
                                trans.SetPositionZ(trans.position.z + zValue);
                        }
                    }
                }
                else if (executionType is ExecutionType.SetLocalPosition or ExecutionType.AddLocalPosition)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.localPosition.x;
                        value.y = y.transform == null ? y.value : y.transform.localPosition.y;
                        value.z = z.transform == null ? z.value : z.transform.localPosition.z;
                        if (executionType is ExecutionType.SetLocalPosition)
                            trans.SetLocalPosition(value);
                        else if (executionType is ExecutionType.AddLocalPosition)
                            trans.SetLocalPosition(trans.localPosition + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.localPosition.x;
                            if (executionType is ExecutionType.SetLocalPosition)
                                trans.SetLocalPositionX(xValue);
                            else if (executionType is ExecutionType.AddLocalPosition)
                                trans.SetLocalPositionX(trans.localPosition.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.localPosition.y;
                            if (executionType is ExecutionType.SetLocalPosition)
                                trans.SetLocalPositionY(yValue);
                            else if (executionType is ExecutionType.AddLocalPosition)
                                trans.SetLocalPositionY(trans.localPosition.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.localPosition.z;
                            if (executionType is ExecutionType.SetLocalPosition)
                                trans.SetLocalPositionZ(zValue);
                            else if (executionType is ExecutionType.AddLocalPosition)
                                trans.SetLocalPositionZ(trans.localPosition.z + zValue);
                        }
                    }
                }
                else if (executionType is ExecutionType.SetGlobalRotation or ExecutionType.AddGlobalRotation)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.eulerAngles.x;
                        value.y = y.transform == null ? y.value : y.transform.eulerAngles.y;
                        value.z = z.transform == null ? z.value : z.transform.eulerAngles.z;
                        if (executionType is ExecutionType.SetGlobalRotation)
                            trans.SetRotation(value);
                        else if (executionType is ExecutionType.AddGlobalRotation)
                            trans.SetRotation(trans.eulerAngles + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.eulerAngles.x;
                            if (executionType is ExecutionType.SetGlobalRotation)
                                trans.SetRotationX(xValue);
                            else if (executionType is ExecutionType.AddGlobalRotation)
                                trans.SetRotationX(trans.eulerAngles.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.eulerAngles.y;
                            if (executionType is ExecutionType.SetGlobalRotation)
                                trans.SetRotationY(yValue);
                            else if (executionType is ExecutionType.AddGlobalRotation)
                                trans.SetRotationY(trans.eulerAngles.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.eulerAngles.z;
                            if (executionType is ExecutionType.SetGlobalRotation)
                                trans.SetRotationZ(zValue);
                            else if (executionType is ExecutionType.AddGlobalRotation)
                                trans.SetRotationZ(trans.eulerAngles.z + zValue);
                        }
                    }
                }
                else if (executionType is ExecutionType.SetLocalRotation or ExecutionType.AddLocalRotation)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.localEulerAngles.x;
                        value.y = y.transform == null ? y.value : y.transform.localEulerAngles.y;
                        value.z = z.transform == null ? z.value : z.transform.localEulerAngles.z;
                        if (executionType is ExecutionType.SetLocalRotation)
                            trans.SetLocalRotation(value);
                        else if (executionType is ExecutionType.AddLocalRotation)
                            trans.SetLocalRotation(trans.localEulerAngles + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.localEulerAngles.x;
                            if (executionType is ExecutionType.SetLocalRotation)
                                trans.SetLocalRotationX(xValue);
                            else if (executionType is ExecutionType.AddLocalRotation)
                                trans.SetLocalRotationX(trans.localEulerAngles.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.localEulerAngles.y;
                            if (executionType is ExecutionType.SetLocalRotation)
                                trans.SetLocalRotationY(yValue);
                            else if (executionType is ExecutionType.AddLocalRotation)
                                trans.SetLocalRotationY(trans.localEulerAngles.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.localEulerAngles.z;
                            if (executionType is ExecutionType.SetLocalRotation)
                                trans.SetLocalRotationZ(zValue);
                            else if (executionType is ExecutionType.AddLocalRotation)
                                trans.SetLocalRotationZ(trans.localEulerAngles.z + zValue);
                        }
                    }
                }
                else if (executionType is ExecutionType.SetGlobalScale or ExecutionType.AddGlobalScale)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.lossyScale.x;
                        value.y = y.transform == null ? y.value : y.transform.lossyScale.y;
                        value.z = z.transform == null ? z.value : z.transform.lossyScale.z;
                        if (executionType is ExecutionType.SetGlobalScale)
                            trans.SetScale(value);
                        else if (executionType is ExecutionType.AddGlobalScale)
                            trans.SetScale(trans.lossyScale + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.lossyScale.x;
                            if (executionType is ExecutionType.SetGlobalScale)
                                trans.SetScaleX(xValue);
                            else if (executionType is ExecutionType.AddGlobalScale)
                                trans.SetScaleX(trans.lossyScale.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.lossyScale.y;
                            if (executionType is ExecutionType.SetGlobalScale)
                                trans.SetScaleY(yValue);
                            else if (executionType is ExecutionType.AddGlobalScale)
                                trans.SetScaleY(trans.lossyScale.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.lossyScale.z;
                            if (executionType is ExecutionType.SetGlobalScale)
                                trans.SetScaleZ(zValue);
                            else if (executionType is ExecutionType.AddGlobalScale)
                                trans.SetScaleZ(trans.lossyScale.y + zValue);
                        }
                    }
                }
                else if (executionType is ExecutionType.SetLocalScale or ExecutionType.AddLocalScale)
                {
                    if (x.enabled && y.enabled && z.enabled)
                    {
                        Vector3 value = Vector3.one;
                        value.x = x.transform == null ? x.value : x.transform.localScale.x;
                        value.y = y.transform == null ? y.value : y.transform.localScale.y;
                        value.z = z.transform == null ? z.value : z.transform.localScale.z;
                        if (executionType is ExecutionType.SetLocalScale)
                            trans.SetLocalScale(value);
                        else if (executionType is ExecutionType.AddLocalScale)
                            trans.SetLocalScale(trans.localScale + value);
                    }
                    else
                    {
                        if (x.enabled)
                        {
                            var xValue = x.transform == null ? x.value : x.transform.localScale.x;
                            if (executionType is ExecutionType.SetLocalScale)
                                trans.SetLocalScaleX(xValue);
                            else if (executionType is ExecutionType.AddLocalScale)
                                trans.SetLocalScaleX(trans.localScale.x + xValue);
                        }

                        if (y.enabled)
                        {
                            var yValue = y.transform == null ? y.value : y.transform.localScale.y;
                            if (executionType is ExecutionType.SetLocalScale)
                                trans.SetLocalScaleY(yValue);
                            else if (executionType is ExecutionType.AddLocalScale)
                                trans.SetLocalScaleY(trans.localScale.y + yValue);
                        }

                        if (z.enabled)
                        {
                            var zValue = z.transform == null ? z.value : z.transform.localScale.z;
                            if (executionType is ExecutionType.SetLocalScale)
                                trans.SetLocalScaleZ(zValue);
                            else if (executionType is ExecutionType.AddLocalScale)
                                trans.SetLocalScaleZ(trans.localScale.z + zValue);
                        }
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private void UpdateValueX ()
        {
            if (x.enabled)
            {
                if (x.transform != null)
                {
                    var value = 0f;
                    if (executionType == ExecutionType.SetGlobalPosition)
                    {
                        value = x.transform.position.x;
                    }
                    else if (executionType == ExecutionType.SetLocalPosition)
                    {
                        value = x.transform.localPosition.x;
                    }
                    else if (executionType == ExecutionType.SetGlobalRotation)
                    {
                        value = x.transform.eulerAngles.x;
                    }
                    else if (executionType == ExecutionType.SetLocalRotation)
                    {
                        value = x.transform.localEulerAngles.x;
                    }
                    else if (executionType == ExecutionType.SetGlobalScale)
                    {
                        value = x.transform.lossyScale.x;
                    }
                    else if (executionType == ExecutionType.SetLocalScale)
                    {
                        value = x.transform.localScale.x;
                    }

                    x.value = new FloatProperty(value);
                }
            }

            if (x.dirty)
            {
                x.dirty = false;
                MarkDirty();
            }
        }

        private void UpdateValueY ()
        {
            if (y.enabled)
            {
                if (y.transform != null)
                {
                    var value = 0f;
                    if (executionType == ExecutionType.SetGlobalPosition)
                    {
                        value = y.transform.position.y;
                    }
                    else if (executionType == ExecutionType.SetLocalPosition)
                    {
                        value = y.transform.localPosition.y;
                    }
                    else if (executionType == ExecutionType.SetGlobalRotation)
                    {
                        value = y.transform.eulerAngles.y;
                    }
                    else if (executionType == ExecutionType.SetLocalRotation)
                    {
                        value = y.transform.localEulerAngles.y;
                    }
                    else if (executionType == ExecutionType.SetGlobalScale)
                    {
                        value = y.transform.lossyScale.y;
                    }
                    else if (executionType == ExecutionType.SetLocalScale)
                    {
                        value = y.transform.localScale.y;
                    }

                    y.value = new FloatProperty(value);
                }
            }

            if (y.dirty)
            {
                y.dirty = false;
                MarkDirty();
            }
        }

        private void UpdateValueZ ()
        {
            if (z.enabled)
            {
                if (z.transform != null)
                {
                    var value = 0f;
                    if (executionType == ExecutionType.SetGlobalPosition)
                    {
                        value = z.transform.position.z;
                    }
                    else if (executionType == ExecutionType.SetLocalPosition)
                    {
                        value = z.transform.localPosition.z;
                    }
                    else if (executionType == ExecutionType.SetGlobalRotation)
                    {
                        value = z.transform.eulerAngles.z;
                    }
                    else if (executionType == ExecutionType.SetLocalRotation)
                    {
                        value = z.transform.localEulerAngles.z;
                    }
                    else if (executionType == ExecutionType.SetGlobalScale)
                    {
                        value = z.transform.lossyScale.z;
                    }
                    else if (executionType == ExecutionType.SetLocalScale)
                    {
                        value = z.transform.localScale.z;
                    }

                    z.value = new FloatProperty(value);
                }
            }

            if (z.dirty)
            {
                z.dirty = false;
                MarkDirty();
            }
        }

        private string XLabel ()
        {
            switch (executionType)
            {
                case ExecutionType.AddGlobalPosition:
                case ExecutionType.AddGlobalRotation:
                case ExecutionType.AddGlobalScale:
                case ExecutionType.AddLocalPosition:
                case ExecutionType.AddLocalRotation:
                case ExecutionType.AddLocalScale:
                    return "+ X";
            }

            return "X";
        }

        private string YLabel ()
        {
            switch (executionType)
            {
                case ExecutionType.AddGlobalPosition:
                case ExecutionType.AddGlobalRotation:
                case ExecutionType.AddGlobalScale:
                case ExecutionType.AddLocalPosition:
                case ExecutionType.AddLocalRotation:
                case ExecutionType.AddLocalScale:
                    return "+ Y";
            }

            return "Y";
        }

        private string ZLabel ()
        {
            switch (executionType)
            {
                case ExecutionType.AddGlobalPosition:
                case ExecutionType.AddGlobalRotation:
                case ExecutionType.AddGlobalScale:
                case ExecutionType.AddLocalPosition:
                case ExecutionType.AddLocalRotation:
                case ExecutionType.AddLocalScale:
                    return "+ Z";
            }

            return "Z";
        }

        public static string displayName = "Transform Behaviour Node";
        public static string nodeName = "Transform";

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
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (!transform.IsEmpty)
            {
                string message = "Set ";
                switch (executionType)
                {
                    case ExecutionType.AddGlobalPosition:
                    case ExecutionType.AddGlobalRotation:
                    case ExecutionType.AddGlobalScale:
                    case ExecutionType.AddLocalPosition:
                    case ExecutionType.AddLocalRotation:
                    case ExecutionType.AddLocalScale:
                        message = "Add ";
                        break;
                }

                if (executionType is ExecutionType.SetGlobalPosition or ExecutionType.AddGlobalPosition)
                {
                    message += $"{transform.name}'s global position to ";
                }
                else if (executionType is ExecutionType.SetLocalPosition or ExecutionType.AddLocalPosition)
                {
                    message += $"{transform.name}'s local position to ";
                }
                else if (executionType is ExecutionType.SetGlobalRotation or ExecutionType.AddGlobalRotation)
                {
                    message += $"{transform.name}'s global rotation to ";
                }
                else if (executionType is ExecutionType.SetLocalRotation or ExecutionType.AddLocalRotation)
                {
                    message += $"{transform.name}'s local rotation to ";
                }
                else if (executionType is ExecutionType.SetGlobalScale or ExecutionType.AddGlobalScale)
                {
                    message += $"{transform.name}'s global scale to ";
                }
                else if (executionType is ExecutionType.SetLocalScale or ExecutionType.AddLocalScale)
                {
                    message += $"{transform.name}'s local scale to ";
                }

                if (!x.enabled)
                    message += "-,";
                else if (x.transform != null)
                    message += "Obj,";
                else
                    message += x.value + ",";
                if (!y.enabled)
                    message += "-,";
                else if (y.transform != null)
                    message += "Obj,";
                else
                    message += y.value + ",";
                if (!z.enabled)
                    message += "-";
                else if (z.transform != null)
                    message += "Obj";
                else
                    message += z.value;
                return message;
            }

            return string.Empty;
        }
#endif
    }
}