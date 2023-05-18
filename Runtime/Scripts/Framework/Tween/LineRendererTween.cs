using DG.Tweening;
using UnityEngine;

namespace Reshape.ReFramework
{
    public class LineRendererTween : TweenData
    {
        public enum LineRendererCommand
        {
            Color
        }

        public LineRendererCommand command;

        public Color startColorA;
        public Color startColorB;
        public Color endColorA;
        public Color endColorB;

        public override Tween GetTween (LineRenderer line)
        {
            switch (command)
            {
                case LineRendererCommand.Color:
                    return line.DOColor(new Color2(startColorA, startColorB), new Color2(endColorA, endColorB), duration);
                default:
                    return null;
            }
        }
    }
}