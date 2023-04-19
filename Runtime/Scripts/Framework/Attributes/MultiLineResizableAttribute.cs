using System;
using UnityEngine;

namespace Reshape.ReFramework
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MultiLineResizableAttribute : PropertyAttribute
    {
        public int minLine;

        public MultiLineResizableAttribute (int min)
        {
            minLine = min;
        }
    
        public MultiLineResizableAttribute ()
        {
            minLine = 1;
        }
    }
}
