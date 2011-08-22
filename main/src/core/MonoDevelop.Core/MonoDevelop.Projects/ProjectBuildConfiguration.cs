// 
// ProjectBuildConfiguration.cs
//  
// Author:
//       João Matos (triton)
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



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xml;
using MonoDevelop.Core;
using System.ComponentModel;
using MonoDevelop.Projects;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.Projects
{
	/// <summary>
	/// This class represent a build configuration information in an Project object.
	/// </summary>
	[DataItem(Name = "ProjectConfiguration", FallbackType = typeof(UnknownProjectBuildConfiguration))]
	public class ProjectBuildConfiguration : ProjectItem, ICloneable
	{
		public ProjectBuildConfiguration()
		{
		}

		internal void SetOwnerProject(Project project)
		{
			OwnerProject = project;
		}

		public Project OwnerProject
		{
			get;
			set;
		}

		//[ItemProperty]
		public string Include
		{
			get;
			set;
		}

		//[ItemProperty]
		public string Configuration
		{
			get;
			set;
		}

		//[ItemProperty]
		public string Platform
		{
			get;
			set;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public class UnknownProjectBuildConfiguration : ProjectBuildConfiguration
		{
		}
	}
}
