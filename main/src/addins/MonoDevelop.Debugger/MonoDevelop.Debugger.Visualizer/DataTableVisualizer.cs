// 
// DataTableVisualizer.cs
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
using System.IO;

namespace MonoDevelop.Debugger.Visualizer
{
    public class DataTableVisualizer : IValueVisualizer
    {
        
        Gtk.TreeView tree;

        public bool CanVisualize(ObjectValue val)
        {
            return val.TypeName == "System.Data.DataTable";
        }

        public bool CanEdit(ObjectValue val)
        {
            return false;
        }

        public Gtk.Widget GetVisualizerWidget(ObjectValue val)
        {
            int rowsCount, colsCount;
            string fileXML = Path.GetTempFileName();
            string fileXMLSchema = Path.GetTempFileName();

            string TableName = "";
            DataSet DS_table = new DataSet();
            try
            {
                RawValue dt = (RawValue) val.GetRawValue();                
                RawValue cols = (RawValue) dt.GetMemberValue("Columns");
                RawValue colList = (RawValue)cols.GetMemberValue("List"); // Not being work because of its protection level

                RawValue rows = (RawValue)dt.GetMemberValue("Rows");
                RawValue rowList = (RawValue)rows.GetMemberValue("List"); // Not working because of its protection level

                colsCount = (int)cols.GetMemberValue("Count"); // get columns count
                rowsCount = (int)rows.GetMemberValue("Count"); // get rows count

                // THE ENUMERATOR METHOD - RESULT IS 
                // EXCEPTION: "Unable to cast object of type 'Mono.Debugger.Soft.StructMirror' to type 'Mono.Debugger.Soft.ObjectMirror'."

                //keyCollection = new string[count];
                //valueCollection = new string[count];
                //RawValue enumerator = (RawValue) rw.CallMethod("GetEnumerator");

                //for (int indx = 0; indx < count; indx++)
                //{
                //    enumerator.CallMethod("MoveNext");
                //    RawValue current = (RawValue) enumerator.GetMemberValue("Current");
                //    keyCollection[indx] = (string) current.GetMemberValue("Key");
                //    valueCollection[indx] = (string) current.GetMemberValue("Value");
                //}

                // THE FILE WRITE/READ METHOD
                string name = (string) dt.GetMemberValue("TableName");
                if (name == "")
                {
                    // This is to set the Datatable name, otherwise it wont get serialized
                    dt.SetMemberValue("TableName", "Unnamed");
                }

                dt.CallMethod("WriteXmlSchema", fileXMLSchema);
                // Call WriteXMLSchema to export null values as well
                // Cant use XmlWriteMode enumeration in WriteXml method because of the framework limitations
                dt.CallMethod("WriteXml", fileXML);

                DS_table.ReadXmlSchema(fileXMLSchema);
                DS_table.ReadXml(fileXML);
                int count = DS_table.Tables[0].Rows.Count;
                TableName = DS_table.Tables[0].TableName;

            }
            finally
            {
                File.Delete(fileXMLSchema);
                File.Delete(fileXML);
            }

            Gtk.ScrolledWindow scrolled = new Gtk.ScrolledWindow();
            scrolled.HscrollbarPolicy = PolicyType.Automatic;
            scrolled.VscrollbarPolicy = PolicyType.Automatic;
            scrolled.ShadowType = ShadowType.In;

            // Create info labels
            Gtk.Label NameLabel = new Gtk.Label("Table Name: [" + TableName + "]");
            Gtk.Label RowLabel = new Gtk.Label("Row Count = " + rowsCount);
            Gtk.Label ColLabel = new Gtk.Label("Column Count = " + colsCount);

            // Put them into a little box so they show up side by side
            Gtk.HBox TableBox = new Gtk.HBox();
            TableBox.PackStart(NameLabel, true, true, 0);
            TableBox.PackStart(RowLabel, true, true, 0);
            TableBox.PackStart(ColLabel, true, true, 0);

            // Create our TreeView
            tree = new Gtk.TreeView();
            tree.WidthRequest = 200;
            scrolled.Add(tree);

            // Create a box to hold info and Tree
            VBox box = new VBox(false, 1);

            // Add the widgets to the box
            box.PackStart(TableBox, false, false, 5);
            box.PackStart(scrolled, true, true, 5);

            #region "Tree Build UI code"

            Type[] types = new Type[colsCount];

                for (int colIndx = 0; colIndx < colsCount; colIndx++)
                {
                    // Create a column for Table Values
                    tree.AppendColumn(DS_table.Tables[0].Columns[colIndx].ColumnName, new CellRendererText(), "text", colIndx);
                    TreeViewColumn col = tree.GetColumn(colIndx);
                    col.Expand = true;

                    types[colIndx] = DS_table.Tables[0].Columns[colIndx].DataType;
                }

                // Create a model that will hold Table values
                Gtk.ListStore tableStore = new Gtk.ListStore(types);

                // Add data to the store
                for (int rowIndx = 0; rowIndx < rowsCount; rowIndx++)
                {
                    object[] arr = DS_table.Tables[0].Rows[rowIndx].ItemArray;
                    tableStore.AppendValues(arr);
                }

            // Assign the filter as our tree's model
            tree.Model = tableStore;

            #endregion

            box.ShowAll();
            return box;
        }

        public bool StoreValue(ObjectValue val)
        {
            return true;
        }


        public string Name{
            get{
                return GettextCatalog.GetString("Data Table");
            }
        }
    }
}
