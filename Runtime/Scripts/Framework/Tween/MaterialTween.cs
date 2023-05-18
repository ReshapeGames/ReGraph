﻿using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class MaterialTween : TweenData
    {
        public enum MaterialCommand
        {
            Color,
            ColorProperty,
            Fade,
            FadeProperty,
            Float,
            GradientColor,
            GradientColorProperty,
            Offset,
            OffsetProperty,
            Tiling,
            TilingProperty,
            Vector,
            BlendableColor,
            BlendableColorProperty
        }

        public MaterialCommand command;

        public bool useSharedMaterial;

        [HideIf("HideColor")]
        public Color color;

        [HideIf("HideFloat")]
        public float to;

        [ShowIf("ShowGradient")]
        public Gradient gradient;

        [HideIf("HideVector2")]
        public Vector2 vector2;

        [ShowIf("ShowVector4")]
        public Vector4 vector4;

        [ShowIf("ShowProperty")]
        public string property;

        public override Tween GetTween (MeshRenderer meshRenderer)
        {
            Material mat = useSharedMaterial ? meshRenderer.sharedMaterial : meshRenderer.material;

            switch (command)
            {
                case MaterialCommand.Color:
                    return mat.DOColor(color, duration);
                case MaterialCommand.ColorProperty:
                    return mat.DOColor(color, property, duration);
                case MaterialCommand.Fade:
                    return mat.DOFade(to, duration);
                case MaterialCommand.FadeProperty:
                    return mat.DOFade(to, property, duration);
                case MaterialCommand.Float:
                    return mat.DOFloat(to, property, duration);
                case MaterialCommand.GradientColor:
                    return mat.DOGradientColor(gradient, duration);
                case MaterialCommand.GradientColorProperty:
                    return mat.DOGradientColor(gradient, property, duration);
                case MaterialCommand.Offset:
                    return mat.DOOffset(vector2, duration);
                case MaterialCommand.OffsetProperty:
                    return mat.DOOffset(vector2, property, duration);
                case MaterialCommand.Tiling:
                    return mat.DOTiling(vector2, duration);
                case MaterialCommand.TilingProperty:
                    return mat.DOTiling(vector2, property, duration);
                case MaterialCommand.Vector:
                    return mat.DOVector(vector4, property, duration);
                case MaterialCommand.BlendableColor:
                    return mat.DOBlendableColor(color, duration);
                case MaterialCommand.BlendableColorProperty:
                    return mat.DOBlendableColor(color, property, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.ToString().Contains("Color") || command.ToString().Contains("Gradient");
        }

        private bool HideFloat ()
        {
            return command != MaterialCommand.Fade && command != MaterialCommand.FadeProperty && command != MaterialCommand.Float;
        }

        private bool ShowGradient ()
        {
            return command == MaterialCommand.GradientColor || command == MaterialCommand.GradientColorProperty;
        }

        private bool HideVector2 ()
        {
            return !command.ToString().Contains("Offset") || !command.ToString().Contains("Tiling");
        }

        private bool ShowVector4 ()
        {
            return command == MaterialCommand.Vector;
        }

        private bool ShowProperty ()
        {
            return command.ToString().Contains("Property") || command == MaterialCommand.Float || command == MaterialCommand.Vector;
        }
#endif
    }
}