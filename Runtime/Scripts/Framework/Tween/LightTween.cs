using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class LightTween : TweenData
    {
        public enum LightCommand
        {
            Color,
            Intensity,
            ShadowStrength,
            BlendableColor
        }

        public LightCommand command;

        [HideIf("HideColor")]
        public Color color;

        [ShowIf("HideColor")]
        public float to;

        public override Tween GetTween (Light light)
        {
            switch (command)
            {
                case LightCommand.Color:
                    return light.DOColor(color, duration);
                case LightCommand.Intensity:
                    return light.DOIntensity(to, duration);
                case LightCommand.ShadowStrength:
                    return light.DOShadowStrength(to, duration);
                case LightCommand.BlendableColor:
                    return light.DOBlendableColor(color, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.ToString().Contains("Color");
        }
#endif
    }
}