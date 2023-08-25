using System;
using System.Globalization;
using Reshape.ReGraph;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    [Serializable]
    public class FloatProperty : ReProperty, IClone<FloatProperty>
    {
        [SerializeField]
        [HideLabel]
        [ShowIf("@type == 0")]
        [InlineButton("SwitchToVariable", "▼")]
        [OnValueChanged("MarkDirty")]
        private float floatValue;

        [SerializeField]
        [HideLabel]
        [ShowIf("@type == 1")]
        [InlineButton("SwitchToFloat", "▼")]
        [InlineButton("CreateNumberVariable", "✚")]
        [OnValueChanged("MarkDirty")]
        private NumberVariable variableValue;

        [HideInInspector]
        public int type = 0;

        public FloatProperty ShallowCopy ()
        {
            return (FloatProperty) this.MemberwiseClone();
        }

        public static implicit operator float (FloatProperty f)
        {
            if (f.type == 0)
                return f.floatValue;
            if (f.variableValue == null)
                return 0;
            return f.variableValue;
        }

        public static implicit operator int (FloatProperty f)
        {
            if (f.type == 0)
                return (int) f.floatValue;
            if (f.variableValue == null)
                return 0;
            return (int) f.variableValue;
        }


        public static explicit operator FloatProperty (float f) => new FloatProperty(f);

        public FloatProperty (float f)
        {
            type = 0;
            variableValue = null;
            floatValue = f;
        }

        public override string ToString ()
        {
            if (type == 0)
                return floatValue.DisplayString();
            if (variableValue == null)
                return "0";
            return variableValue.ToString();
        }

        public void Reset ()
        {
            type = 0;
            variableValue = null;
            floatValue = 0;
        }

#if UNITY_EDITOR
        private void CreateNumberVariable ()
        {
            variableValue = NumberVariable.CreateNew(variableValue);
            dirty = true;
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

        private void SwitchToFloat ()
        {
            dirty = true;
            type = 0;
        }
#endif
    }
}