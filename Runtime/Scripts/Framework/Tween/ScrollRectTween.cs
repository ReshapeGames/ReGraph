using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Reshape.ReFramework
{
    public class ScrollRectTween : TweenData
    {
        public enum ScrollRectCommand
        {
            NormalizedPos,
            HorizontalNormalizedPos,
            VerticalPos
        }

        public ScrollRectCommand command;

        [ShowIf("ShowVector2")]
        public Vector2 pos;

        [HideIf("ShowVector2")]
        public float to;

        public bool snapping;

        public override Tween GetTween (ScrollRect scroll)
        {
            switch (command)
            {
                case ScrollRectCommand.NormalizedPos:
                    return scroll.DONormalizedPos(pos, duration, snapping);
                case ScrollRectCommand.HorizontalNormalizedPos:
                    return scroll.DOHorizontalNormalizedPos(to, duration, snapping);
                case ScrollRectCommand.VerticalPos:
                    return scroll.DOVerticalNormalizedPos(to, duration, snapping);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool ShowVector2 ()
        {
            return command == ScrollRectCommand.NormalizedPos;
        }
#endif
    }
}