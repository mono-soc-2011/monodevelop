//
// CppCompilerConfiguration.cs
//
// Author:
//   João Matos (triton)
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Core.Assemblies;
using MonoDevelop.Core.StringParsing;
using System.Collections.Generic;

namespace MonoDevelop.Projects
{
	#region C++ Properties Enums

	public enum WarningLevel
	{
		Level1,
		Level2,
		Level3,
		Level4,
	}

	public enum OptimizationLevel
	{
		Disabled,
		MinimizeSize,
		MaximizeSize,
		FullOptimization,
	}

	public enum OptimizationPreference
	{
		Size,
		Speed,
	}

	public enum RuntimeLibrary
	{
		MultiThreaded,
		MultiThreadedDebug,
		MultiThreadedDLL,
		MultiThreadedDebugDLL,
	}

	public enum FloatingPointModel
	{
		Precise,
		Fast,
		Strict,
	}

	public enum PrecompiledHeaderMode
	{
		None,
		Create,
		Use,
	}

	public enum CallingConvention
	{
		Cdecl,
		Fastcall,
		Stdcall,
	}

	#endregion

	public class CppCompilerConfiguration : SolutionItemConfiguration
	{
		#region C/C++ Properties

		#region General

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty("AdditionalIncludeDirectories")]
		public virtual string[] AdditionalIncludeDirectories
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty("WarningLevel")]
		public virtual WarningLevel WarningLevel
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty("TreatWarningsAsErrors")]
		public virtual bool TreatWarningsAsErrors
		{
			get;
			set;
		}

		#endregion

		#region Optimization

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual OptimizationLevel OptimizationLevel
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool EnableIntrinsicFunctions
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual OptimizationPreference FavorSizeOrSpeed
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool WholeProgramOptimization
		{
			get;
			set;
		}

		#endregion

		#region Preprocessor

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] PreprocessorDefinitions
		{
			get;
			set;
		}

		#endregion

		#region Code Generation

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual RuntimeLibrary RuntimeLibrary
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual FloatingPointModel RuntimeLibrary
		{
			get;
			set;
		}

		#endregion

		#region Language

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool EnableRuntimeTypeInformation
		{
			get;
			set;
		}

		#endregion

		#region Precompiled Headers

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual PrecompiledHeaderMode PrecompiledHeader
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual FilePath PrecompiledHeaderFile
		{
			get;
			set;
		}

		#endregion

		#region Output Files

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual FilePath ProgramDatabaseFileName
		{
			get;
			set;
		}

		#endregion

		#region Advanced

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual CallingConvention CallingConvention
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] DisableSpecificWarnings
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] TreatSpecificWarningsAsErrors
		{
			get;
			set;
		}

		#endregion

		#region Command Line

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string AdditionalOptions
		{
			get;
			set;
		}

		#endregion

		#endregion
	}
}