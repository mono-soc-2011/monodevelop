// 
// ColorVisualizer.cs
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
using MonoDevelop.Core;
using Gtk;

namespace MonoDevelop.Debugger.Visualizer
{
    public class ColorVisualizer : IValueVisualizer
    {
        #region IValueVisualizer implementation

        public ColorVisualizer()
        {
        }

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Drawing.Color";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            Gdk.Color GdkColor;
            byte alpha;
            try
            {
                RawValue rw = (RawValue)val.GetRawValue();
                //color = (string) rw.CallMethod("ToString","{0})"); CANT CALL any function on strcuture
                //Unable to cast object of type 'Mono.Debugger.Soft.Struct Mirror' to type 'Mono.Debugger.Soft.ObjectMirror'.
                alpha = (byte)rw.GetMemberValue("A");
                byte red = (byte)rw.GetMemberValue("R");
                byte green = (byte)rw.GetMemberValue("G");
                byte blue = (byte)rw.GetMemberValue("B");
                GdkColor = new Gdk.Color(red, green, blue);

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
            box.ModifyBg(StateType.Normal, GdkColor);
            sc.Add(box);
            vbox.PackStart(sc, true, true, 0);

            TextView tv = new Gtk.TextView();
            tv.Buffer.Text = "Alpha: " + alpha.ToString() + " RGB string: " + GdkColor.ToString();
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

