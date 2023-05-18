using System;
using Reshape.ReGraph;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    [Serializable]
    public class StringProperty : ReProperty, IClone<StringProperty>
    {
        [SerializeField]
        [HideLabel]
        [ShowIf("@type == 0")]
        [InlineButton("SwitchToVariable", "▼")]
        [MultiLineResizable]
        [OnValueChanged("MarkDirty")]
        private string stringValue;

        [SerializeField]
        [HideLabel]
        [ShowIf("@type == 1")]
        [InlineButton("SwitchToString", "▼")]
        [InlineButton("CreateWordVariable", "✚")]
        [OnValueChanged("MarkDirty")]
        private WordVariable variableValue;

        [HideInInspector]
        public int type = 0;

        public StringProperty ShallowCopy ()
        {
            return (StringProperty) this.MemberwiseClone();
        }

        public static implicit operator string (StringProperty s)
        {
            return s.ToString();
        }

        public override string ToString ()
        {
            if (type == 0)
                return stringValue;
            if (variableValue == null)
                return string.Empty;
            return variableValue.ToString();
        }

        public void Reset ()
        {
            type = 0;
            variableValue = null;
            stringValue = string.Empty;
        }

#if UNITY_EDITOR
        private void CreateWordVariable ()
        {
            variableValue = WordVariable.CreateNew(variableValue);
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

        private void SwitchToString ()
        {
            dirty = true;
            type = 0;
        }
#endif
    }
}