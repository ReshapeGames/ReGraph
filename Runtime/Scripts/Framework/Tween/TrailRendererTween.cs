using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class TrailRendererTween : TweenData
    {
        public enum TrailCommand
        {
            Resize,
            Time
        }

        public TrailCommand command;

        [ShowIf("IsResize")]
        public float toStartWidth;

        [ShowIf("IsResize")]
        public float toEndWidth;

        [HideIf("IsResize")]
        public float to;

        public override Tween GetTween (TrailRenderer trail)
        {
            switch (command)
            {
                case TrailCommand.Resize:
                    return trail.DOResize(toStartWidth, toEndWidth, duration);
                case TrailCommand.Time:
                    return trail.DOTime(to, duration);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool IsResize ()
        {
            return command == TrailCommand.Resize;
        }
#endif
    }
}