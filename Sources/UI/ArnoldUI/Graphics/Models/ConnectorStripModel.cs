using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoodAI.Arnold.Graphics.Models
{
    public enum ConnectorStripType
    {
        Input,
        Output
    }

    public class ConnectorStripModel<TConnector> : CompositeModelBase<TConnector> where TConnector : ConnectorModel
    {
        private readonly RegionModel m_region;
        private readonly ConnectorStripType m_stripType;
        public int TotalSlots { get; set; }

        public ConnectorStripModel(RegionModel region, ConnectorStripType stripType)
        {
            m_region = region;
            m_stripType = stripType;
        }

        protected override void UpdateModel(float elapsedMs)
        {
            // TODO(HonzaS): Only recalculate if something changed.

            // Set this strip's position.
            float positionX = m_stripType == ConnectorStripType.Input ? -m_region.HalfSize.X : m_region.HalfSize.X;
            Position = new Vector3(positionX, 0, 0);

            // Starting position of the connector in the strip.
            var position = -m_region.HalfSize.Z;

            foreach (TConnector connector in Children)
            {
                float sizeZ = (float) connector.Slots/TotalSlots * m_region.Size.Z;

                // Positioned relatively to the strip, which is in the center of the input or output face.
                connector.Reposition(position, sizeZ);

                position += sizeZ;
            }
        }

        protected override void RenderModel(float elapsedMs)
        {
        }

        public override void AddChild(TConnector child)
        {
            base.AddChild(child);

            TotalSlots += child.Slots;
        }
    }
}
