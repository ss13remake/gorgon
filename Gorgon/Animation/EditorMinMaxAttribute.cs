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
// Created: Saturday, May 03, 2008 7:15:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Attribute for sprite editor to determine min/max.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class EditorMinMaxAttribute
		: Attribute
	{
		#region Variables.
		private float _min = float.MinValue;		// Minimum value.
		private float _max = float.MaxValue;		// Maximum value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the minimum value.
		/// </summary>
		public float Minimum
		{
			get
			{
				return _min;
			}
		}

		/// <summary>
		/// Property to return the maximum value.
		/// </summary>
		public float Maximum
		{
			get
			{
				return _max;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMinMaxAttribute"/> class.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public EditorMinMaxAttribute(float min, float max)
		{
			_min = min;
			_max = max;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMinMaxAttribute"/> class.
		/// </summary>
		public EditorMinMaxAttribute()
		{
		}
		#endregion
	}
}
