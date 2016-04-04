﻿#region License
// Copyright (c) 2009 Sander van Rossen
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	internal sealed class NodeTitleItem : NodeItem
	{
		#region Text
		string			internalTitle = string.Empty;
		public string	Title		
		{
			get { return internalTitle; }
			set
			{
				if (internalTitle == value)
					return;
				internalTitle = value;
				ForceResize();
			}
		}
		#endregion

        #region Icon
        Image internalIcon = null;
        public Image Icon
        {
            get
            {
                return internalIcon;
            }
            set
            {
                internalIcon = value;
            }
        }

        private float IconXOffset
        {
            get
            {
                if (Icon == null) return 0.0f;
                else return Icon.Size.Width + 5.0f;
            }
        }

        private float IconHeight
        {
            get
            {
                if (Icon == null) return 0.0f;
                else return Icon.Size.Height;
            }
        }
        #endregion

		internal void ForceResize() { TextSize = Size.Empty; }
		internal SizeF				TextSize;

		internal override SizeF Measure(Graphics graphics)
		{
			if (!string.IsNullOrWhiteSpace(this.Title))
			{
				if (this.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight);
					this.TextSize			= graphics.MeasureString(this.Title, SystemFonts.CaptionFont, size, GraphConstants.TitleMeasureStringFormat);

					this.TextSize.Width		= Math.Max(size.Width,  this.TextSize.Width + (GraphConstants.CornerSize * 2)) + IconXOffset;
					this.TextSize.Height	= Math.Max(size.Height, Math.Max(this.TextSize.Height, IconHeight)) + GraphConstants.TopHeight;
				}
				return this.TextSize;
			} else
			{
                return new SizeF(GraphConstants.MinimumItemWidth + IconXOffset, Math.Max(GraphConstants.TitleHeight, IconHeight) + GraphConstants.TopHeight);
			}
		}

		internal override void Render(Graphics graphics, SizeF minimumSize, PointF location)
		{
            if (internalIcon != null)
            {
                location.X += IconXOffset * 0.5f;

                PointF pos = Node.bounds.Location;
                pos.X += 5.0f;
                pos.Y += 1.0f;
                var rect = new RectangleF(pos, internalIcon.Size);

                graphics.DrawImage(internalIcon, rect);
            }

			var size = Measure(graphics);
			size.Width  = Math.Max(minimumSize.Width, size.Width);
			size.Height = Math.Max(minimumSize.Height, size.Height);

			size.Height -= GraphConstants.TopHeight;
			location.Y += GraphConstants.TopHeight;

			if ((state & RenderState.Hover) == RenderState.Hover)
				graphics.DrawString(this.Title, SystemFonts.CaptionFont, Brushes.White, new RectangleF(location, size), GraphConstants.TitleStringFormat);
			else
				graphics.DrawString(this.Title, SystemFonts.CaptionFont, Brushes.Black, new RectangleF(location, size), GraphConstants.TitleStringFormat);
		}
	}
}
