/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

using UnityEngine;
using System.Collections.Generic;
using Color = UnityEngine.Color;

namespace Reshape.ReFramework
{
    [RequireComponent(typeof(Renderer))]
    public class Outline : MonoBehaviour
    {
        public Renderer Renderer { get; private set; }

        public int colorIndex;
        public Color color;
        public bool eraseRenderer;

        private int registeredId;
        private bool invisible;

        public void Enable (bool enable, int id = 0)
        {
            if (enable)
            {
                if (!enabled)
                {
                    enabled = true;
                    registeredId = id;
                    OnEnable();
                }
            }
            else if (registeredId == 0)
            {
                if (enabled)
                {
                    enabled = false;
                    OnDisable();
                }
            }
            else if (registeredId == id)
            {
                if (enabled)
                {
                    enabled = false;
                    OnDisable();
                }
            }
        }

        public void SetColor (Color c, int index)
        {
            color = c;
            colorIndex = index;
        }

        private void Awake ()
        {
            Renderer = GetComponent<Renderer>();
        }

        void OnEnable ()
        {
            List<OutlineEffect> effects = OutlineEffect.Instances;
            for (int i = 0; i < effects.Count; i++)
                effects[i].AddOutline(this);
        }

        void OnDisable ()
        {
            List<OutlineEffect> effects = OutlineEffect.Instances;
            for (int i = 0; i < effects.Count; i++)
                effects[i].RemoveOutline(this);
        }

        private void OnBecameInvisible ()
        {
            if (enabled)
            {
                invisible = true;
                OnDisable();
            }
        }

        private void OnBecameVisible ()
        {
            if (enabled && invisible)
            {
                invisible = false;
                OnEnable();
            }
        }
    }
}