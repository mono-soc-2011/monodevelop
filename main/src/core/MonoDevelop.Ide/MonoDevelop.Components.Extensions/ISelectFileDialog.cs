// 
// ISelectFileDialog.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
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
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Mono.Addins;

namespace MonoDevelop.Components.Extensions
{
	/// <summary>
	/// This interface can be implemented to provide a custom implementation
	/// for the SelectFileDialog dialog.
	/// </summary>
	public interface ISelectFileDialogHandler : IDialogHandler<SelectFileDialogData>
	{
	}
	
	/// <summary>
	/// Data for the ISelectFileDialogHandler implementation
	/// </summary>
	public class SelectFileDialogData: PlatformDialogData
	{
		List<SelectFileDialogFilter> filters = new List<SelectFileDialogFilter> ();
		public Gtk.FileChooserAction Action { get; set; }
		public IList<SelectFileDialogFilter> Filters { get { return filters; } }
		public FilePath CurrentFolder { get; set; }
		public bool SelectMultiple { get; set; }
		public FilePath[] SelectedFiles { get; set; }
		public string InitialFileName { get; set; }
		public SelectFileDialogFilter DefaultFilter { get; set; }  
	}	
			
	/// <summary>
	/// Filter option to be displayed in file selector dialogs.
	/// </summary>
	public class SelectFileDialogFilter
	{
		public SelectFileDialogFilter (string name, params string[] patterns)
		{
			this.Name = name;
			this.Patterns = patterns;
		}
		
		public SelectFileDialogFilter (string name, IList<string> patterns, IList<string> mimetypes)
		{
			this.Name = name;
			this.Patterns = patterns;
			this.MimeTypes = mimetypes;
		}
		
		/// <summary>Label for the filter</summary>
		public string Name { get; private set; }
		
		/// <summary>Filename glob patterns permitted by this filter</summary>
		public IList<string> Patterns { get; private set; }
		
		/// <summary>MIME types permitted by this filter</summary>
		public IList<string> MimeTypes { get; private set; }
		
		public static SelectFileDialogFilter AllFiles {
			get {
				return new SelectFileDialogFilter (GettextCatalog.GetString ("All Files"), "*");
			}
		}
	}
	
	/// <summary>
	/// Generic class to be used to implement file selectors.
	/// The T type argument is the type of the handler.
	/// The U type is the type of the data parameter (must subclass SelectFileDialogData)
	/// </summary>
	public abstract class SelectFileDialog<T>: PlatformDialog<T> where T:SelectFileDialogData, new()
	{
		/// <summary>
		/// Action to perform with the file dialog.
		/// </summary>
		public Gtk.FileChooserAction Action {
			get { return data.Action; }
			set { data.Action = value; }
		}
		
		/// <summary>
		/// Folder to show by default.
		/// </summary>
		public FilePath CurrentFolder {
			get { return data.CurrentFolder; }
			set { data.CurrentFolder = value; }
		}
		
		/// <summary>
		/// Set to True to allow multiple selection.
		/// </summary>
		public bool SelectMultiple {
			get { return data.SelectMultiple; }
			set { data.SelectMultiple = value; }
		}
		
		/// <summary>
		/// List of selected files (or folders).
		/// </summary>
		public FilePath[] SelectedFiles {
			get { return data.SelectedFiles; }
		}
		
		/// <summary>
		/// Selected file (or folder) when using single selection mode.
		/// </summary>
		public FilePath SelectedFile {
			get { return data.SelectedFiles.Length > 0 ? data.SelectedFiles [0] : null; }
		}
		
		/// <summary>
		/// File name to show by default.
		/// </summary>
		public string InitialFileName {
			get { return data.InitialFileName; }
			set { data.InitialFileName = value; }
		}
		
		/// <summary>
		/// File filters that allow the user to choose the kinds of files the dialog displays.
		/// </summary>
		public IList<SelectFileDialogFilter> Filters {
			get { return data.Filters; }
		}
		
		/// <summary>
		/// The default file filter. If there is only one, the user will not have a choice of file types.
		/// </summary>
		public SelectFileDialogFilter DefaultFilter {
			get { return data.DefaultFilter; }
			set { data.DefaultFilter = value; }
		}
		
		#region File filter utilities
				
		public SelectFileDialogFilter AddFilter (string label, params string[] patterns)
		{
			return AddFilter (new SelectFileDialogFilter (label, patterns));
		}
		
		public SelectFileDialogFilter AddFilter (SelectFileDialogFilter filter)
		{
			if (useDefaultFilters)
				throw new InvalidOperationException ("Cannot mix default filters and custom filters");
			data.Filters.Add (filter);
			return filter;
		}
		
		public SelectFileDialogFilter AddAllFilesFilter ()
		{
			return AddFilter (SelectFileDialogFilter.AllFiles);
		}
		
		public bool HasFilters {
			get {
				return data.Filters != null && data.Filters.Count > 0;
			}
		}
		
		/// <summary>
		/// Adds the default file filters registered by MD core and addins. Includes the All Files filter.
		/// </summary>
		public void AddDefaultFileFilters ()
		{
			if (HasFilters)
				throw new InvalidOperationException ("Cannot mix default filters and custom filters");
			if (useDefaultFilters)
				throw new InvalidOperationException ("Already called");
			
			useDefaultFilters = true;
			
			foreach (var f in GetDefaultFilters ())
				data.Filters.Add (f);
			data.Filters.Add (SelectFileDialogFilter.AllFiles);
			
			LoadDefaultFilter ();
		}
		
