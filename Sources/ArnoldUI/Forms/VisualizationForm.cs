using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArnoldUI.Properties;
using GoodAI.Arnold.Graphics;
using GoodAI.Arnold.Graphics.Models;
using GoodAI.Arnold.Properties;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.OpenTKExtensions;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using QuickFont;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GoodAI.Arnold.Forms
{
    public partial class VisualizationForm : Form
    {
        public const float MouseSlowFactor = 2;

        private readonly Stopwatch m_stopwatch = new Stopwatch();

        private InputInfo m_inputInfo;

        private bool m_mouseCaptured;
        private Vector2 m_lastMousePosition;

        private readonly SimulationHandler m_simulationHandler;
        private readonly BrainSimulation m_simulation;
        private readonly Visualization m_visualization;

        public VisualizationForm(SimulationHandler handler)
        {
            InitializeComponent();

            m_simulationHandler = handler;
            m_simulation = handler.BrainSimulation;

            m_visualization = new Visualization(glControl, m_simulation);
        }

        // Resize the glControl
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            glControl.Resize += glControl_Resize;
            glControl.MouseUp += glControl_MouseUp;

            m_visualization.Init();

            Application.Idle += Application_Idle;

            glControl_Resize(glControl, EventArgs.Empty);

            glControl.Context.SwapInterval = 1;


            m_stopwatch.Start();
        }

        private void glControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                m_mouseCaptured = !m_mouseCaptured;
                
                if (m_mouseCaptured)
                    HideCursor();
                else
                    ShowCursor();
            }


            if (m_mouseCaptured)
                ResetLastMousePosition();

            if (!m_mouseCaptured && e.Button == MouseButtons.Left)
                m_visualization.PickObject(e.X, glControl.Size.Height - e.Y);  // Invert Y (windows 0,0 is top left, GL is bottom left).
        }

        private void ResetLastMousePosition()
        {
            MouseState state = Mouse.GetState();
            m_lastMousePosition = new Vector2(state.X, state.Y);
        }

        private void HandleKeyboard()
        {
            if (!glControl.Focused)
                return;

            KeyboardState keyboardState = Keyboard.GetState();

            // TODO: Implement, duh.
            //if (keyboardState.IsKeyDown(Key.Escape))
            //    StopSimulation()

            m_inputInfo.KeyLeft = keyboardState.IsKeyDown(Key.A);
            m_inputInfo.KeyRight = keyboardState.IsKeyDown(Key.D);
            m_inputInfo.KeyForward = keyboardState.IsKeyDown(Key.W);
            m_inputInfo.KeyBack = keyboardState.IsKeyDown(Key.S);
            m_inputInfo.KeyUp = keyboardState.IsKeyDown(Key.Space);
            m_inputInfo.KeyDown = keyboardState.IsKeyDown(Key.C);

            // Ctrl doesn't work with the above methods.
            m_inputInfo.KeySlow = Keyboard.GetState().IsKeyDown(Key.ControlLeft);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            if (glControl.IsDisposed)
                return;

            while (glControl.IsIdle)
                Step();
        }

        private void Step()
        {
            m_stopwatch.Stop();
            float elapsedMs = m_stopwatch.ElapsedMilliseconds;
            m_stopwatch.Reset();
            m_stopwatch.Start();

            if (m_mouseCaptured)
            {
                Vector2 delta = (m_lastMousePosition - new Vector2(Mouse.GetState().X, Mouse.GetState().Y))/MouseSlowFactor;
                m_lastMousePosition += delta;

                m_inputInfo.CameraRotated = delta != Vector2.Zero;
                m_inputInfo.CameraDeltaX = delta.X;
                m_inputInfo.CameraDeltaY = delta.Y;

                Mouse.SetPosition(Left + glControl.Size.Width / 2, Top + glControl.Size.Height / 2);
                m_lastMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }

            HandleKeyboard();

            m_visualization.Step(m_inputInfo, elapsedMs);
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            GLControl c = sender as GLControl;

            if (c.Size.Height == 0)
                c.Size = new Size(c.Size.Width, 1);
        }

        private void HideCursor()
        {
            Cursor = new Cursor(Resources.EmptyCursor.Handle);
        }

        private void ShowCursor()
        {
            Cursor = Cursors.Default;
        }
    }
}
