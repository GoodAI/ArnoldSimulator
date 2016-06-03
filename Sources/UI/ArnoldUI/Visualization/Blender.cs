using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Visualization
{
    public class Blender : IDisposable
    {
        public Blender(BlendingFactorSrc source, BlendingFactorDest destination)
        {
            GL.BlendFunc(source, destination);
            GL.Enable(EnableCap.Blend);
        }

        public void Dispose()
        {
            GL.Disable(EnableCap.Blend);
        }

        public static Blender AveragingBlender()
        {
            return new Blender(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public static Blender MultiplicativeBlender()
        {
            return new Blender(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }
    }
}
