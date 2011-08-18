// 
// List<string>Visualizer.cs
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
    public class ListVisualizer : IValueVisualizer
    {
        
        Gtk.Entry filterEntryValues;
        Gtk.TreeModelFilter filter;
        Gtk.TreeView tree;

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Collections.Generic.List<string>";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            Array values;
            int count;
            try
            {
                RawValue list = (RawValue) val.GetRawValue();
                string rep1 = (string)list.CallMethod("ToString");
                count = (int) list.GetMemberValue("Count");

                // THE ARRAY METHOD 
                RawValueArray arr = (RawValueArray) list.CallMethod("ToArray");
                values = arr.ToArray();
            }
            finally
            {
            }

            Gtk.ScrolledWindow scrolled = new Gtk.ScrolledWindow();
            scrolled.HscrollbarPolicy = PolicyType.Automatic;
            scrolled.VscrollbarPolicy = PolicyType.Automatic;
            scrolled.ShadowType = ShadowType.In;
            
            // Create a nice label describing the Entry
            Gtk.Label filterLabel = new Gtk.Label("Value Search");

            // Create an Entry used to filter the tree
            filterEntryValues = new Gtk.Entry();

            //filterEntryValues.Backspace += OnFilterEntryValueBackspace;
            //filterEntryValues.Changed += OnFilterEntryValueChanged;

            // Create a Search Button
            Gtk.Button filterButton = new Gtk.Button("Search");
            filterButton.Clicked += OnfilterButtonClicked;

            // Put them both into a little box so they show up side by side
            Gtk.HBox filterBox = new Gtk.HBox();
            filterBox.PackStart (filterLabel, false, false, 5);
            filterBox.PackStart(filterEntryValues, true, true, 5);
            filterBox.PackStart(filterButton, false, true, 5);

            // Create our TreeView
            tree = new Gtk.TreeView();
            tree.WidthRequest = 200;
            scrolled.Add(tree);

            // Create a box to hold the Entry and Tree
            VBox box = new VBox(false, 1);

            // Add the widgets to the box
            box.PackStart(filterBox, false, false, 5);
            box.PackStart(scrolled, true, true, 5);

            #region "Tree Build UI code"

            // Create a column for the List index
            Gtk.TreeViewColumn numColumn = new Gtk.TreeViewColumn();
            numColumn.Title = "List Index";
            numColumn.Alignment = 0.5f;
            numColumn.Expand = false;

            // Create a column for the List Values
            Gtk.TreeViewColumn valueColumn = new Gtk.TreeViewColumn();
            valueColumn.Title = "Values";
            valueColumn.Alignment = 0.5f;
            valueColumn.Expand = true;

            // Create a cell that will display index
            Gtk.CellRendererText numCell = new Gtk.CellRendererText();
            numCell.Xalign = 0.5f;
            numColumn.PackStart(numCell, false);

            // Create a cell that will display values
            Gtk.CellRendererText valueCell = new Gtk.CellRendererText();
            valueCell.Xalign = 0.5f;
            valueColumn.PackStart(valueCell, true);

            // Add the columns to the TreeView
            tree.AppendColumn(numColumn);
            tree.AppendColumn(valueColumn);

            // Tell the Cell Renderers which items in the model to display
            numColumn.AddAttribute(numCell, "text", 0);
            valueColumn.AddAttribute(valueCell, "text", 1);

            // Create a model that will hold strings of the List
            Gtk.ListStore listStore = new Gtk.ListStore(typeof(int), typeof(string));

            // Add some data to the store
            int indx = 0;
            foreach (string str in values)
            {
                listStore.AppendValues(indx, str);
                indx++;
            }

            // Instead of assigning the ListStore model directly to the TreeStore, we create a TreeModelFilter
            // which sits between the Model (the ListStore) and the View (the TreeView) filtering what the model sees.
            // Some may say that this is a "Controller", even though the name and usage suggests that it is still part of
            // the Model.
            filter = new Gtk.TreeModelFilter(listStore, null);

            // Specify the function that determines which rows to filter out and which ones to display
            filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);

            // Assign the filter as our tree's model
            tree.Model = listStore;

            #endregion

            box.ShowAll();
            return box;
        }

        private void OnfilterButtonClicked(object o, System.EventArgs args)
        {
            // Since the filter text changed, tell the filter to re-determine which rows to display
            tree.Model = filter;
            filter.Refilter();
        }

        private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
        {
            string value = model.GetValue(iter, 1).ToString();

            if (filterEntryValues.Text == "")
                return true;

            if (value.IndexOf(filterEntryValues.Text) > -1)
                return true;

            else
                return false;
        }

        public bool StoreValue(ObjectValue val)
        {
            return true;
        }


        public string Name{
            get{
                return GettextCatalog.GetString("List<string>");
            }
        }
    }
}
