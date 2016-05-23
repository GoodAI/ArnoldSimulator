using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoodAI.Arnold.Graphics.Models
{
    public abstract class ConnectorStripModel<TConnector> : CompositeModelBase<TConnector> where TConnector : ConnectorModel
    {
        protected readonly RegionModel m_region;
        public int TotalSlots { get; set; }

        public ConnectorStripModel(RegionModel region)
        {
            m_region = region;
        }

        protected abstract Vector3 AdjustedPosition { get; }

        protected override void UpdateModel(float elapsedMs)
        {
            // TODO(HonzaS): Only recalculate if something changed.
            Position = AdjustedPosition;

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

    public class InputConnectorStripModel : ConnectorStripModel<InputConnectorModel>
    {
        public InputConnectorStripModel(RegionModel region) : base(region)
        {
        }

        protected override Vector3 AdjustedPosition => new Vector3(-m_region.HalfSize.X, 0, 0);
    }

    public class OutputConnectorStripModel : ConnectorStripModel<OutputConnectorModel>
    {
        public OutputConnectorStripModel(RegionModel region) : base(region)
        {
        }

        protected override Vector3 AdjustedPosition => new Vector3(m_region.HalfSize.X, 0, 0);
    }
}
