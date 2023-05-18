using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Reshape.ReFramework
{
    public class AudioMixerTween : TweenData
    {
        public enum AudioMixerCommand
        {
            SetFloat
        }

        public AudioMixerCommand command;

        public string floatName;
        public float to;

        public override Tween GetTween (AudioMixer mixer)
        {
            switch (command)
            {
                case AudioMixerCommand.SetFloat:
                    return mixer.DOSetFloat(floatName, to, duration);
                default:
                    return null;
            }
        }
    }
}