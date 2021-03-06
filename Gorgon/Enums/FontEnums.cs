#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Tuesday, November 14, 2006 11:09:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Direction of the font shadow.
    /// </summary>
    public enum FontShadowDirection
    {
        /// <summary>Upper left corner.</summary>
        UpperLeft = 0,
        /// <summary>Middle left.</summary>
        MiddleLeft = 1,
        /// <summary>Lower left corner.</summary>
        LowerLeft = 2,
        /// <summary>Upper middle.</summary>
        UpperMiddle = 3,
        /// <summary>Lower middle.</summary>
        LowerMiddle = 4,
        /// <summary>Upper right corner.</summary>
        UpperRight = 5,
        /// <summary>Middle right.</summary>
        MiddleRight = 6,
        /// <summary>Lower right corner.</summary>
        LowerRight = 7
    }

}
