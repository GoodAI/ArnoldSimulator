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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Graph
{
	public sealed class NodeConnectionEventArgs : EventArgs
	{
		public NodeConnectionEventArgs(NodeConnection connection) { Connection = connection; From = connection.From; To = connection.To; }
		public NodeConnectionEventArgs(NodeConnector from, NodeConnector to, NodeConnection connection) { Connection = connection; From = from; To = to; }
		public NodeConnector From { get; set; }
		public NodeConnector To { get; set; }
		public NodeConnection Connection { get; private set; }
	}

	public sealed class AcceptNodeConnectionEventArgs : CancelEventArgs
	{
		public AcceptNodeConnectionEventArgs(NodeConnection connection) { Connection = connection; }
		public AcceptNodeConnectionEventArgs(NodeConnection connection, bool cancel) : base(cancel) { Connection = connection; }
		public NodeConnection Connection { get; private set; }
	}

	public class NodeConnection : IElement
	{
		public event EventHandler<NodeConnectionEventArgs>	DoubleClick;

		public NodeConnector	From	{ get; set; }
		public NodeConnector	To		{ get; set; }
		public virtual string	Name	{ get; set; }
		public object			Tag		{ get; set; }
        public bool             IgnoreDragging { get; set; }

		internal RenderState	state;
		internal RectangleF		bounds;
		internal RectangleF		textBounds;

		internal void			DoDoubleClick() { if (DoubleClick != null) DoubleClick(this, new NodeConnectionEventArgs(this)); }

		public ElementType ElementType { get { return ElementType.Connection; } }
	}
}
