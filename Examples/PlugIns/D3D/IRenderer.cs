﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Interface for the 3D renderer.
    /// </summary>
    public interface IRenderer
        : IDisposable
    {
        /// <summary>
        /// Function to begin the rendering.
        /// </summary>
        void Begin();

        /// <summary>
        /// Function to render the scene.
        /// </summary>
        /// <param name="frameTime">Frame delta time.</param>
        void Render(float frameTime);

        /// <summary>
        /// Function to end the rendering.
        /// </summary>
        void End();

        /// <summary>
        /// Function to initialize the 3D renderer.
        /// </summary>
        void Initialize();
    }
}
