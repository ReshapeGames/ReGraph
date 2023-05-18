using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class ImageTween : TweenData
    {
        public enum ImageCommand
        {
            Color,
            Fade,
            FillAmount,
            GradientColor,
            BlendableColor
        }

        public ImageCommand command;

        [HideIf("HideColor")]
        public Color color;

        [HideIf("HideTo")]
        public float to;

        [HideIf("HideGradient")]
        public Gradient gradient;

        public override Tween GetTween (Image img)
        {
            switch (command)
            {
                case ImageCommand.Color:
                    return img.DOColor(color, duration);
                case ImageCommand.Fade:
                    return img.DOFade(to, duration);
                case ImageCommand.FillAmount:
                    return img.DOFillAmount(to, duration);
                case ImageCommand.GradientColor:
                    return img.DOGradientColor(gradient, duration);
                case ImageCommand.BlendableColor:
                    return img.DOBlendableColor(color, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.ToString().Contains("Color") || command == ImageCommand.GradientColor;
        }

        private bool HideTo ()
        {
            return command != ImageCommand.Fade && command != ImageCommand.FillAmount;
        }

        private bool HideGradient ()
        {
            return command != ImageCommand.GradientColor;
        }
#endif
    }
}