using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class RectTween : TweenData
    {
        public enum RectCommand
        {
            AnchorMax,
            AnchorMin,
            AnchorPos,
            AnchorPosY,
            AnchorPosX,
            JumpAnchorPos,
            Pivot,
            PivotX,
            PivotY,
            PunchAnchorPos,
            ShakeAnchorPos,
            SizeDelta
        }

        public RectCommand command;
        public Vector2 value;

        [ShowIf("IsJumpAnchorPos")]
        public float jumpPower;

        [ShowIf("IsJumpAnchorPos")]
        public int numJumps;

        [HideIf("HideSnapping")]
        public bool snapping = false;

        public override Tween GetTween (RectTransform rect)
        {
            switch (command)
            {
                case RectCommand.AnchorPosY:
                    return rect.DOAnchorPosY(value.y, duration, snapping);
                case RectCommand.AnchorPosX:
                    return rect.DOAnchorPosX(value.x, duration, snapping);
                case RectCommand.AnchorPos:
                    return rect.DOAnchorPos(value, duration, snapping);
                case RectCommand.AnchorMin:
                    return rect.DOAnchorMin(value, duration, snapping);
                case RectCommand.AnchorMax:
                    return rect.DOAnchorMax(value, duration, snapping);
                case RectCommand.JumpAnchorPos:
                    return rect.DOJumpAnchorPos(value, jumpPower, numJumps, duration, snapping);
                case RectCommand.Pivot:
                    return rect.DOPivot(value, duration);
                case RectCommand.PivotX:
                    return rect.DOPivotX(value.x, duration);
                case RectCommand.PivotY:
                    return rect.DOPivotY(value.y, duration);
                case RectCommand.PunchAnchorPos:
                    return rect.DOPunchAnchorPos(value, duration, snapping: snapping);
                case RectCommand.ShakeAnchorPos:
                    return rect.DOShakeAnchorPos(duration, snapping: snapping);
                case RectCommand.SizeDelta:
                    return rect.DOSizeDelta(value, duration, snapping);
            }

            return null;
        }

#if UNITY_EDITOR
        private bool IsJumpAnchorPos()
        {
            return command == RectCommand.JumpAnchorPos;
        }

        private bool HideSnapping()
        {
            return command.ToString().Contains("Pivot");
        }
#endif
    }
}