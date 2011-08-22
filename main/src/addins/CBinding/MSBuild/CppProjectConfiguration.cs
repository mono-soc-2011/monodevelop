//
// CppProjectConfiguration.cs
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
using MonoDevelop.Projects;

namespace CBinding
{
	#region General Properties Enums

	public enum PlatformToolsets
	{
		v100,
		v90,
		MinGW,
		GCC,
		Clang,
	}

	public enum ConfigurationType
	{
		Application,
		DynamicLibrary,
		StaticLibrary,
	}

	public enum CharacterSets
	{
		Unicode,
		MultiByte,
	}

	public enum ProgramOptimizationType
	{
		None,
		LinkTimeCodeGeneration,
		ProfileGuidedOptimization,
	}

	#endregion

	#region Librarian Properties Enums

	public enum MachineModel
	{
		X86,
		X64,
		MIPS,
		ARM,
	}

	public enum LinkingSubsystem
	{
		Console,
		Native,
		Windows,
	}

	#endregion

	public class CppProjectConfiguration : ProjectConfiguration
	{
		public CppProjectConfiguration()
		{
		}

		public CppProjectConfiguration(string name)
			: base(name)
		{
		}

		#region General Properties

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ProjectPathItemProperty(DefaultValue = "$(SolutionDir)lib")]
		public virtual FilePath OutputDirectory
		{
			get;
			set;
		}

		[ProjectPathItemProperty("IntermediatePath", DefaultValue="$(SolutionDir)obj")]
		public virtual FilePath IntermediateDirectory
		{
			get;
			set;
		}

		[ProjectPathItemProperty("TargetName", DefaultValue="$(ProjectName)")]
		public virtual FilePath TargetName
		{
			get;
			set;
		}

		[ItemProperty(DefaultValue=".lib")]
		public virtual string TargetExtension
		{
			get;
			set;
		}

		[ItemProperty(DefaultValue = PlatformToolsets.v100)]
		public virtual PlatformToolsets PlatformToolset
		{
			get;
			set;
		}

		[ItemProperty(DefaultValue = ProgramOptimizationType.None)]
		public virtual ProgramOptimizationType WholeProgramOptimization
		{
			get;
			set;
		}

		[ItemProperty]
		public virtual ConfigurationType ConfigurationType
		{
			get
			{
#if CPP_PROJECT
				CppProject prj = ParentItem as CppProject;
				if (prj != null)
					return prj.CompileTarget;
				else
#endif
				return ConfigurationType.StaticLibrary;
			}
		}

		#endregion

		#region Debugging Properties

		[ItemProperty(DefaultValue = "$(TargetPath)")]
		public virtual string Command
		{
			get;
			set;
		}

		[ItemProperty]
		public virtual string WorkingDirectory
		{
			get;
			set;
		}

		#endregion

		#region Librarian Properties

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ProjectPathItemProperty(DefaultValue = "$(OutDir)$(TargetName)$(TargetExt)")]
		public virtual FilePath OutputFile
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] AdditionalDependencies
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] AdditionalLibraryDirectories
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool IgnoreAllDefaultLibraries
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] AdditionalSpecificDefaultLibraries
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] ExportNamedFunctions
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual string[] ForceSymbolReferences
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool TreatLibWarningsAsErrors
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual LinkingSubsystem SubSystem
		{
			get;
			set;
		}

		[MonoDevelop.Projects.Formats.MSBuild.MergeToProject]
		[ItemProperty]
		public virtual bool Verbose
		{
			get;
			set;
		}

		#endregion

		CppCompilerConfiguration compilationParameters;
		public CppCompilerConfiguration CompilationParameters
		{
			get { return compilationParameters; }
			set
			{
				compilationParameters = value;
				compilationParameters.ParentProject = this;
			}
		}

		public new CppProject ParentItem
		{
			get { return (CppProject)base.ParentItem; }
		}
	}

	public class UnknownCompilationParameters : ConfigurationParameters, IExtendedDataItem
	{
		Hashtable table = new Hashtable();

		public IDictionary ExtendedProperties
		{
			get { return table; }
		}
	}

	public class UnknownProjectParameters : ProjectParameters, IExtendedDataItem
	{
		Hashtable table = new Hashtable();

		public IDictionary ExtendedProperties
		{
			get { return table; }
		}
	}
}
