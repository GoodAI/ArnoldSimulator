using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using GoodAI.Arnold.Core;
using GoodAI.Arnold.Extensions;
using GoodAI.Arnold.Properties;
using GoodAI.Arnold.Visualization.Models;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GoodAI.Arnold.Visualization
{
    public class Scene
    {
        private readonly GLControl m_control;

        public const int GridWidth = 100;
        public const int GridDepth = 100;
        public const int GridCellSize = 10;

        public const float NearZ = 1;
        public const float FarZ = 2048;

        private readonly Color4 m_backgroundColor = new Color4(30, 30, 30, 255);

        public Matrix4 ProjectionMatrix { get; set; }

        private readonly ICamera m_camera;
        private readonly GridModel m_gridModel;

        private PickRay m_pickRay;
        private readonly Dictionary<IModel, float> m_translucentDistanceCache = new Dictionary<IModel, float>();
        private QFont m_font;

        private int m_modelsDisplayed;
        private float m_fps;

        private readonly ISet<NeuronModel> m_pickedNeurons = new HashSet<NeuronModel>();
        private readonly IConductor m_conductor;

        private SimulationModel m_simulationModel;

        public Scene(GLControl glControl, IConductor conductor)
        {
            m_control = glControl;
            m_conductor = conductor;
            m_conductor.ModelProvider.ModelUpdated += OnModelUpdated;

            m_camera = new Camera
            {
                Position = new Vector3(5, 100, 100),
                Orientation = new Vector3((float) Math.PI, (float) (-Math.PI/4), 0)
            };

            m_gridModel = new GridModel(GridWidth, GridDepth, GridCellSize);

            //InjectCamera();
        }

        private void OnModelUpdated(object sender, NewModelEventArgs e)
        {
            m_simulationModel = e.Model ?? m_simulationModel;
        }

        private void InjectCamera()
        {
            // TODO: Nasty! Change!
            // If (when) neurons are drawn via shaders, they might not actually need the camera position? (no sprites)
            foreach (var region in m_simulationModel.Regions)
                foreach (NeuronModel neurons in region.Neurons)
                    neurons.Camera = m_camera;
        }

        public void Init()
        {
            GL.ClearColor(m_backgroundColor);

            // Heh. Blending is a pain with this.
            //GL.Enable(EnableCap.DepthTest);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            LoadSprites();

            m_font = new QFont(new Font(FontFamily.GenericMonospace, 14))
            {
                Options =
                {
                    Colour = Color4.GreenYellow
                }
            };
        }

        private void LoadSprites()
        {
            GL.GenTextures(1, out NeuronModel.NeuronTexture);
            GL.BindTexture(TextureTarget.Texture2D, NeuronModel.NeuronTexture);

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

        public void PickObject(int x, int y)
        {
            if (m_simulationModel == null)
                return;

            m_pickRay = PickRay.Pick(x, y, m_camera, m_control.Size, ProjectionMatrix);

            NeuronModel neuron = FindFirstNeuron(m_pickRay, m_simulationModel.Regions);
            if (neuron != null)
                ToggleNeuron(neuron);
        }

        private NeuronModel FindFirstNeuron(PickRay pickRay, IEnumerable<RegionModel> regions)
        {
            float closestDistance = float.MaxValue;
            NeuronModel closestNeuron = null;
            foreach (RegionModel region in regions)
            {
                foreach (NeuronModel neuron in region.Neurons)
                {
                    float distance = neuron.DistanceToRayOrigin(pickRay);
                    if (distance < closestDistance)
                    {
                        closestNeuron = neuron;
                        closestDistance = distance;
                    }
                }
            }

            return closestNeuron;
        }

        private void ToggleNeuron(NeuronModel neuron)
        {
            neuron.Picked = !neuron.Picked;

            // The neuron is recorded in a hashset for future drawing of it's properties.
            if (neuron.Picked)
                m_pickedNeurons.Add(neuron);
            else
                m_pickedNeurons.Remove(neuron);
        }

        private Vector3 ModelToScreenCoordinates(IModel model, out bool isBehindCamera)
        {
            Vector2 projected = Project(model, out isBehindCamera);
            return new Vector3(projected.X, m_control.Size.Height - projected.Y, 0);
        }

        /// <summary>
        /// Project the center of the given model onto screen coordinates.
        /// </summary>
        private Vector2 Project(IModel model, out bool isBehindCamera)
        {
            // TODO(HonzaS): Allow different points than centers?
            var center4 = new Vector4(Vector3.Zero, 1);

            // Transform the to clip space.
            Vector4 world = Vector4.Transform(center4, model.CurrentWorldMatrix);
            Vector4 view = Vector4.Transform(world, m_camera.CurrentFrameViewMatrix);
            Vector4 clip = Vector4.Transform(view, ProjectionMatrix);

            isBehindCamera = clip.Z < 0;

            // Transform to screen space.
            Vector3 ndc = clip.Xyz/clip.W;

            Vector2 screen = ((ndc.Xy + Vector2.One)/2f) * new Vector2(m_control.Size.Width, m_control.Size.Height);

            return screen;
        }

        public void Step(InputInfo inputInfo, float elapsedMs)
        {
            m_conductor.ModelProvider.GetNewModel();

            if (m_simulationModel != null)
            {
                UpdateModelFilter();

                // TODO(HonzaS): When we have incremental updates, optimize this to not go through all neurons.
                InjectCamera();
            }

            m_fps = 1000/elapsedMs;

            if (inputInfo.CameraRotated)
                m_camera.Rotate(inputInfo.CameraDeltaX, inputInfo.CameraDeltaY, elapsedMs);

            int keyRight = inputInfo.KeyRight ? 1 : 0;
            int keyLeft = inputInfo.KeyLeft ? 1 : 0;
            int keyForward = inputInfo.KeyForward ? 1 : 0;
            int keyBack = inputInfo.KeyBack ? 1 : 0;
            int keyUp = inputInfo.KeyUp ? 1 : 0;
            int keyDown = inputInfo.KeyDown ? 1 : 0;

            m_camera.Move(keyRight - keyLeft, keyUp - keyDown, keyForward - keyBack, elapsedMs, inputInfo.KeySlow);

            UpdateFrame(elapsedMs);
            RenderFrame(elapsedMs);
        }

        private void UpdateModelFilter()
        {
            var lowerBound = new Vector3(float.MaxValue);
            var upperBound = new Vector3(float.MinValue);
            foreach (RegionModel region in m_simulationModel.Regions)
            {
                Vector3 regionLowerBound = region.Position - region.HalfSize;
                Vector3 regionUpperBound = region.Position + region.HalfSize;

                lowerBound.X = Math.Min(lowerBound.X, regionLowerBound.X);
                lowerBound.Y = Math.Min(lowerBound.Y, regionLowerBound.Y);
                lowerBound.Z = Math.Min(lowerBound.Z, regionLowerBound.Z);

                upperBound.X = Math.Max(upperBound.X, regionUpperBound.X);
                upperBound.Y = Math.Max(upperBound.Y, regionUpperBound.Y);
                upperBound.Z = Math.Max(upperBound.Z, regionUpperBound.Z);
            }

            Vector3 size = upperBound - lowerBound;
            Vector3 position = lowerBound + size/2;

            m_conductor.ModelProvider.Filter = new ModelFilter
            {
                Boxes = {new FilterBox(position, size)}
            };
        }

        private IEnumerable<IModel> AllModels()
        {
            return new List<IModel> {m_gridModel, m_simulationModel};
        }

        private void UpdateFrame(float elapsedMs)
        {
            foreach (IModel model in AllModels())
                model?.Update(elapsedMs);
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
            var opaqueModels = new List<IModel>();
            var translucentModels = new List<IModel>();

            // TODO: Do this only when necessary.
            foreach (IModel model in AllModels())
                CollectModels(model, ref opaqueModels, ref translucentModels);

            // TODO: Only if the camera is moved.
            SortModels(translucentModels);

            m_modelsDisplayed = opaqueModels.Count + translucentModels.Count;

            // Debug only.
            m_pickRay?.Render();

            // Render here.
            foreach (IModel model in opaqueModels)
                model.Render(elapsedMs);

            foreach (IModel model in translucentModels)
                model.Render(elapsedMs);
        }

        private void RenderOverlay()
        {
            if (m_font == null)
                return;

            // The fonts setup the same projection, but we also need to draw rectangles etc.
            // So we'll set up our own on the bottom of theirs.
            SetupOverlayProjection();

            RenderPickedInfo();

            RenderDiagnostics();

            // Tear down our projection in case there is some more drawing happening.
            TeardownOverlayProjection();
        }

        private void RenderPickedInfo()
        {
            foreach (NeuronModel neuron in m_pickedNeurons)
            {
                bool isBehindCamera;
                Vector3 screenPosition = ModelToScreenCoordinates(neuron, out isBehindCamera);

                if (isBehindCamera)
                    continue;

                RenderNeuronInfo(neuron, screenPosition);
            }
        }

        private void RenderNeuronInfo(NeuronModel neuron, Vector3 screenPosition)
        {
            screenPosition.X += 10;
            screenPosition.Y += 10;

            using (Blender.AveragingBlender())
            {
                GL.PushMatrix();
                GL.Translate(screenPosition);

                GL.Color4(new Color4(100, 100, 100, 220));
                GL.Begin(PrimitiveType.Quads);

                GL.Vertex3(0, 0, 0);
                GL.Vertex3(150, 0, 0);
                GL.Vertex3(150, 30, 0);
                GL.Vertex3(0, 30, 0);

                GL.End();
                GL.PopMatrix();

                QFont.Begin();
                GL.Translate(10, 0, 0);
                GL.Translate(screenPosition);

                m_font.Print($"{neuron.Position}", QFontAlignment.Left);

                QFont.End();
            }
        }

        private void SetupOverlayProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix(); //push projection matrix
            GL.LoadIdentity();
            GL.Ortho(0, m_control.Size.Width, m_control.Size.Height, 0, -1.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();  //push modelview matrix
            GL.LoadIdentity();
        }

        private void TeardownOverlayProjection()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix(); //pop modelview

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix(); //pop projection

            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void RenderDiagnostics()
        {
            QFont.Begin();
            GL.PushMatrix();

            GL.Translate(m_font.MonoSpaceWidth, 0, 0);
            m_font.Print($"fps: {(int) m_fps}", QFontAlignment.Left);

            GL.Translate(0, m_font.LineSpacing, 0);
            m_font.Print($"# of models: {m_modelsDisplayed}", QFontAlignment.Left);

            GL.PopMatrix();
            QFont.End();
        }

        private void SortModels(List<IModel> models)
        {
            foreach (IModel model in models)
                m_translucentDistanceCache[model] = model.CurrentWorldMatrix.ExtractTranslation().DistanceFrom(m_camera.Position);

            models.Sort(
                (model1, model2) => m_translucentDistanceCache[model1] < m_translucentDistanceCache[model2]
                    ? 1
                    : m_translucentDistanceCache[model1] > m_translucentDistanceCache[model2] ? -1 : 0);
        }

        private static void CollectModels(IModel model, ref List<IModel> opaqueModels, ref List<IModel> translucentModels)
        {
            // This can happen if there is no SimulationModel (yet?).
            if (model == null)
                return;

            if (!model.Visible)
                return;

            var compositeModel = model as ICompositeModel;
            if (compositeModel != null)
                foreach (IModel child in compositeModel.GenericModels)
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

            GL.Viewport(0, 0, m_control.Size.Width, m_control.Size.Height); // Use all of the glControl painting area
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            float aspectRatio = m_control.Size.Width / (float)m_control.Size.Height;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), aspectRatio, NearZ, FarZ);
            Matrix4 perspective = ProjectionMatrix;
            GL.LoadMatrix(ref perspective);
        }

        private void RenderEnd()
        {
            GL.Flush();

            m_control.SwapBuffers();
        }
    }

    public struct InputInfo
    {
        public bool KeyRight { get; set; }
        public bool KeyLeft { get; set; }
        public bool KeyForward { get; set; }
        public bool KeyBack { get; set; }
        public bool KeyUp { get; set; }
        public bool KeyDown { get; set; }

        public bool KeySlow { get; set; }

        public bool CameraRotated { get; set; }
        public float CameraDeltaX { get; set; }
        public float CameraDeltaY { get; set; }

        public bool ShouldStop { get; set; }
    }
}
