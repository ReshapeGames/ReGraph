using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class SpriteRendererTween : TweenData
    {
        public enum SpriteRendererCommand
        {
            Color,
            Fade,
            GradientColor,
            BlendableColor
        }

        public SpriteRendererCommand command;

        [HideIf("HideColor")]
        public Color color;

        [HideIf("HideTo")]
        public float to;

        [HideIf("HideGradient")]
        public Gradient gradient;

        public override Tween GetTween (SpriteRenderer sr)
        {
            switch (command)
            {
                case SpriteRendererCommand.Color:
                    return sr.DOColor(color, duration);
                case SpriteRendererCommand.Fade:
                    return sr.DOFade(to, duration);
                case SpriteRendererCommand.GradientColor:
                    return sr.DOGradientColor(gradient, duration);
                case SpriteRendererCommand.BlendableColor:
                    return sr.DOBlendableColor(color, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.ToString().Contains("Color") || command == SpriteRendererCommand.GradientColor;
        }

        private bool HideTo ()
        {
            return command != SpriteRendererCommand.Fade;
        }

        private bool HideGradient ()
        {
            return command != SpriteRendererCommand.GradientColor;
        }
#endif
    }
}