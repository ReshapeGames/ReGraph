using DG.Tweening;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class SliderTween : TweenData
    {
        public enum SliderCommand
        {
            Value
        }

        public SliderCommand command;
        public float to;
        public bool snapping;

        public override Tween GetTween (Slider slider)
        {
            switch (command)
            {
                case SliderCommand.Value:
                    return slider.DOValue(to, duration, snapping);
                default:
                    return null;
            }
        }
    }
}