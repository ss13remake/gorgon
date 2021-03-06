#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, November 12, 2007 12:47:25 PM
// 
#endregion


using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private RenderWindow _control2Target = null;			// Render target for control #2.
		private Sprite _torus1 = null;							// Torus sprite.
		private Sprite _torus2 = null;							// Torus sprite.
		private Image _torusImage = null;						// Torus sprite image.
		private TextSprite _label = null;						// Info label.
		private TextSprite _status = null;						// Status label.
		private Font _labelFont = null;							// Label font.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear();
			_control2Target.Clear();

			// Draw a nice animated torus.
			_torus1.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_torus1.Animations["Rotate"].Advance(e.FrameDeltaTime * 1000.0f);
			_torus1.Draw();

			// Draw an empty circle on the secondary control.
			Gorgon.CurrentRenderTarget = _control2Target;

			_torus2.Animations["Rotate"].Advance(e.FrameDeltaTime * (1000.0f * ((float)trackSpeed.Value) / 5.0f));

			_torus2.Position = new Vector2D(_control2Target.Width / 2.0f - 64.0f, _control2Target.Height / 2.0f - 64.0f);
			_torus2.Color = Drawing.Color.Red;
			_torus2.UniformScale = 2.0f;
			_torus2.Draw();

			_torus2.Position = new Vector2D(_control2Target.Width / 2.0f + 64.0f, _control2Target.Height / 2.0f - 64.0f);
			_torus2.Color = Drawing.Color.Green;
			_torus2.Draw();

			_torus2.Position = new Vector2D(_control2Target.Width / 2.0f + 64.0f, _control2Target.Height / 2.0f + 64.0f);
			_torus2.Color = Drawing.Color.Blue;
			_torus2.Draw();

			_torus2.Position = new Vector2D(_control2Target.Width / 2.0f - 64.0f, _control2Target.Height / 2.0f + 64.0f);
			_torus2.Color = Drawing.Color.White;
			_torus2.Draw();

			// Draw the info label.
			_label.Draw();

			_status.Text = "Speed: ";
			if (_torus2.Animations["Rotate"].AnimationState == AnimationState.Stopped)
				_status.AppendText("Stopped");
			else
				_status.AppendText(string.Format("{0:0.0}x", ((float)trackSpeed.Value) / 5.0f));
			_status.Draw(true);

			// We have to call Update to refresh the secondary swap chain.
			_control2Target.Update();
			Gorgon.CurrentRenderTarget = null;
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		private void Initialize()
		{
			// Set smoothing mode to all the sprites.
			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

			_control2Target = new RenderWindow("Control2", groupControl2, false);

			// Get resources.
			_torusImage = Image.FromFile(@"..\..\..\..\Resources\Images\torus.png");
			_torus1 = Sprite.FromFile(@"..\..\..\..\Resources\Sprites\GiveMeSomeControl\Torus.gorSprite");
			_torus2 = _torus1.Clone() as Sprite;
			_torus1.Animations["Rotate"].AnimationState = AnimationState.Playing;
			_torus2.Animations["Rotate"].AnimationState = AnimationState.Playing;

			// Set up info label.
			_labelFont = new GorgonLibrary.Graphics.Font("Arial9pt", "Arial", 9.0f, true, true);
			// Minimize the fonts memory usage.
			_labelFont.CharacterList = "SpdtoxDrag me!\u2190:1234567890.";
			_labelFont.MaxFontImageWidth = 64;
			_labelFont.MaxFontImageHeight = 32;
			_label = new TextSprite("InfoLabel", "\u2190Drag me!", _labelFont, Drawing.Color.Black);
			_label.Smoothing = Smoothing.None;
			_status = new TextSprite("StatusLabel", "Speed: ", _labelFont, Drawing.Color.Black);
			_status.Smoothing = Smoothing.None;
			_status.Alignment = Alignment.Center;
		}

		/// <summary>
		/// Handles the Click event of the buttonAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonAnimation_Click(object sender, EventArgs e)
		{
			if (_torus2.Animations["Rotate"].AnimationState == AnimationState.Playing)
				_torus2.Animations["Rotate"].AnimationState = AnimationState.Stopped;
			else
				_torus2.Animations["Rotate"].AnimationState = AnimationState.Playing;
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Handles the SplitterMoving event of the splitViews control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterCancelEventArgs"/> instance containing the event data.</param>
		private void splitViews_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			Gorgon.Stop();
		}

		/// <summary>
		/// Handles the SplitterMoved event of the splitViews control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.SplitterEventArgs"/> instance containing the event data.</param>
		private void splitViews_SplitterMoved(object sender, SplitterEventArgs e)
		{
			Gorgon.Go();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				Close();

			if (e.KeyCode == Keys.S)
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Perform clean up.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Initialize Gorgon
				// Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
				Gorgon.Initialize(false, true);

				// Display the logo.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;

				// Set the video mode.
				ClientSize = new Drawing.Size(640, 480);
				Gorgon.SetMode(groupControl1);

				// Set an ugly background color.
				Gorgon.Screen.BackgroundColor = Drawing.Color.White;

				// Initialize.
				Initialize();

				// Assign idle event.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Begin rendering.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion
	}
}