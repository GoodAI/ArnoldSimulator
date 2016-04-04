using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GoodAI.Arnold.Graphics
{
    public class GridModel : ModelBase
    {
        private readonly int m_width;
        private readonly int m_depth;
        private readonly int m_cellSize;
        private readonly int m_xStart;
        private readonly int m_yStart;

        private readonly Color4 m_baseLineColor = new Color4(0.8f, 0.8f, 0.8f, 1f);
        private readonly Color4 m_lineColor = new Color4(0.3f, 0.3f, 0.3f, 1f);

        public GridModel(int width, int depth, int cellSize)
        {
            m_width = width;
            m_depth = depth;
            m_cellSize = cellSize;

            m_xStart = -(m_width*m_cellSize/2);
            m_yStart = -(m_depth*m_cellSize/2);
        }

        protected override void UpdateModel(float elapsedMs)
        {
        }

        protected override void RenderModel(Camera camera, float elapsedMs)
        {
            using (Blender.TextureBlender())
            {
                GL.LineWidth(1f);

                GL.Begin(PrimitiveType.Lines);


                for (int i = 0; i < m_width; i++)
                {
                    for (int j = 0; j < m_depth; j++)
                    {
                        int x = m_xStart + (i*m_cellSize);
                        int y = 0;
                        int z = m_yStart + (j*m_cellSize);


                        GL.Color4(j == m_depth/2 ? m_baseLineColor : m_lineColor);

                        GL.Vertex3(x, y, z);
                        GL.Vertex3(x + m_cellSize, y, z);


                        GL.Color4(i == m_width/2 ? m_baseLineColor : m_lineColor);

                        GL.Vertex3(x, y, z);
                        GL.Vertex3(x, y, z + m_cellSize);
                    }
                }
                GL.End();
            }
        }
    }
}
