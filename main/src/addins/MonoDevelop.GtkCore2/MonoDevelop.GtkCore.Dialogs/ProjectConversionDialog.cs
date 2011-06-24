using System;
using System.Collections.Generic;
using Gtk;

using MonoDevelop.Projects;

namespace MonoDevelop.GtkCore.Dialogs
{
	public partial class ProjectConversionDialog : Gtk.Dialog
	{		
		List<GtkDesignInfo> infos;
		
		public Action<GtkDesignInfo> ConversionMethod { get; set;}
		
		public string GuiFolderName { get; private set; }
		
		public bool MakeBackup { get; private set; }
		
		
		public ProjectConversionDialog (IntPtr raw)
			: base (raw)
		{
		}
		
		public ProjectConversionDialog (List<GtkDesignInfo> infos, 
			string guiFolderName, string solutionName)
		{
			this.Build ();
			
			this.infos = infos;
			
			entryFolder.Text = guiFolderName;
			entryFolder.Position = -1;
			Title = solutionName;
			
			buttonConvert.Clicked += HandleButtonConvertClicked;
		}

		void HandleButtonConvertClicked (object sender, EventArgs e)
		{
			GuiFolderName = entryFolder.Text;
			MakeBackup = checkBackup.Active;
			
			notebook.CurrentPage = 1;
			labelProgress.Visible = true;
			progressbar.Visible = true;
			progressbar.Adjustment.Lower = 0;
			progressbar.Adjustment.Upper = infos.Count;
			progressbar.Adjustment.StepIncrement = 1;
			
			foreach (GtkDesignInfo info in infos) {
				Project project = info.GuiBuilderProject.Project;
				var adjustment = progressbar.Adjustment;
				labelProgress.Text = string.Format (@"Converting {0} {1:N0}\{2}",
					project.Name, adjustment.Value + 1, infos.Count);
				ConversionMethod (info);
				adjustment.Value += adjustment.StepIncrement;
				adjustment.ChangeValue ();
				
			}
			Respond (ResponseType.Yes);
		}
	}
}

