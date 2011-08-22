//
// CppLanguageBinding.cs
//
// Authors:
//   Marcos David Marin Amador <MarcosMarin@gmail.com>
//
// Copyright (C) 2007 Marcos David Marín Amador
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
using System.IO;

using Mono.Addins;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.CodeGeneration;

namespace CBinding
{
	public class CppLanguageBinding : IDotNetLanguageBinding
	{
		public string Language {
			get { return "C++"; }
		}
		
		public string SingleLineCommentTag { get { return "//"; } }
		public string BlockCommentStartTag { get { return "/*"; } }
		public string BlockCommentEndTag { get { return "*/"; } }
		
		public bool IsSourceCodeFile (string fileName)
		{
			return fileName.EndsWith (".cpp", StringComparison.OrdinalIgnoreCase);
		}
		
		public IParser Parser {
			get { return null; }
		}
		
		public IRefactorer Refactorer {
			get { return null; }
		}
		
		public string GetFileName (string baseName)
		{
			return baseName + ".cpp";
		}

		public string ProjectStockIcon
		{
			get { return "md-cpp-file"; }
		}

		public ConfigurationParameters CreateCompilationParameters(System.Xml.XmlElement projectOptions)
		{
			LoggingService.LogDebug("NotImplemented");
			return null;
		}

		public ProjectParameters CreateProjectParameters(System.Xml.XmlElement projectOptions)
		{
			return new CProjectParameters();
		}

		public BuildResult Compile(ProjectItemCollection items, DotNetProjectConfiguration configuration, ConfigurationSelector configSelector, IProgressMonitor monitor)
		{
			LoggingService.LogDebug("NotImplemented");
			return null;
		}

		public ClrVersion[] GetSupportedClrVersions()
		{
			return new ClrVersion[] { 
				ClrVersion.Net_1_1, 
				ClrVersion.Net_2_0, 
				ClrVersion.Clr_2_1,
				ClrVersion.Net_4_0
			};
		}

		public System.CodeDom.Compiler.CodeDomProvider GetCodeDomProvider()
		{
			LoggingService.LogDebug("NotImplemented");
			return null;
		}
	}
}
