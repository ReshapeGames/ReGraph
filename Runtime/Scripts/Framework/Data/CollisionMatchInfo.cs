using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Reshape.ReFramework
{
    [System.Serializable]
    public class CollisionMatchInfo : IClone<CollisionMatchInfo>
    {
        [OnValueChanged("MarkDirty")]
        public string[] specificNames;
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawTagDropdown", ExpandAllMenuItems = true)]
        public string[] excludeTags;
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawLayerDropdown", ExpandAllMenuItems = true)]
        public string[] excludeLayers;
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawTagDropdown", ExpandAllMenuItems = true)]
        public string[] onlyTags;
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("DrawLayerDropdown", ExpandAllMenuItems = true)]
        public string[] onlyLayers;
        [HideInInspector]
        [OnValueChanged("MarkDirty")]
        public bool inOutDetection;
        [ShowIfGroup("inOutDetection")]
        [FoldoutGroup("inOutDetection/In Out Direction")]
        [OnValueChanged("MarkDirty")]
        public bool leftToRight = true;
        [FoldoutGroup("inOutDetection/In Out Direction")]
        [OnValueChanged("MarkDirty")]
        public bool rightToLeft = true;
        [FoldoutGroup("inOutDetection/In Out Direction")]
        [OnValueChanged("MarkDirty")]
        public bool topToBottom = true;
        [FoldoutGroup("inOutDetection/In Out Direction")]
        [OnValueChanged("MarkDirty")]
        public bool bottomToTop = true;
        [HideInInspector]
        [OnValueChanged("MarkDirty")]
        public bool uniqueDetection;
        
        public CollisionMatchInfo ShallowCopy()
        {
            return (CollisionMatchInfo) this.MemberwiseClone();
        }
        
        [HideInInspector]
        public bool dirty;
        
#if UNITY_EDITOR
        private void MarkDirty ()
        {
            dirty = true;
        }
        
        private static IEnumerable DrawTagDropdown()
        {
            ValueDropdownList<string> tagListDropdown = new ValueDropdownList<string>();
            
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                tagListDropdown.Add(UnityEditorInternal.InternalEditorUtility.tags[i], UnityEditorInternal.InternalEditorUtility.tags[i]);
            }
            return tagListDropdown;
        }
        
        private static IEnumerable DrawLayerDropdown()
        {
            ValueDropdownList<string> layerListDropdown = new ValueDropdownList<string>();
            
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
            {
                layerListDropdown.Add(UnityEditorInternal.InternalEditorUtility.layers[i], UnityEditorInternal.InternalEditorUtility.layers[i]);
            }
            return layerListDropdown;
        }
#endif
    }
}