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
        //public const float FrameMilliseconds = 1000f/60;

        public const float NearZ = 1;
        public const float FarZ = 2048;

        public const float MouseSlowFactor = 2;

        public const int GridWidth = 100;
        public const int GridDepth = 100;
        public const int GridCellSize = 10;

        private readonly Color m_backgroundColor = Color.FromArgb(255, 30, 30, 30);

        readonly Stopwatch m_stopwatch = new Stopwatch();

        private float m_keyRight;
        private float m_keyLeft;
        private float m_keyForward;
        private float m_keyBack;
        private float m_keyUp;
        private float m_keyDown;

        private float m_fps;

        private bool m_mouseCaptured;
        private Vector2 m_lastMousePosition;

        private BrainSimulation m_brainSimulation;

        public Matrix4 ProjectionMatrix { get; set; }

        private readonly Camera m_camera;
        private readonly GridModel m_gridModel;
        private readonly CompositeModelBase<ModelBase> m_brainModel;

        private readonly IList<ModelBase> m_models = new List<ModelBase>();
        private PickRay m_pickRay;
        private readonly Dictionary<ModelBase, float> m_translucentDistanceCache = new Dictionary<ModelBase, float>();
        private QFont m_font;
        private int m_modelsDisplayed;

        public BrainSimulation BrainSimulation
        {
            get { return m_brainSimulation; }
            set
            {
                // TODO: Cleanup the old simulation?
                m_brainModel.Clear();
                m_brainSimulation = value;
                // TODO: Nasty! Change!
                foreach (var region in m_brainSimulation.Regions)
                {
                    foreach (ExpertModel expert in region.Experts)
                        expert.Camera = m_camera;

                    m_brainModel.AddChild(region);
                }
            }
        }

        public VisualizationForm()
        {
            InitializeComponent();

            m_camera = new Camera
            {
                Position = new Vector3(5, 100, 100),
                Orientation = new Vector3((float) Math.PI, (float) (-Math.PI/4), 0)
            };

            m_gridModel = new GridModel(GridWidth, GridDepth, GridCellSize);

            m_models.Add(m_gridModel);

            m_brainModel = new BrainModel();

            m_models.Add(m_brainModel);
        }

        // Resize the glControl
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            glControl.Resize += glControl_Resize;
            glControl.MouseUp += glControl_MouseUp;

            GL.ClearColor(m_backgroundColor);

            // Heh. Blending is a pain with this.
            //GL.Enable(EnableCap.DepthTest);

            Application.Idle += Application_Idle;

            glControl_Resize(glControl, EventArgs.Empty);

            glControl.Context.SwapInterval = 1;

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            LoadSprites();

            ResetLastMousePosition();

            m_font = new QFont(new Font(FontFamily.GenericMonospace, 14))
            {
                Options =
                {
                    Colour = Color4.GreenYellow
                }
            };

            m_stopwatch.Start();
        }

        private void LoadSprites()
        {
            GL.GenTextures(1, out ExpertModel.NeuronTexture);
            GL.BindTexture(TextureTarget.Texture2D, ExpertModel.NeuronTexture);

            Bitmap bitmap = Resources.BasicNeuron;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void glControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                m_mouseCaptured = !m_mouseCaptured;

            if (m_mouseCaptured)
                ResetLastMousePosition();

            if (!m_mouseCaptured && e.Button == MouseButtons.Left)
                PickObject(e.X, ClientSize.Height - e.Y);  // Invert Y (windows 0,0 is top left, GL is bottom left).
        }

        private void PickObject(int x, int y)
        {
            Console.WriteLine($"Mouse: {x}, {y} of {ClientSize.Width}, {ClientSize.Height}");
            PickRay pickRay = GetPickRay(x, y);
            Console.WriteLine($"Ray: {pickRay.Direction.X}, {pickRay.Direction.Y}, {pickRay.Direction.Z}");

            m_pickRay = pickRay;

            ExpertModel expert = FindFirstExpert(pickRay, BrainSimulation.Regions);
            if (expert != null)
                SelectExpert(expert);
        }

        private void SelectExpert(ExpertModel expert)
        {
        }

        private PickRay GetPickRay(int x, int y)
        {
            float normX = (2f * x) / ClientSize.Width - 1f;
            float normY = (2f * y) / ClientSize.Height - 1f;

            Vector4 clipRay = new Vector4(normX, normY, -1, 0);

            Vector4 eyeRay = Vector4.Transform(clipRay, ProjectionMatrix.Inverted());
            eyeRay = new Vector4(eyeRay.X, eyeRay.Y, -1, 0);

            
            Vector3 worldRay = (Vector4.Transform(eyeRay, m_camera.CurrentFrameViewMatrix.Inverted())).Xyz.Normalized;

            return new PickRay
            {
                Position = m_camera.Position,
                Direction = worldRay,
                Length = 100f
            };
        }

        private ExpertModel FindFirstExpert(PickRay pickRay, List<RegionModel> regions)
        {
            float closestDistance = float.MaxValue;
            ExpertModel closestExpert = null;
            foreach (RegionModel region in regions)
            {
                foreach (ExpertModel expert in region.Models.OfType<ExpertModel>())
                {
                    float distance = expert.DistanceToRayOrigin(pickRay);
                    if (distance < closestDistance)
                    {
                        closestExpert = expert;
                        closestDistance = distance;
                    }
                }
            }

            return closestExpert;
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

            m_keyLeft = keyboardState.IsKeyDown(Key.A) ? 1 : 0;
            m_keyRight = keyboardState.IsKeyDown(Key.D) ? 1 : 0;
            m_keyForward = keyboardState.IsKeyDown(Key.W) ? 1 : 0;
            m_keyBack = keyboardState.IsKeyDown(Key.S) ? 1 : 0;
            m_keyUp = keyboardState.IsKeyDown(Key.Space) ? 1 : 0;
            m_keyDown = keyboardState.IsKeyDown(Key.C) ? 1 : 0;
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

            m_fps = 1000/elapsedMs;

            if (m_mouseCaptured)
            {
                Vector2 delta = (m_lastMousePosition - new Vector2(Mouse.GetState().X, Mouse.GetState().Y))/MouseSlowFactor;
                m_lastMousePosition += delta;

                if (delta != Vector2.Zero)
                    m_camera.AddRotation(delta.X, delta.Y, elapsedMs);

                HideCursor();
            }
            else
            {
                ShowCursor();
            }

            HandleKeyboard();

            UpdateFrame(elapsedMs);

            RenderFrame(elapsedMs);
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            GLControl c = sender as GLControl;

            if (c.ClientSize.Height == 0)
                c.ClientSize = new Size(c.ClientSize.Width, 1);
        }

        private void HideCursor()
        {
            Cursor = new Cursor(Resources.EmptyCursor.Handle);
            Mouse.SetPosition(Left + ClientSize.Width / 2, Top + ClientSize.Height / 2);
            m_lastMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        private void ShowCursor()
        {
            Cursor = Cursors.Default;
        }

        private void UpdateFrame(float elapsedMs)
        {
            foreach (ModelBase model in m_models)
                model.Update(elapsedMs);

            bool isSlow = Keyboard.GetState().IsKeyDown(Key.ControlLeft);
            m_camera.Move(m_keyRight - m_keyLeft, m_keyUp - m_keyDown, m_keyForward - m_keyBack, elapsedMs, isSlow);
        }

        private void RenderFrame(float elapsedMs)
        {
            RenderBegin();

            RenderScene(elapsedMs);

            RenderOverlay();

            RenderEnd();
        }

        private void RenderScene(float elapsedMs)
        {
            List<ModelBase> opaqueModels = new List<ModelBase>();
            List<ModelBase> translucentModels = new List<ModelBase>();

            // TODO: Do this only when necessary.
            foreach (ModelBase model in m_models)
                CollectModels(model, ref opaqueModels, ref translucentModels);

            // TODO: Only if the camera is moved.
            SortModels(translucentModels);

            m_modelsDisplayed = opaqueModels.Count + translucentModels.Count;

            // Debug only.
            m_pickRay?.Render(m_camera, elapsedMs);

            // Render here.
            foreach (ModelBase model in opaqueModels)
                model.Render(elapsedMs);

            foreach (ModelBase model in translucentModels)
                model.Render(elapsedMs);

        }

        private void RenderOverlay()
        {
            if (m_font == null)
                return;

            QFont.Begin();
            GL.PushMatrix();

            GL.Translate(m_font.MonoSpaceWidth, 0, 0);
            m_font.Print($"fps: {(int) m_fps}", QFontAlignment.Left);

            GL.Translate(0, m_font.LineSpacing, 0);
            m_font.Print($"# of models: {m_modelsDisplayed}", QFontAlignment.Left);


            GL.PopMatrix();
            QFont.End();
        }

        private void SortModels(List<ModelBase> models)
        {
            foreach (ModelBase model in models)
                m_translucentDistanceCache[model] = model.CurrentWorldMatrix.ExtractTranslation().DistanceFrom(m_camera.Position);

            models.Sort(
                (model1, model2) => m_translucentDistanceCache[model1] < m_translucentDistanceCache[model2]
                    ? 1
                    : m_translucentDistanceCache[model1] > m_translucentDistanceCache[model2] ? -1 : 0);
        }

        private static void CollectModels(ModelBase model, ref List<ModelBase> opaqueModels, ref List<ModelBase> translucentModels)
        {
            if (!model.Visible)
                return;

            model.UpdateCurrentWorldMatrix();

            var compositeModel = model as ICompositeModel;
            if (compositeModel != null)
                foreach (ModelBase child in compositeModel.Models)
                    CollectModels(child, ref opaqueModels, ref translucentModels);

            if (model.Translucent)
                translucentModels.Add(model);
            else
                opaqueModels.Add(model);
        }

        private void RenderBegin()
        {
            SetUpProjection();

            SetUpView();

            // QFont leaves the last texture bound.
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void SetUpView()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            m_camera.UpdateCurrentFrameMatrix();
            Matrix4 viewMatrix = m_camera.CurrentFrameViewMatrix;

            GL.LoadMatrix(ref viewMatrix);
        }

        private void SetUpProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height); // Use all of the glControl painting area
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            float aspectRatio = ClientSize.Width/(float) ClientSize.Height;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float) (Math.PI/4), aspectRatio, NearZ, FarZ);
            Matrix4 perspective = ProjectionMatrix;
            GL.LoadMatrix(ref perspective);
        }

        private void RenderEnd()
        {
            GL.Flush();

            glControl.SwapBuffers();
        }
    }
}
