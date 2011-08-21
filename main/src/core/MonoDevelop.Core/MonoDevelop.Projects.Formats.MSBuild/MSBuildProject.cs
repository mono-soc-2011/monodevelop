// MSBuildProject.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
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
//
//

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MonoDevelop.Projects.Formats.MSBuild
{
	class MSBuildProject : MSBuildItemGroupCollection
	{
		public MSBuildProjectFilter projectFilter;

		public MSBuildProject ()
		{
		}
		
		public string DefaultTargets {
			get { return doc.DocumentElement.GetAttribute ("DefaultTargets"); }
			set { doc.DocumentElement.SetAttribute ("DefaultTargets", value); }
		}
		
		public void AddNewImport (string name, string condition)
		{
			XmlElement elem = doc.CreateElement (null, "Import", MSBuildProject.Schema);
			elem.SetAttribute ("Project", name);
			
			XmlElement last = doc.DocumentElement.SelectSingleNode ("tns:Import[last()]", XmlNamespaceManager) as XmlElement;
			if (last != null)
				doc.DocumentElement.InsertAfter (elem, last);
			else
				doc.DocumentElement.AppendChild (elem);
		}
		
		public void RemoveImport (string name)
		{
			XmlElement elem = (XmlElement) doc.DocumentElement.SelectSingleNode ("tns:Import[@Project='" + name + "']", XmlNamespaceManager);
			if (elem != null)
				elem.ParentNode.RemoveChild (elem);
			else
				Console.WriteLine ("ppnf:");
		}
		
		public List<string> Imports {
			get {
				List<string> ims = new List<string> ();
				foreach (XmlElement elem in doc.DocumentElement.SelectNodes ("tns:Import", XmlNamespaceManager)) {
					ims.Add (elem.GetAttribute ("Project"));
				}
				return ims;
			}
		}
		
		public MSBuildPropertySet GetGlobalPropertyGroup ()
		{
			MSBuildPropertyGroupMerged res = new MSBuildPropertyGroupMerged ();
			foreach (MSBuildPropertyGroup grp in PropertyGroups) {
				if (grp.Condition.Length == 0)
					res.Add (grp);
			}
			return res.GroupCount > 0 ? res : null;
		}
		
		public MSBuildPropertyGroup AddNewPropertyGroup (bool insertAtEnd)
		{
			XmlElement elem = doc.CreateElement (null, "PropertyGroup", MSBuildProject.Schema);
			
			XmlElement last = doc.DocumentElement.SelectSingleNode ("tns:PropertyGroup[last()]", XmlNamespaceManager) as XmlElement;
			if (last != null)
				doc.DocumentElement.InsertAfter (elem, last);
			else
				doc.DocumentElement.AppendChild (elem);
			
			return GetGroup (elem);
		}
		
		public IEnumerable<MSBuildItem> GetAllItems ()
		{
			foreach (XmlElement elem in doc.DocumentElement.SelectNodes ("tns:ItemGroup/*", XmlNamespaceManager)) {
				yield return GetItem (elem);
			}
		}
		
		public IEnumerable<MSBuildItem> GetAllItems (params string[] names)
		{
			string name = string.Join ("|tns:ItemGroup/tns:", names);
			foreach (XmlElement elem in doc.DocumentElement.SelectNodes ("tns:ItemGroup/tns:" + name, XmlNamespaceManager)) {
				yield return GetItem (elem);
			}
		}
		
		public IEnumerable<MSBuildPropertyGroup> PropertyGroups {
			get {
				foreach (XmlElement elem in doc.DocumentElement.SelectNodes ("tns:PropertyGroup", XmlNamespaceManager))
					yield return GetGroup (elem);
			}
		}
		
		public string GetProjectExtensions (string section)
		{
			XmlElement elem = doc.DocumentElement.SelectSingleNode ("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as XmlElement;
			if (elem != null)
				return elem.InnerXml;
			else
				return string.Empty;
		}
		
		public void SetProjectExtensions (string section, string value)
		{
			XmlElement elem = doc.DocumentElement ["ProjectExtensions", MSBuildProject.Schema];
			if (elem == null) {
				elem = doc.CreateElement (null, "ProjectExtensions", MSBuildProject.Schema);
				doc.DocumentElement.AppendChild (elem);
			}
			XmlElement sec = elem [section];
			if (sec == null) {
				sec = doc.CreateElement (null, section, MSBuildProject.Schema);
				elem.AppendChild (sec);
			}
			sec.InnerXml = value;
		}

		public void RemoveProjectExtensions (string section)
		{
			XmlElement elem = doc.DocumentElement.SelectSingleNode ("tns:ProjectExtensions/tns:" + section, XmlNamespaceManager) as XmlElement;
			if (elem != null) {
				XmlElement parent = (XmlElement) elem.ParentNode;
				parent.RemoveChild (elem);
				if (!parent.HasChildNodes)
					parent.ParentNode.RemoveChild (parent);
			}
		}
	}
}
