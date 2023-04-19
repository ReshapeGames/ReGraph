using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reshape.ReFramework
{
    [Serializable]
    public class ItemData : ScriptableObject
    {
        [HideInInspector]
        public string id;
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        [HorizontalGroup("Split", width:57f)]
        [VerticalGroup("Split/1")]
        [HideLabel]
        public Sprite icon;
        [InlineProperty]
        [VerticalGroup("Split/2")]
        [HideLabel]
        [BoxGroup("Split/2/Name")]
        public StringProperty displayName;
        [InlineProperty]
        [VerticalGroup("Split/3")]
        [HideLabel]
        [BoxGroup("Split/3/Description")]
        public StringProperty description;
    }
}