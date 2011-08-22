// 
// ProjectFilterItem.cs
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
using System.Collections.Generic;
using MonoDevelop.Core.Serialization;

namespace MonoDevelop.Projects
{
	// Filters behave like (virtual) project folders but can also have patterns of
	// files, so they can also filter the files shown based on some user criteria.
	
	public class ProjectFilterItem : ProjectItem
	{
		public List<ProjectItem> items;

		public ProjectFilterItem(string name)
		{
			Name = name;
			items = new List<ProjectItem>();
		}

		[ItemProperty("Include")]
		public string Name { get; set; }

		[ItemProperty]
		public string Filter { get; set; }

		[ItemProperty("UniqueIdentifier")]
		public string UniqueIdentifier { get; set; }
	}

	public class UnknownProjectFilterItem : ProjectFilterItem
	{
		public UnknownProjectFilterItem(string name) : base(name)
		{
		}
	}
}
