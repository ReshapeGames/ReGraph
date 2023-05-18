using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class TextTween : TweenData
    {
        public enum TextCommand
        {
            Color,
            Fade,
            Text,
            BlendableColor
        }

        public TextCommand command;

        [HideIf("HideColor")]
        public Color color;

        [HideIf("HideTo")]
        public float to;

        [HideIf("HideNewText")]
        public string newText;

        [HideIf("HideNewText")]
        public bool richText;

        [HideIf("HideNewText")]
        public ScrambleMode scrambleMode;

        public override Tween GetTween (Text text)
        {
            switch (command)
            {
                case TextCommand.Color:
                    return text.DOColor(color, duration);
                case TextCommand.Fade:
                    return text.DOFade(to, duration);
                case TextCommand.Text:
                    return text.DOText(newText, duration, richText, scrambleMode);
                case TextCommand.BlendableColor:
                    return text.DOBlendableColor(color, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.GetType().ToString().Contains("Color");
        }

        private bool HideTo ()
        {
            return command != TextCommand.Fade;
        }

        private bool HideNewText ()
        {
            return command != TextCommand.Text;
        }
#endif
    }
}