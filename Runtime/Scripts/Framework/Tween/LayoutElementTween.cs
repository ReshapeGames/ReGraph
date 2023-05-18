using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class LayoutElementTween : TweenData
    {
        public enum LayoutElementCommand
        {
            FlexibleSize,
            MinSize,
            PreferredSize
        }

        public LayoutElementCommand command;
        public Vector2 to;
        public bool snapping;

        public override Tween GetTween (LayoutElement element)
        {
            switch (command)
            {
                case LayoutElementCommand.FlexibleSize:
                    return element.DOFlexibleSize(to, duration, snapping);
                case LayoutElementCommand.MinSize:
                    return element.DOMinSize(to, duration, snapping);
                case LayoutElementCommand.PreferredSize:
                    return element.DOPreferredSize(to, duration, snapping);
                default:
                    return null;
            }
        }
    }
}