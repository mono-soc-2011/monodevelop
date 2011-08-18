// 
// ImageVisualizer.cs
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

namespace MonoDevelop.Debugger.Visualizer
{
    public class ImageVisualizer : IValueVisualizer
    {
        #region IValueVisualizer implementation

        public ImageVisualizer()
        {
        }

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Drawing.Bitmap";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            Gdk.Pixbuf pixbuf;
            int height, width;
            string file = Path.GetTempFileName();
            try
            {
                RawValue pix = (RawValue)val.GetRawValue();

                object[] parameters = { file };
                pix.CallMethod("Save", parameters);
                pixbuf = new Gdk.Pixbuf (file);

                height = pixbuf.Height;
                width = pixbuf.Width;
                //bmp = (Bitmap) val.GetRawValue();
                //bmp = new Bitmap(file);
            }
            finally
            {
                File.Delete(file);
            }
            Gtk.VBox vbox = new Gtk.VBox(false, 2);
            Gtk.ScrolledWindow sc = new Gtk.ScrolledWindow();
            sc.ShadowType = Gtk.ShadowType.In;
            sc.HscrollbarPolicy = Gtk.PolicyType.Automatic;
            sc.VscrollbarPolicy = Gtk.PolicyType.Automatic;
            Gtk.Image image = new Gtk.Image(pixbuf);
            sc.AddWithViewport(image);
            vbox.PackStart(sc, true, true, 0);

            Gtk.HBox hbox = new Gtk.HBox();
            Gtk.Label hLabel = new Gtk.Label();
            hLabel.Text = "Height = " + height.ToString();

            Gtk.Label wLabel = new Gtk.Label();
            wLabel.Text = "Width = " + width.ToString();
            hbox.PackStart(hLabel, true, true, 5);
            hbox.PackStart(wLabel, true, true, 5);

            vbox.PackStart(hbox, false, false, 5);

            vbox.ShowAll();
            return vbox;
        }


        public bool StoreValue(ObjectValue val)
        {
            return true;
        }


        public string Name{
            get{
                return GettextCatalog.GetString ("Image");
            }
        }

        #endregion
   
    } 
}

