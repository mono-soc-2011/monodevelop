// 
// XmlDocumentVisualizer.cs
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
using Gtk;
using MonoDevelop.Core;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace MonoDevelop.Debugger.Visualizer
{
    public class XmlDocumentVisualizer : IValueVisualizer
    {


        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Xml.XmlDocument";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            string parsedXml;
           
            try
            {
                RawValue rw = (RawValue)val.GetRawValue();

                string xml = (string)rw.GetMemberValue("InnerXml");

                xml = xml.Replace("\r\n", string.Empty); // Remove any carriage return chatacters

                System.Xml.Linq.XElement element = System.Xml.Linq.XElement.Parse(xml);
			    // parse xml string using Xml Linq to put it into XML format
    		    parsedXml = element.ToString();
            }
            finally
            {
            }

            Gtk.ScrolledWindow scrolled = new Gtk.ScrolledWindow();
            scrolled.HscrollbarPolicy = PolicyType.Automatic;
            scrolled.VscrollbarPolicy = PolicyType.Automatic;
            scrolled.ShadowType = ShadowType.In;

            // Create a box to hold the Entry and Tree
            VBox box = new VBox(false, 1);
            TextView textView = new TextView();
            textView.Buffer.Text = parsedXml;

            textView.WrapMode = WrapMode.None;

            scrolled.Add(textView);
           
            scrolled.ShowAll();
            return scrolled;
        }

        public bool StoreValue(ObjectValue val)
        {
            return true;
        }


        public string Name
        {
            get
            {
                return GettextCatalog.GetString("XmlDocument");
            }
        }
    }
}
