// 
// ExceptionVisualizer.cs
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
using System.Collections.Generic;
using System.Data;

namespace MonoDevelop.Debugger.Visualizer
{
    public class ExceptionVisualizer : IValueVisualizer
    {

        Gtk.TreeView tree;

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Exception";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            string msg, src, strace;
           
            try
            {
                RawValue rw = (RawValue)val.GetRawValue();
  
                msg = (string) rw.GetMemberValue("Message");
                src = (string) rw.GetMemberValue("Source");
                strace= (string) rw.GetMemberValue("StackTrace");

            }
            finally
            {
            }

            Gtk.ScrolledWindow scrolled = new Gtk.ScrolledWindow();
            scrolled.HscrollbarPolicy = PolicyType.Automatic;
            scrolled.VscrollbarPolicy = PolicyType.Automatic;
            scrolled.ShadowType = ShadowType.In;

            // Create our TreeView
            tree = new Gtk.TreeView();
            tree.BorderWidth = 1;

            // Create a box to hold the Entry and Tree
            VBox box = new VBox(false, 1);

            // Add tree widget to the box

            scrolled.Add(tree);

            //Create a column for the Exception Type
            Gtk.TreeViewColumn typeColumn = new Gtk.TreeViewColumn();
            typeColumn.Title = "Exception";
            typeColumn.Alignment = 0.5f;
            typeColumn.Expand = true;

            // Create the text cell that will display the keys
            Gtk.CellRendererText typeCell = new Gtk.CellRendererText();

            // Add the cell to the column
            typeColumn.PackStart(typeCell, true);

            // Add the columns to the TreeView
            tree.AppendColumn(typeColumn);

            // Tell the Cell Renderers which items in the model to display
            typeColumn.AddAttribute(typeCell, "text", 0);

            // Create a model that will hold two strings - Keys and Values
            Gtk.ListStore dicStore = new Gtk.ListStore(typeof(string), typeof(string));

            // Add some data to the store
            //dicStore.AppendValues(msg, src);

            Gtk.TreeIter iter = dicStore.AppendValues("Message");
            dicStore.AppendValues(iter, msg);

            iter = dicStore.AppendValues("Source");
            dicStore.AppendValues(iter, src);

            iter = dicStore.AppendValues("Stack Trace");
            dicStore.AppendValues(iter, strace);

            // Assign the filter as our tree's model
            tree.Model = dicStore;
            //tvStackTrace.Buffer.Text = strace;
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
                return GettextCatalog.GetString("Exception");
            }
        }
    }
}
