// 
// GdkColorVisualizer.cs
//  
// Author:
//       Abdul Rauf <raufbutt@gmail.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
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

using System;
using Mono.Debugging.Client;
using System.IO;
using System.Drawing;
using Gtk;
using MonoDevelop.Core;

namespace MonoDevelop.Debugger.Visualizer
{
    public class GdkColorVisualizer : IValueVisualizer
    {
        #region IValueVisualizer implementation

        public GdkColorVisualizer()
        {
        }

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "Gdk.Color";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            Gdk.Color colour;
            try
            {
                RawValue rw = (RawValue)val.GetRawValue();
                UInt16 red = (UInt16)rw.GetMemberValue("Red");
                UInt16 green = (UInt16)rw.GetMemberValue("Green");
                UInt16 blue = (UInt16)rw.GetMemberValue("Blue");
                colour = new Gdk.Color((byte)red, (byte)green, (byte)blue);

            }
            finally
            {
            }
            VBox vbox = new VBox(false, 2);
            Gtk.ScrolledWindow sc = new Gtk.ScrolledWindow();
            sc.ShadowType = Gtk.ShadowType.In;
            sc.HscrollbarPolicy = Gtk.PolicyType.Never;
            sc.VscrollbarPolicy = Gtk.PolicyType.Never;
           
            Gtk.DrawingArea box = new Gtk.DrawingArea();
            box.ModifyBg(StateType.Normal, colour);
            sc.Add(box);
            vbox.PackStart(sc, true, true, 0);

            TextView tv = new Gtk.TextView();
            tv.Buffer.Text = "RGB String is " + colour.ToString();
            vbox.PackStart(tv, false, true, 10);
            
            vbox.ShowAll();
            return vbox;
        }


        public bool StoreValue(ObjectValue val)
        {
            return true;
        }


        public string Name{
            get{
                return GettextCatalog.GetString ("Color Swatch");
            }
        }

        #endregion
   
    } 
}

