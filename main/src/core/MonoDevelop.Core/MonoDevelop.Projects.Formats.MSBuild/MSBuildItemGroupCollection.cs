// MSBuildItemGroupCollection.cs
//
// Author:
//   Jo�o Matos (triton)
//
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
	public class MSBuildItemGroupCollection
	{
		public XmlDocument doc;
		protected Dictionary<XmlElement, MSBuildObject> elemCache = new Dictionary<XmlElement, MSBuildObject>();
		protected Dictionary<string, MSBuildItemGroup> bestGroups;

		public const string Schema = "http://schemas.microsoft.com/developer/msbuild/2003";
		protected static XmlNamespaceManager manager;

		bool endsWithEmptyLine;
		string newLine = Environment.NewLine;

		internal static XmlNamespaceManager XmlNamespaceManager
		{
			get
			{
				if (manager == null)
				{
					manager = new XmlNamespaceManager(new NameTable());
					manager.AddNamespace("tns", Schema);
				}
				return manager;
			}
		}

		public MSBuildItemGroupCollection()
		{
			doc = new XmlDocument();
			doc.PreserveWhitespace = false;
			doc.AppendChild(doc.CreateElement(null, "Project", Schema));
		}

		public void Load(string file)
		{
			string xml = File.ReadAllText(file);
			LoadXml(xml);
		}

		public void LoadXml(string xml)
		{
			doc = new XmlDocument();
			doc.PreserveWhitespace = false;
			doc.LoadXml(xml);
			newLine = CountNewLines("\r\n", xml) > (CountNewLines("\n", xml) / 2) ? "\r\n" : "\n";
			if (xml.EndsWith(newLine))
				endsWithEmptyLine = true;
		}

		class Utf8Writer : StringWriter
		{
			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}
		}

		public string Save()
		{
			// StringWriter.Encoding always returns UTF16. We need it to return UTF8, so the
			// XmlDocument will write the UTF8 header.
			Utf8Writer sw = new Utf8Writer();
			sw.NewLine = newLine;
			doc.Save(sw);
			string txt = sw.ToString();
			if (endsWithEmptyLine && !txt.EndsWith(newLine))
				txt += newLine;
			return txt;
		}

		public int CountNewLines(string nl, string text)
		{
			int i = -1;
			int c = -1;
			do
			{
				c++;
				i++;
				i = text.IndexOf(nl, i);
			}
			while (i != -1);
			return c;
		}

		public string ToolsVersion
		{
			get { return doc.DocumentElement.GetAttribute("ToolsVersion"); }
			set
			{
				if (!string.IsNullOrEmpty(value))
					doc.DocumentElement.SetAttribute("ToolsVersion", value);
				else
					doc.DocumentElement.RemoveAttribute("ToolsVersion");
			}
		}

		public IEnumerable<MSBuildItemGroup> ItemGroups
		{
			get
			{
				foreach (XmlElement elem in doc.DocumentElement.SelectNodes("tns:ItemGroup", XmlNamespaceManager))
					yield return GetItemGroup(elem);
			}
		}

		public MSBuildItemGroup AddNewItemGroup()
		{
			XmlElement elem = doc.CreateElement(null, "ItemGroup", MSBuildProject.Schema);
			doc.DocumentElement.AppendChild(elem);
			return GetItemGroup(elem);
		}

		public MSBuildItem AddNewItem(string name, string include)
		{
			MSBuildItemGroup grp = FindBestGroupForItem(name);
			return grp.AddNewItem(name, include);
		}

		protected MSBuildItemGroup FindBestGroupForItem(string itemName)
		{
			MSBuildItemGroup group;

			if (bestGroups == null)
				bestGroups = new Dictionary<string, MSBuildItemGroup>();
			else
			{
				if (bestGroups.TryGetValue(itemName, out group))
					return group;
			}

			foreach (MSBuildItemGroup grp in ItemGroups)
			{
				foreach (MSBuildItem it in grp.Items)
				{
					if (it.Name == itemName)
					{
						bestGroups[itemName] = grp;
						return grp;
					}
				}
			}
			group = AddNewItemGroup();
			bestGroups[itemName] = group;
			return group;
		}

		public void RemoveItem(MSBuildItem item)
		{
			elemCache.Remove(item.Element);
			XmlElement parent = (XmlElement)item.Element.ParentNode;
			item.Element.ParentNode.RemoveChild(item.Element);
			if (parent.ChildNodes.Count == 0)
			{
				elemCache.Remove(parent);
				parent.ParentNode.RemoveChild(parent);
				bestGroups = null;
			}
		}

		internal MSBuildItem GetItem(XmlElement elem)
		{
			MSBuildObject ob;
			if (elemCache.TryGetValue(elem, out ob))
				return (MSBuildItem)ob;
			MSBuildItem it = new MSBuildItem(elem);
			elemCache[elem] = it;
			return it;
		}

		protected MSBuildPropertyGroup GetGroup(XmlElement elem)
		{
			MSBuildObject ob;
			if (elemCache.TryGetValue(elem, out ob))
				return (MSBuildPropertyGroup)ob;
			MSBuildPropertyGroup it = new MSBuildPropertyGroup(this, elem);
			elemCache[elem] = it;
			return it;
		}

		protected MSBuildItemGroup GetItemGroup(XmlElement elem)
		{
			MSBuildObject ob;
			if (elemCache.TryGetValue(elem, out ob))
				return (MSBuildItemGroup)ob;
			MSBuildItemGroup it = new MSBuildItemGroup(this, elem);
			elemCache[elem] = it;
			return it;
		}

		public void RemoveGroup(MSBuildPropertyGroup grp)
		{
			elemCache.Remove(grp.Element);
			grp.Element.ParentNode.RemoveChild(grp.Element);
		}
	}

	public class MSBuildObject
	{
		protected XmlElement elem;

		public MSBuildObject(XmlElement elem)
		{
			this.elem = elem;
		}

		public XmlElement Element
		{
			get { return elem; }
		}

		protected XmlElement AddChildElement(string name)
		{
			XmlElement e = elem.OwnerDocument.CreateElement(null, name, MSBuildProject.Schema);
			elem.AppendChild(e);
			return e;
		}

		public string Condition
		{
			get
			{
				return Element.GetAttribute("Condition");
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					Element.RemoveAttribute("Condition");
				else
					Element.SetAttribute("Condition", value);
			}
		}
	}

	public class MSBuildProperty : MSBuildObject
	{
		public MSBuildProperty(XmlElement elem)
			: base(elem)
		{
		}

		public string Name
		{
			get { return Element.Name; }
		}

		public string Value
		{
			get
			{
				return Element.InnerXml;
			}
			set
			{
				Element.InnerXml = value;
			}
		}
	}

	public interface MSBuildPropertySet
	{
		MSBuildProperty GetProperty(string name);
		IEnumerable<MSBuildProperty> Properties { get; }
		void SetPropertyValue(string name, string value);
		string GetPropertyValue(string name);
		bool RemoveProperty(string name);
		void RemoveAllProperties();
		void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude);
	}

	public class MSBuildPropertyGroupMerged : MSBuildPropertySet
	{
		List<MSBuildPropertyGroup> groups = new List<MSBuildPropertyGroup>();

		public void Add(MSBuildPropertyGroup g)
		{
			groups.Add(g);
		}

		public int GroupCount
		{
			get { return groups.Count; }
		}

		public MSBuildProperty GetProperty(string name)
		{
			// Find property in reverse order, since the last set
			// value is the good one
			for (int n = groups.Count - 1; n >= 0; n--)
			{
				var g = groups[n];
				MSBuildProperty p = g.GetProperty(name);
				if (p != null)
					return p;
			}
			return null;
		}

		public void SetPropertyValue(string name, string value)
		{
			MSBuildProperty p = GetProperty(name);
			if (p != null)
				p.Value = value;
			else
				groups[0].SetPropertyValue(name, value);
		}

		public string GetPropertyValue(string name)
		{
			MSBuildProperty prop = GetProperty(name);
			return prop != null ? prop.Value : null;
		}

		public bool RemoveProperty(string name)
		{
			bool found = false;
			foreach (var g in groups)
			{
				if (g.RemoveProperty(name))
				{
					Prune(g);
					found = true;
				}
			}
			return found;
		}

		public void RemoveAllProperties()
		{
			foreach (var g in groups)
			{
				g.RemoveAllProperties();
				Prune(g);
			}
		}

		public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude)
		{
			foreach (var g in groups)
			{
				g.UnMerge(baseGrp, propertiesToExclude);
			}
		}

		public IEnumerable<MSBuildProperty> Properties
		{
			get
			{
				foreach (var g in groups)
				{
					foreach (var p in g.Properties)
						yield return p;
				}
			}
		}

		void Prune(MSBuildPropertyGroup g)
		{
			if (g != groups[0] && !g.Properties.Any())
			{
				// Remove this group since it's now empty
				g.Parent.RemoveGroup(g);
			}
		}
	}

	public class MSBuildPropertyGroup : MSBuildObject, MSBuildPropertySet
	{
		Dictionary<string, MSBuildProperty> properties = new Dictionary<string, MSBuildProperty>();
		MSBuildItemGroupCollection parent;

		public MSBuildPropertyGroup(MSBuildItemGroupCollection parent, XmlElement elem)
			: base(elem)
		{
			this.parent = parent;
		}

		public MSBuildItemGroupCollection Parent
		{
			get
			{
				return this.parent;
			}
		}

		public MSBuildProperty GetProperty(string name)
		{
			MSBuildProperty prop;
			if (properties.TryGetValue(name, out prop))
				return prop;
			XmlElement propElem = Element[name, MSBuildProject.Schema];
			if (propElem != null)
			{
				prop = new MSBuildProperty(propElem);
				properties[name] = prop;
				return prop;
			}
			else
				return null;
		}

		public IEnumerable<MSBuildProperty> Properties
		{
			get
			{
				foreach (XmlNode node in Element.ChildNodes)
				{
					XmlElement pelem = node as XmlElement;
					if (pelem == null)
						continue;
					MSBuildProperty prop;
					if (properties.TryGetValue(pelem.Name, out prop))
						yield return prop;
					else
					{
						prop = new MSBuildProperty(pelem);
						properties[pelem.Name] = prop;
						yield return prop;
					}
				}
			}
		}

		public void SetPropertyValue(string name, string value)
		{
			MSBuildProperty prop = GetProperty(name);
			if (prop == null)
			{
				XmlElement pelem = AddChildElement(name);
				prop = new MSBuildProperty(pelem);
				properties[name] = prop;
			}
			prop.Value = value;
		}

		public string GetPropertyValue(string name)
		{
			MSBuildProperty prop = GetProperty(name);
			if (prop == null)
				return null;
			else
				return prop.Value;
		}

		public bool RemoveProperty(string name)
		{
			MSBuildProperty prop = GetProperty(name);
			if (prop != null)
			{
				properties.Remove(name);
				Element.RemoveChild(prop.Element);
				return true;
			}
			return false;
		}

		public void RemoveAllProperties()
		{
			List<XmlNode> toDelete = new List<XmlNode>();
			foreach (XmlNode node in Element.ChildNodes)
			{
				if (node is XmlElement)
					toDelete.Add(node);
			}
			foreach (XmlNode node in toDelete)
				Element.RemoveChild(node);
			properties.Clear();
		}

		public void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propsToExclude)
		{
			foreach (MSBuildProperty prop in baseGrp.Properties)
			{
				if (propsToExclude != null && propsToExclude.Contains(prop.Name))
					continue;
				MSBuildProperty thisProp = GetProperty(prop.Name);
				if (thisProp != null && prop.Value.Equals(thisProp.Value, StringComparison.CurrentCultureIgnoreCase))
					RemoveProperty(prop.Name);
			}
		}

		public override string ToString()
		{
			string s = "[MSBuildPropertyGroup:";
			foreach (MSBuildProperty prop in Properties)
				s += " " + prop.Name + "=" + prop.Value;
			return s + "]";
		}
	}

	public class MSBuildItem : MSBuildObject
	{
		public MSBuildItem(XmlElement elem)
			: base(elem)
		{
		}

		public string Include
		{
			get { return Element.GetAttribute("Include"); }
			set { Element.SetAttribute("Include", value); }
		}

		public string Name
		{
			get { return Element.Name; }
		}

		public bool HasMetadata(string name)
		{
			return Element[name, MSBuildProject.Schema] != null;
		}

		public void SetMetadata(string name, string value)
		{
			SetMetadata(name, value, true);
		}

		public void SetMetadata(string name, string value, bool isLiteral)
		{
			XmlElement elem = Element[name, MSBuildProject.Schema];
			if (elem == null)
			{
				elem = AddChildElement(name);
				Element.AppendChild(elem);
			}
			elem.InnerXml = value;
		}

		public void UnsetMetadata(string name)
		{
			XmlElement elem = Element[name, MSBuildProject.Schema];
			if (elem != null)
				Element.RemoveChild(elem);
		}

		public string GetMetadata(string name)
		{
			XmlElement elem = Element[name, MSBuildProject.Schema];
			if (elem != null)
				return elem.InnerXml;
			else
				return null;
		}

		public bool GetMetadataIsFalse(string name)
		{
			return String.Compare(GetMetadata(name), "False", StringComparison.OrdinalIgnoreCase) == 0;
		}

		public void MergeFrom(MSBuildItem other)
		{
			foreach (XmlNode node in Element.ChildNodes)
			{
				if (node is XmlElement)
					SetMetadata(node.LocalName, node.InnerXml);
			}
		}
	}

	public class MSBuildItemGroup : MSBuildObject
	{
		MSBuildItemGroupCollection parent;

		internal MSBuildItemGroup(MSBuildItemGroupCollection parent, XmlElement elem)
			: base(elem)
		{
			this.parent = parent;
			this.elem = elem;
		}

		public MSBuildItem AddNewItem(string name, string include)
		{
			XmlElement elem = AddChildElement(name);
			MSBuildItem it = parent.GetItem(elem);
			it.Include = include;
			return it;
		}

		public IEnumerable<MSBuildItem> Items
		{
			get
			{
				foreach (XmlNode node in Element.ChildNodes)
				{
					XmlElement elem = node as XmlElement;
					if (elem != null)
						yield return parent.GetItem(elem);
				}
			}
		}
	}
}