using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class CameraTween : TweenData
    {
        public enum CameraCommand
        {
            Aspect,
            Color,
            FarClipPlane,
            FieldOfView,
            NearClipPlane,
            OrthoSize,
            PixerRect,
            Rect,
            ShakePosition,
            ShakeRotation
        }

        public CameraCommand command;

        [HideIf("HideColor")]
        public Color color;

        [HideIf("HideFloat")]
        public float to;

        [ShowIf("ShowRect")]
        public Rect rect;

        [ShowIf("IsShake")]
        public float strength = 3;

        [ShowIf("IsShake")]
        public int vibrato = 10;

        [ShowIf("IsShake")]
        public float randomness = 90;

        [ShowIf("IsShake")]
        public bool fadeOut = true;

        public override Tween GetTween (Camera cam)
        {
            switch (command)
            {
                case CameraCommand.Aspect:
                    return cam.DOAspect(to, duration);
                case CameraCommand.Color:
                    return cam.DOColor(color, duration);
                case CameraCommand.FarClipPlane:
                    return cam.DOFarClipPlane(to, duration);
                case CameraCommand.FieldOfView:
                    return cam.DOFieldOfView(to, duration);
                case CameraCommand.NearClipPlane:
                    return cam.DONearClipPlane(to, duration);
                case CameraCommand.OrthoSize:
                    return cam.DOOrthoSize(to, duration);
                case CameraCommand.PixerRect:
                    return cam.DOPixelRect(rect, duration);
                case CameraCommand.Rect:
                    return cam.DORect(rect, duration);
                case CameraCommand.ShakePosition:
                    return cam.DOShakePosition(duration, strength, vibrato, randomness, fadeOut);
                case CameraCommand.ShakeRotation:
                    return cam.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut);
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private bool HideColor ()
        {
            return !command.ToString().Contains("Color");
        }

        private bool HideFloat ()
        {
            return ShowRect() || !HideColor() || IsShake();
        }

        private bool ShowRect ()
        {
            return command.ToString().Contains("Rect");
        }

        private bool IsShake ()
        {
            return command.ToString().Contains("Shake");
        }
#endif
    }
}