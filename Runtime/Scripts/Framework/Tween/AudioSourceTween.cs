using DG.Tweening;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class AudioSourceTween : TweenData
    {
        public enum AudioSourceCommand
        {
            Fade,
            Pitch
        }

        public AudioSourceCommand command;

        public float to;

        public override Tween GetTween (AudioSource source)
        {
            switch (command)
            {
                case AudioSourceCommand.Fade:
                    return source.DOFade(to, duration);
                case AudioSourceCommand.Pitch:
                    return source.DOPitch(to, duration);
                default:
                    return null;
            }
        }
    }
}