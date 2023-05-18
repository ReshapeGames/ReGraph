using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class SceneObjectDrawer : OdinValueDrawer<SceneObject>
    {
        protected override void DrawPropertyLayout (GUIContent label)
        {
            SceneObject value = this.ValueEntry.SmartValue;
            if (!value.showType && !value.ShowGo() && !value.ShowMaterial() && !value.ShowAudioMixer() && value.showAsNodeProperty)
            {
                Component temp = value.component;
                var fieldLabel = value.ComponentName();
                if (fieldLabel.Equals("HIDE", StringComparison.InvariantCulture))
                    value.component = (Component) EditorGUILayout.ObjectField(value.component, value.ComponentType(), true);
                else
                    value.component = (Component) EditorGUILayout.ObjectField(fieldLabel, value.component, value.ComponentType(), true);
                if (temp != value.component)
                {
                    value.dirty = true;
                }
            }
            
            this.ValueEntry.SmartValue = value;

            base.CallNextDrawer(label);
        }
    }
}