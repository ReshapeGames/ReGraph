using DG.Tweening;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class CanvasGroupTween : TweenData
    {
        public enum CanvasGroupCommand
        {
            Fade
        }

        public CanvasGroupCommand command;
        public float value;

        public override Tween GetTween (CanvasGroup canvasGroup)
        {
            switch (command)
            {
                case CanvasGroupCommand.Fade:
                    return canvasGroup.DOFade(value, duration);
            }

            return null;
        }
    }
}