		bool useDefaultFilters;
		
		///<summary>Loads last default filter from MD prefs</summary>
		void LoadDefaultFilter ()
		{
			// Load last used filter pattern
			var lastPattern = PropertyService.Get ("Monodevelop.FileSelector.LastPattern", "*");
			foreach (var filter in Filters) {
				if (filter.Patterns != null && filter.Patterns.Contains (lastPattern)) {
					DefaultFilter = filter;
					break;
				}
			}
		}
		
		///<summary>Saves last default filter to MD prefs, if necessary</summary>
		protected void SaveDefaultFilter ()
		{
			if (!useDefaultFilters)
				return;
			
			// Save active filter
			//it may be null if e.g. SetSelectedFile was used
			if (DefaultFilter != null && DefaultFilter.Patterns != null && DefaultFilter.Patterns.Count > 0)
				PropertyService.Set ("Monodevelop.FileSelector.LastPattern", DefaultFilter.Patterns[0]);
		}
		
		static IEnumerable<SelectFileDialogFilter> GetDefaultFilters ()
		{
			foreach (var f in ParseFilters (AddinManager.GetExtensionObjects ("/MonoDevelop/Ide/ProjectFileFilters")))
				yield return f;
			foreach (var f in ParseFilters (AddinManager.GetExtensionObjects ("/MonoDevelop/Ide/FileFilters")))
				yield return f;
		}
		
		static IEnumerable<SelectFileDialogFilter> ParseFilters (System.Collections.IEnumerable filterStrings)
		{
			if (filterStrings == null)
				yield break;
			foreach (string filterStr in filterStrings) {
				var parts = filterStr.Split ('|');
				var f = new SelectFileDialogFilter (parts[0], parts[1].Split (';'));
				yield return f;
			}
		}

		void SetGtkFileFilters (FileSelector fdiag)
		{
			var list = new List<Gtk.FileFilter> ();
			Gtk.FileFilter defaultGtkFilter = null;
			
			foreach (var filter in data.Filters) {
				var gf = new Gtk.FileFilter ();
				if (!string.IsNullOrEmpty (filter.Name))
					gf.Name = filter.Name;
				if (filter.Patterns != null)
					foreach (var pattern in filter.Patterns)
						gf.AddPattern (pattern);
				if (filter.MimeTypes != null)
					foreach (var mimetype in filter.MimeTypes)
						gf.AddMimeType (mimetype);
				list.Add (gf);
				if (filter == DefaultFilter)
					defaultGtkFilter = gf;
			}
			
			foreach (var filter in list)
				fdiag.AddFilter (filter);
			
			if (defaultGtkFilter != null)
				fdiag.Filter = defaultGtkFilter;
			
			fdiag.Destroyed += CaptureDefaultFilter;
		}

		[GLib.ConnectBefore]
		void CaptureDefaultFilter (object sender, EventArgs e)
		{
			FileSelector fsel = (FileSelector) sender;
			if (fsel.Filter == null)
				return;
			var name = fsel.Filter.Name;
			foreach (var filter in data.Filters) {
				if (filter.Name == name) {
					data.DefaultFilter = filter;
					return;
				}
			}
		}
		
		#endregion
		
		/// <summary>
		/// Utility method to populate a GTK FileSelector from the data properties.
		/// </summary>
		internal void SetDefaultProperties (FileSelector fdiag)
		{
			fdiag.Title = Title;
			fdiag.Action = Action;
			fdiag.LocalOnly = true;
			fdiag.SelectMultiple = SelectMultiple;
			fdiag.TransientFor = TransientFor;
			
			if (!CurrentFolder.IsNullOrEmpty)
				fdiag.SetCurrentFolder (CurrentFolder);
			if (!string.IsNullOrEmpty (InitialFileName))
				fdiag.CurrentName = InitialFileName;
			
			if (!CurrentFolder.IsNullOrEmpty && !string.IsNullOrEmpty (InitialFileName)) {
				var checkName = data.CurrentFolder.Combine (InitialFileName);
				if (System.IO.File.Exists (checkName))
					fdiag.SetFilename (checkName);
			}
			
			SetGtkFileFilters (fdiag);
		}
		
		internal void GetDefaultProperties (FileSelector fdiag)
		{
			data.SelectedFiles = fdiag.Filenames.ToFilePathArray ();
			var currentFilter = fdiag.Filter;
			if (currentFilter != null) {
				var name = fdiag.Filter.Name;
				var def = data.Filters.Where (f => f.Name == name).FirstOrDefault ();
				if (def != null)
					DefaultFilter = def;
			}
		}
		
		public override bool Run ()
		{
			if (!HasFilters)
				AddDefaultFileFilters ();
			
			bool success = base.Run ();
			
			if (success && useDefaultFilters)
				SaveDefaultFilter ();
			
			return success;
		}
		
		/// <summary>Runs the default implementation of the dialog.</summary>
		protected override bool RunDefault ()
		{
			var fdiag = new FileSelector ();
			SetDefaultProperties (fdiag);
			try {
				int result = MessageService.RunCustomDialog (fdiag, data.TransientFor ?? MessageService.RootWindow);
				GetDefaultProperties (fdiag);
				return result == (int) Gtk.ResponseType.Ok;
			} finally {
				fdiag.Destroy ();
			}
		}
	}
}
