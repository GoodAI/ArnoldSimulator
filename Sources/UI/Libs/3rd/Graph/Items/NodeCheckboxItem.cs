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
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Graph.Items
{
    public sealed class CheckboxValueChangedEventArgs : CancelEventArgs
    {
        public CheckboxValueChangedEventArgs(bool isChecked) { Checked = isChecked; }
        public CheckboxValueChangedEventArgs(bool isChecked, bool cancel) : base(cancel) { Checked = isChecked; }
        public bool Checked { get; private set; }
    }

	public sealed class NodeCheckboxItem : NodeItem
	{
        public event EventHandler<CheckboxValueChangedEventArgs> CheckedChanged;

		public NodeCheckboxItem(string text, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
		}

		#region Text
		string internalText = string.Empty;
		public string Text
		{
			get { return internalText; }
			set
			{
				if (internalText == value)
					return;
				internalText = value;
				TextSize = Size.Empty;
			}
		}
		#endregion

		#region Checked
		bool internalChecked = false;
		public bool Checked
		{
			get { return internalChecked; }
			set
			{
				if (internalChecked == value)
					return;
                if (CheckedChanged != null)
                {
                    CheckboxValueChangedEventArgs args = new CheckboxValueChangedEventArgs(internalChecked);
                    CheckedChanged(this, args);
                    if (args.Cancel)
                        return;
                    internalChecked = value;
                }
                else
                {
                    internalChecked = value;
                }

                TextSize = Size.Empty;
			}
		}
		#endregion


		public override bool OnClick()
		{
			base.OnClick();
			Checked = !Checked;
			return true;
		}

		internal SizeF TextSize;


		internal override SizeF Measure(Graphics graphics)
		{
			if (!string.IsNullOrWhiteSpace(this.Text))
			{
				if (this.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

					this.TextSize = graphics.MeasureString(this.Text, SystemFonts.MenuFont, size, GraphConstants.CenterMeasureTextStringFormat);

					this.TextSize.Width	 = Math.Max(size.Width, this.TextSize.Width);
					this.TextSize.Height = Math.Max(size.Height, this.TextSize.Height);
				}

				return this.TextSize;
			} else
			{
				return new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight + GraphConstants.TopHeight);
			}
		}

		internal override void Render(Graphics graphics, SizeF minimumSize, PointF location)
		{
			var size = Measure(graphics);
			size.Width  = Math.Max(minimumSize.Width, size.Width);
			size.Height = Math.Max(minimumSize.Height, size.Height);
			
			using (var path = GraphRenderer.CreateRoundedRectangle(size, location))
			{
				var rect = new RectangleF(location, size);
				if (this.Checked)
				{
					using (var brush = new SolidBrush(Color.FromArgb(128+32, Color.White)))
					{
						graphics.FillPath(brush, path);
					}
				} else
				{
					using (var brush = new SolidBrush(Color.FromArgb(64, Color.Black)))
					{
						graphics.FillPath(brush, path);
					}
				}
				graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, rect, GraphConstants.CenterTextStringFormat);

				if ((state & RenderState.Hover) != 0)
					graphics.DrawPath(Pens.White, path);
				else	
					graphics.DrawPath(Pens.Black, path);
			}
		}
	}
}
