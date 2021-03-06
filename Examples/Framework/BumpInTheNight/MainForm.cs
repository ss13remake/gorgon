#region MIT.
// 
// Examples.
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using Dialogs;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Classes.
		/// <summary>
		/// This will contain information about our "lights" to pass into the shader.
		/// </summary>
		private class Lights
		{
			/// <summary>
			/// Flag to enable or disable the light.
			/// </summary>
			public bool[] Enabled;
			/// <summary>
			/// Position of the light in 3D space.
			/// </summary>
			public Vector4D[] LightPosition;
			/// <summary>
			/// Diffuse color of the light.
			/// </summary>
			public Drawing.Color[] LightColor;
			/// <summary>
			/// Ambient color for the light.
			/// </summary>
			public Drawing.Color[] LightAmbientColor;
			/// <summary>
			/// Light intensity.
			/// </summary>
			public float[] LightIntensity;
			/// <summary>
			/// Flag to enable or disable specular hilights.
			/// </summary>
			public bool[] SpecularEnabled;
			/// <summary>
			/// Intensity of specular hilights.
			/// </summary>
			public float[] LightSpecularIntensity;
			/// <summary>
			/// Specular hilight color.
			/// </summary>
			public Drawing.Color[] LightSpecularColor;

			/// <summary>
			/// Initializes a new instance of the <see cref="Lights"/> class.
			/// </summary>
			public Lights()
			{
				Enabled = new bool[] {true, false, false, false};
				SpecularEnabled = new bool[4] {true, true, true, true};
				LightPosition = new Vector4D[4] { Vector3D.Zero, Vector3D.Zero, Vector3D.Zero, Vector3D.Zero };
				LightIntensity = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
				LightColor = new Drawing.Color[4] { Drawing.Color.White, Drawing.Color.White, Drawing.Color.White, Drawing.Color.White };
				LightAmbientColor = new Drawing.Color[4] { Drawing.Color.Black, Drawing.Color.Black, Drawing.Color.Black, Drawing.Color.Black };
				LightSpecularIntensity = new float[4] { 64.0f, 64.0f, 64.0f, 64.0f };
				LightSpecularColor = new Drawing.Color[4] { Drawing.Color.White, Drawing.Color.White, Drawing.Color.White, Drawing.Color.White };
			}
		}
		#endregion

		#region Variables.
		private Random _rnd = new Random();										// Random number generator.
		private RenderImage _lightBuffer = null;								// Buffer used to render the lit image.
		private RenderImage _colorBuffer = null;								// Color buffer - used to draw diffuse sprites.
		private RenderImage _normalBuffer = null;								// Normal map buffer - used to draw normal maps.
		private RenderImage _specBuffer = null;									// Specular map buffer - used to draw specular maps.
		private Sprite _bufferSprite = null;									// Buffer sprite.
		private Sprite _torch = null;											// The torch sprite.
		private Sprite _lightSprite = null;										// Lighting sprite.
		private Sprite _bumpSprite = null;										// Bump map sprite.
		private Image _torchImage = null;										// Image used for torch.
		private Image _bumpNormal = null;										// Normal map.
		private Image _bumpColor = null;										// Color map.
		private Image _bumpSpec = null;											// Specular map.
		private Image _lightSource = null;										// "Light source" - Gradient circle.
		private FXShader _bumpShader = null;									// Bump mapping shader.
		private float _angle = 0.0f;											// Bump sprite rotation angle.
		private Drawing.RectangleF _lightSpriteScale = Drawing.RectangleF.Empty;// Scaling for lighting sprite.
		private TextSprite _statusText;											// Status text sprite.
		private bool _showHelp = false;											// Show help.
		private Lights _lights = null;											// Lights to use.
		private PreciseTimer _timer = new PreciseTimer();						// Timer for flickering.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the chosen video driver.
		/// </summary>
		/// <param name="driver">Driver to validate.</param>
		/// <returns>
		/// TRUE if the driver is valid, FALSE if not.
		/// </returns>
		protected override bool OnValidateDriver(Driver driver)
		{
			if ((driver.PixelShaderVersion < new Version(2, 0)) || (driver.VertexShaderVersion < new Version(2, 0)))
			{
				UI.ErrorBox(this, "This example requires a shader model 2 card.\nSelected driver pixel shader version (min. 2.0): " + 
					Gorgon.CurrentDriver.PixelShaderVersion.ToString() + "\n" + 
					"Selected driver vertex shader version (min. 2.0): " + Gorgon.CurrentDriver.VertexShaderVersion.ToString());				
				return false;
			}

			return true;
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			Matrix invWorldMatrix = Matrix.Identity;
			float cos = MathUtility.Cos(MathUtility.Radians(_angle));
			float sin = MathUtility.Sin(MathUtility.Radians(_angle));

			_angle += 12.0f * e.FrameDeltaTime;

			_bumpSprite.SetPosition(_colorBuffer.Width / 2.0f, _colorBuffer.Height / 2.0f);
			_bumpShader.Parameters["LightPosition"].SetValue(_lights.LightPosition);
			_bumpShader.Parameters["LightSpecular"].SetValue(_lights.LightSpecularIntensity);
			_bumpShader.Parameters["LightSpecularEnabled"].SetValue(_lights.SpecularEnabled);

			// We have to rotate our light value to get our specular to show properly.
			invWorldMatrix.m11 = cos;
			invWorldMatrix.m12 = -sin;
			invWorldMatrix.m21 = sin;
			invWorldMatrix.m22 = cos;
			_bumpShader.Parameters["invWorldMatrix"].SetValue(invWorldMatrix);

			_torch.Position = (Vector3D)_lights.LightPosition[0];
			_torch.Animations[0].Advance(e.FrameDeltaTime * 1000.0f);
			_lightSprite.Position = (Vector3D)_lights.LightPosition[0]; ;

			_bufferSprite.ImageOffset = Vector2D.Subtract((Vector2D)_lights.LightPosition[0], _lightSprite.Axis);

			if (_timer.Milliseconds > 100) 
			{
				if (_rnd.Next(1000) > 500)
				{
					_lightSpriteScale.Y = _lightSpriteScale.X = (float)(_rnd.NextDouble() * 0.05) + 0.05f;
					_lights.LightIntensity[0] = (float)_rnd.NextDouble() * 0.3f + 0.7f;
					_lights.LightColor[0] = Drawing.Color.FromArgb(255, _rnd.Next(180, 255), _rnd.Next(160, 180));
				}
				else
				{
					_lightSpriteScale.Y = _lightSpriteScale.X = 0.0f;
					_lights.LightIntensity[0] = 1.1f;
					_lights.LightColor[0] = Drawing.Color.White;
				}
				_timer.Reset();
			}
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			// Draw the sprites to the normal and diffuse buffers.
			Gorgon.CurrentRenderTarget = _normalBuffer;
			_normalBuffer.Clear(Drawing.Color.FromArgb(255, 127, 127, 200));		// Change the blue channel to lighten or darken the background.
			_bumpSprite.Image = _bumpNormal;
			_bumpSprite.Draw();
			Gorgon.CurrentRenderTarget = _specBuffer;
			_specBuffer.Clear(Drawing.Color.Black);									// Dark = no specular.
			_bumpSprite.Image = _bumpSpec;
			_bumpSprite.Draw();
			Gorgon.CurrentRenderTarget = _colorBuffer;
			_colorBuffer.Clear(Drawing.Color.White);
			_bumpSprite.Image = _bumpColor;
			_bumpSprite.Rotation = 0.0f;
			_bumpSprite.Draw();
			_bumpSprite.Rotation = _angle;			
			_bumpSprite.Draw();

			// Draw to our lit buffer.
			Gorgon.CurrentRenderTarget = _lightBuffer;
			_lightBuffer.Clear(Drawing.Color.Black);

			// Draw bump mapped result.
			if (Gorgon.CurrentDriver.PixelShaderVersion.Major < 3)
				Gorgon.CurrentShader = _bumpShader.Techniques["Bump20"];
			else
				Gorgon.CurrentShader = _bumpShader.Techniques["Bump"];
			_bufferSprite.Image = _colorBuffer.Image;
			_bufferSprite.ImageOffset = new Vector2D(_lights.LightPosition[0].X, _lights.LightPosition[0].Y) - _lightSprite.Axis;
			_bufferSprite.Draw();
			Gorgon.CurrentShader = null;
			_lightBuffer.BlendingMode = BlendingModes.Modulated;
			// Draw light map to obscure the lit image.
			_bumpShader.Parameters["LightIntensity"].SetValue(_lights.LightIntensity);
			_bumpShader.Parameters["LightColor"].SetValue(_lights.LightColor);
			_lightSource.Blit(-_lightSource.Width * _lightSpriteScale.X, -_lightSource.Height * _lightSpriteScale.Y, _lightSpriteScale.Width + (_lightSpriteScale.Width * _lightSpriteScale.X), _lightSpriteScale.Height + (_lightSpriteScale.Height * _lightSpriteScale.X));				
			_lightBuffer.BlendingMode = BlendingModes.None;
			Gorgon.CurrentRenderTarget = null;			
			
			// Finally blit the lit image to the screen.
			_lightSprite.Draw();
			_torch.Draw();

			_statusText.Text = "F1 - " + (_showHelp ? " Hide help." : "Show help.\n");
			if (_showHelp)
				_statusText.AppendText("\nRight Mouse - " + (_lights.SpecularEnabled[0] ? "Disable" : "Enable") + " specular map.\nScroll Wheel - Increase or decrease light distance.\nESC - Quit.\nCTRL+F - Show frame statistics.\nAlt+Enter - Switch to " + (Gorgon.Screen.Windowed ? "full screen mode." : "windowed mode.") + "\n\u2191 - Increase specular intensity.\n\u2193 - Decrease specular intensity.\n");
				
			_statusText.AppendText("\nLight distance: " + _lights.LightPosition[0].Z.ToString("0.000"));
			_statusText.AppendText("\nSpecular Intensity: " + _lights.LightSpecularIntensity[0].ToString("0"));
			_statusText.Draw();
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);
			_lights.LightPosition[0] = new Vector3D(e.Position.X, e.Position.Y, _lights.LightPosition[0].Z);
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseButtonDown(MouseInputEventArgs e)
		{
			base.OnMouseButtonDown(e);

			if ((e.Buttons & GorgonLibrary.InputDevices.MouseButtons.Button2) == GorgonLibrary.InputDevices.MouseButtons.Button2)
				_lights.SpecularEnabled[0] = !_lights.SpecularEnabled[0];
		}

		/// <summary>
		/// Function called when a mouse scroll wheel is scrolled.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseWheelScrolled(MouseInputEventArgs e)
		{
			base.OnMouseWheelScrolled(e);

			if (e.WheelDelta > 0)
				_lights.LightPosition[0].Z += 1.0f;
			else
				_lights.LightPosition[0].Z -= 1.0f;

			if (_lights.LightPosition[0].Z < 1.0f)
				_lights.LightPosition[0].Z = 1.0f;
		}

		/// <summary>
		/// Function called when a keyboard key is pushed down.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected override void OnKeyboardKeyDown(KeyboardInputEventArgs e)
		{
			base.OnKeyboardKeyDown(e);

			if (e.Key == KeyboardKeys.F1)
				_showHelp = !_showHelp;
			if (e.Key == KeyboardKeys.Up)
				_lights.LightSpecularIntensity[0] += 1.0f;
			if (e.Key == KeyboardKeys.Down)
				_lights.LightSpecularIntensity[0] -= 1.0f;

			if (_lights.LightSpecularIntensity[0] < 1.0f)
				_lights.LightSpecularIntensity[0] = 1.0f;
			if (_lights.LightSpecularIntensity[0] > 256.0f)
				_lights.LightSpecularIntensity[0] = 256.0f;
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();
			_lightSpriteScale = new System.Drawing.RectangleF(0, 0, Gorgon.Screen.Width / 2.5f, Gorgon.Screen.Width / 2.5f);
			_lightBuffer.SetDimensions((int)_lightSpriteScale.Width, (int)_lightSpriteScale.Height);
			_colorBuffer.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
			_normalBuffer.SetDimensions(_colorBuffer.Width, _colorBuffer.Height);
			_specBuffer.SetDimensions(_colorBuffer.Width, _colorBuffer.Height);

			_bufferSprite.SetSize(_colorBuffer.Width, _colorBuffer.Height);
			_lightSprite.SetSize(_lightBuffer.Width, _lightBuffer.Height);
			_lightSprite.SetAxis(_lightBuffer.Width / 2.0f, _lightBuffer.Height / 2.0f);
			_bumpShader.Parameters["TextureSize"].SetValue(new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height));
			_bumpShader.Parameters["ColorMap"].SetValue(_colorBuffer);
			_bumpShader.Parameters["NormalMap"].SetValue(_normalBuffer);
			_bumpShader.Parameters["SpecularMap"].SetValue(_specBuffer);
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			_lights = new Lights();
			_lights.LightPosition[0] = new Vector3D(Input.Mouse.Position.X, Input.Mouse.Position.Y, 100.0f);

			_torchImage = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/Torch.png");
			_lightSource = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/Lightmap.png");
			_torch = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Torch.gorSprite");

			_lightSpriteScale = new System.Drawing.RectangleF(0, 0, Gorgon.Screen.Width / 2.5f, Gorgon.Screen.Width / 2.5f);
			
			_lightBuffer = new RenderImage("Buffer", (int)_lightSpriteScale.Width, (int)_lightSpriteScale.Height, ImageBufferFormats.BufferRGB888X8);
			_colorBuffer = new RenderImage("ColorBuffer", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888A8);
			_normalBuffer = new RenderImage("NormalBuffer", _colorBuffer.Width, _colorBuffer.Height, ImageBufferFormats.BufferRGB888X8);
			_specBuffer = new RenderImage("SpecBuffer", _colorBuffer.Width, _colorBuffer.Height, ImageBufferFormats.BufferRGB888X8);

			_statusText = new TextSprite("Status", string.Empty, FrameworkFont, Drawing.Color.White);
			FrameworkFont.FontSize = 9.75f;
			FrameworkFont.CharacterList += "\u2191\u2193";
			
			_bufferSprite = new Sprite("BackBuffer", _colorBuffer);
			_bufferSprite.WrapMode = ImageAddressing.Clamp;

			_lightSprite = new Sprite("LightSprite", _lightBuffer);
			_lightSprite.SetAxis(_lightBuffer.Width / 2.0f, _lightBuffer.Height / 2.0f);

			_bumpNormal = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/normalmap.png");
			_bumpColor = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/colormap.png");
			_bumpSpec = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/specmap.png");
			_bumpSprite = new Sprite("BumpSprite", _bumpColor);
			_bumpSprite.UniformScale = Gorgon.Screen.Height / 480.0f;

#if DEBUG
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.Debug);
#else
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.OptimizationLevel3);
#endif


			_bumpShader.Parameters["ColorMap"].SetValue(_colorBuffer);
			_bumpShader.Parameters["NormalMap"].SetValue(_normalBuffer);
			_bumpShader.Parameters["SpecularMap"].SetValue(_specBuffer);
			_bumpShader.Parameters["LightEnabled"].SetValue(_lights.Enabled);
			_bumpShader.Parameters["LightSpecularEnabled"].SetValue(_lights.SpecularEnabled);			
			_bumpShader.Parameters["LightSpecularColor"].SetValue(_lights.LightSpecularColor);
			_bumpShader.Parameters["TextureSize"].SetValue(new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height));

			_bumpSprite.SetAxis(_bumpSprite.Width / 2.0f, _bumpSprite.Height / 2.0f);
			_torch.Animations[0].AnimationState = AnimationState.Playing;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\BumpInTheNight.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}