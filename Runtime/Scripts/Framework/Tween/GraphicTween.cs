using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class GraphicTween : TweenData
    {
        public enum GraphicCommand
        {
            Color,
            Fade,
            BlendableColor
        }

        public GraphicCommand command;

        [ShowIf("IsColor")]
        public Color color;

        [HideIf("IsColor")]
        public float to;

        public override Tween GetTween (Graphic graphic)
        {
            switch (command)
            {
                case GraphicCommand.Color:
                    return graphic.DOColor(color, duration);
                case GraphicCommand.Fade:
                    return graphic.DOFade(to, duration);
                case GraphicCommand.BlendableColor:
                    return graphic.DOBlendableColor(color, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool IsColor ()
        {
            return command == GraphicCommand.Color || command == GraphicCommand.BlendableColor;
        }
#endif
    }
}