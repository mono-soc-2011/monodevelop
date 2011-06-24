//
// GtkCoreService.cs
//
// Author:
//   Lluis Sanchez Gual, Krzysztof Marecki
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Krzysztof Marecki
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
using System.Collections.Generic;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.GtkCore.Dialogs;

namespace MonoDevelop.GtkCore
{
	class StartupCommand: CommandHandler
	{
		protected override void Run()
		{
			ReferenceManager.Initialize ();
		}
	}
	
	class InitCompleteCommand : CommandHandler
	{
		protected override void Run ()
		{
			IdeApp.Workspace.SolutionLoaded += HandleWorkspaceSolutionLoaded;
		}
		
		void ConvertSolution (Solution solution)
		{
			var infos = new List<GtkDesignInfo> ();
			foreach (Project project in solution.GetAllProjects ()) {
				GtkDesignInfo info = GtkDesignInfo.FromProject (project);
				if (info.NeedsConversion) {
					infos.Add (info);
				}
			}
			
			if (infos.Count == 0) {
				return;
			}
			
			var dialog = new ProjectConversionDialog(infos, "Designer", solution.Name);
			dialog.ConversionMethod =  delegate(MonoDevelop.GtkCore.GtkDesignInfo info) {
				Project project = info.GuiBuilderProject.Project;
				info.GuiBuilderProject.Convert (dialog.GuiFolderName, dialog.MakeBackup);
				IdeApp.ProjectOperations.Save (project);
			};
			try {
				MessageService.RunCustomDialog (dialog);
			}
			finally {
				dialog.Destroy ();
			}
		}
		
		void HandleWorkspaceSolutionLoaded (object sender, SolutionEventArgs e)
		{
			ConvertSolution (e.Solution);
		}
	}
}
