// 
// DictionaryVisualizer.cs
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
    public class DictionaryVisualizer : IValueVisualizer
    {
        Gtk.Entry filterEntryKeys;
        Gtk.Entry filterEntryValues;
        Gtk.TreeModelFilter filter;
        Gtk.TreeView tree;

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Collections.Generic.Dictionary<string,string>";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            string[] keyCollection;
            string[] valueCollection;
            
            KeyValuePair<string, string> [] kvp;
            Array keyColl;
            int count;
            try
            {
                RawValue dict = (RawValue) val.GetRawValue();
                string rep1 = (string)dict.CallMethod("ToString");
                count = (int) dict.GetMemberValue("Count");

                // THE ENUMERATOR METHOD - RESULT IS 
                // EXCEPTION: "Unable to cast object of type 'Mono.Debugger.Soft.StructMirror' to type 'Mono.Debugger.Soft.ObjectMirror'."

                keyCollection = new string[count];
                valueCollection = new string[count];

                //RawValue enumerator = (RawValue)dict.CallMethod("GetEnumerator");
                //string rep2 = (string)enumerator.CallMethod("ToString");

                //for (int indx = 0; indx < count; indx++)
                //{
                //    enumerator.CallMethod("MoveNext");
                //    RawValue current = (RawValue)enumerator.GetMemberValue("Current");
                //    string key = (string)current.GetMemberValue("Key");
                //    string values = (string)current.GetMemberValue("Value");
                //}

                //RawValue keys = (RawValue)dict.GetMemberValue("Keys");
                //RawValue keysEnum = (RawValue)keys.CallMethod("GetEnumerator");
                //for (int indx = 0; indx < count; indx++)
                //{
                //    bool res = (bool) keysEnum.CallMethod("MoveNext");
                //    string keyCurrent = (string) keysEnum.GetMemberValue("Current");
                //    keyCollection[indx] = keyCurrent;
                //}

                // THE ARRAY METHOD - RESULT IS NULL
                //keyCollection = new string[count];

                object[] parameters = { keyCollection, 0 };
                RawValue keys = (RawValue)dict.GetMemberValue("Keys");
                string rep2 = (string)keys.CallMethod("ToString");
                //RawValue cast = (RawValue)keys.CallMethod("Cast<string>");
                //RawValueArray arr = (RawValueArray)keys.CallMethod("ToArray");
                //keyColl = arr.ToArray();
            }
            finally
            {
            }

            Gtk.ScrolledWindow scrolled = new Gtk.ScrolledWindow();
            scrolled.HscrollbarPolicy = PolicyType.Automatic;
            scrolled.VscrollbarPolicy = PolicyType.Automatic;
            scrolled.ShadowType = ShadowType.In;

            // Create a nice label describing the Key
            Gtk.Label keyLabel = new Gtk.Label("[Key Search]");
            
            // Create a nice label describing the Value
            Gtk.Label valueLabel = new Gtk.Label("[Value Search]");

            Gtk.Label emptyLabel = new Gtk.Label();

            // Put them both into a little box so they show up side by side
            Gtk.HBox filterLabelBox = new Gtk.HBox();
            filterLabelBox.PackStart(keyLabel, true, true, 0);
            filterLabelBox.PackStart(valueLabel, true, true, 0);
            filterLabelBox.PackStart(emptyLabel, false, true, 0);

            // Create an Entry used to filter the tree
            filterEntryKeys = new Gtk.Entry();

            filterEntryValues = new Gtk.Entry();

            // Create a Search button
            Gtk.Button filterButton = new Gtk.Button("Search");
            filterButton.Clicked += OnfilterButtonClicked;

            // Put them both into a little box so they show up side by side
            Gtk.HBox filterBox = new Gtk.HBox();
            //filterBox.PackStart (filterLabel, false, false, 5);
            filterBox.PackStart(filterEntryKeys, true, true, 5);
            filterBox.PackStart(filterEntryValues, true, true, 5);
            filterBox.PackStart(filterButton, false, true, 5);

            // Create our TreeView
            tree = new Gtk.TreeView();
            scrolled.Add(tree);

            // Create a box to hold filterBox and Tree
            VBox box = new VBox(false, 1);

            // Add the widgets to the box
            box.PackStart(filterLabelBox, false, false, 5);
            box.PackStart(filterBox, false, false, 5);
            box.PackStart(scrolled, true, true, 5);

            // Create a column for the Dictionary Keys
            Gtk.TreeViewColumn keyColumn = new Gtk.TreeViewColumn();
            keyColumn.Title = "Keys";
            keyColumn.Alignment = 0.5f;
            keyColumn.Expand = true;

            // Create the text cell that will display the keys
            Gtk.CellRendererText keyCell = new Gtk.CellRendererText();
            keyCell.Xalign = 0.5f;
            // Add the cell to the column
            keyColumn.PackStart(keyCell, true);

            // Create a column for the Dictionary Values
            Gtk.TreeViewColumn valueColumn = new Gtk.TreeViewColumn();
            valueColumn.Title = "Values";
            valueColumn.Alignment = 0.5f;
            valueColumn.Expand = true;

            // Cell for the value column
            Gtk.CellRendererText valueCell = new Gtk.CellRendererText();
            valueCell.Xalign = 0.5f;
            valueColumn.PackStart(valueCell, true);

            // Add the columns to the TreeView
            tree.AppendColumn(keyColumn);
            tree.AppendColumn(valueColumn);

            // Tell the Cell Renderers which items in the model to display
            keyColumn.AddAttribute(keyCell, "text", 0);
            valueColumn.AddAttribute(valueCell, "text", 1);

            // Create a model that will hold two strings - Keys and Values
            Gtk.ListStore dicStore = new Gtk.ListStore(typeof(string), typeof(string));

            // Add some data to the store
            dicStore.AppendValues("BT", "Circles");
            dicStore.AppendValues("Daft Punk", "Technologic");
            dicStore.AppendValues("Daft Punk", "Digital Love");
            dicStore.AppendValues("The Crystal Method", "PHD");
            dicStore.AppendValues("The Crystal Method", "Name of the game");
            dicStore.AppendValues("The Chemical Brothers", "Galvanize");

            // Instead of assigning the ListStore model directly to the TreeStore, we create a TreeModelFilter
            // which sits between the Model (the ListStore) and the View (the TreeView) filtering what the model sees.
            // Some may say that this is a "Controller", even though the name and usage suggests that it is still part of
            // the Model.
            filter = new Gtk.TreeModelFilter(dicStore, null);

            // Specify the function that determines which rows to filter out and which ones to display
            filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);

            // Assign the filter as our tree's model
            tree.Model = dicStore;
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
            string key = model.GetValue(iter, 0).ToString();
            string value = model.GetValue(iter, 1).ToString();

            if (filterEntryKeys.Text == "" && filterEntryValues.Text == "")
                return true;

            if (key.IndexOf(filterEntryKeys.Text) > -1 && filterEntryValues.Text == "")
                return true;

            if (filterEntryKeys.Text == "" && value.IndexOf(filterEntryValues.Text) > -1)
                return true;

            if (key.IndexOf(filterEntryKeys.Text) > -1 && value.IndexOf(filterEntryValues.Text) > -1)
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
                return GettextCatalog.GetString("Dictionary");
            }
        }
    }
}
