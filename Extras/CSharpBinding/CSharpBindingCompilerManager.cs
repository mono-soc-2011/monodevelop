// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike KrÃ¼ger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using System.Threading;

using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Projects;

namespace CSharpBinding
{
	/// <summary>
	/// This class controls the compilation of C Sharp files and C Sharp projects
	/// </summary>
	public class CSharpBindingCompilerManager
	{	
		public bool CanCompile(string fileName)
		{
			return Path.GetExtension(fileName).ToUpper() == ".CS";
		}

		public ICompilerResult Compile (ProjectFileCollection projectFiles, ProjectReferenceCollection references, DotNetProjectConfiguration configuration, IProgressMonitor monitor)
		{
			CSharpCompilerParameters compilerparameters = (CSharpCompilerParameters) configuration.CompilationParameters;
			if (compilerparameters == null) compilerparameters = new CSharpCompilerParameters ();
			
			string exe = configuration.CompiledOutputName;
			string responseFileName = Path.GetTempFileName();
			StringWriter writer = new StringWriter ();
			bool hasWin32Res = false;
			ArrayList gacRoots = new ArrayList ();

			writer.WriteLine("\"/out:" + exe + '"');
			
			ArrayList pkg_references = new ArrayList ();
			
			if (references != null) {
				foreach (ProjectReference lib in references) {
					foreach (string fileName in lib.GetReferencedFileNames ()) {
						switch (lib.ReferenceType) {
						case ReferenceType.Gac:
							SystemPackage pkg = Runtime.SystemAssemblyService.GetPackageFromFullName (lib.Reference);
							if (pkg == null) {
								string msg = String.Format (GettextCatalog.GetString ("{0} could not be found or is invalid."), lib.Reference);
								monitor.ReportWarning (msg);
								continue;
							}
							if (pkg.IsCorePackage) {
								writer.WriteLine ("\"/r:" + Path.GetFileName (fileName) + "\"");
							} else if (pkg.IsInternalPackage) {
								writer.WriteLine ("\"/r:" + fileName + "\"");
							} else if (!pkg_references.Contains (pkg.Name)) {
								pkg_references.Add (pkg.Name);
								writer.WriteLine ("\"-pkg:" + pkg.Name + "\"");
							}
							if (pkg.GacRoot != null && !gacRoots.Contains (pkg.GacRoot))
								gacRoots.Add (pkg.GacRoot);
							break;
						default:
							writer.WriteLine ("\"/r:" + fileName + "\"");
							break;
						}
					}
				}
			}
			
			writer.WriteLine("/noconfig");
			writer.WriteLine("/nologo");
			writer.WriteLine("/codepage:utf8");
//			writer.WriteLine("/utf8output");
//			writer.WriteLine("/w:" + compilerparameters.WarningLevel);;
				
			if (configuration.SignAssembly) {
				if (File.Exists (configuration.AssemblyKeyFile))
					writer.WriteLine("/keyfile:" + configuration.AssemblyKeyFile);
			}
			
			if (configuration.DebugMode) {
				writer.WriteLine("/debug:+");
				writer.WriteLine("/debug:full");
				writer.WriteLine("/d:DEBUG");
			}
			
			// mcs default is + but others might not be
			if (compilerparameters.Optimize)
				writer.WriteLine("/optimize+");
			else
				writer.WriteLine("/optimize-");

			if (compilerparameters.Win32Resource != null && compilerparameters.Win32Resource.Length > 0 && File.Exists (compilerparameters.Win32Resource)) {
				writer.WriteLine("\"/win32res:" + compilerparameters.Win32Resource + "\"");
				hasWin32Res = true;
			}
		
			if (compilerparameters.Win32Icon != null && compilerparameters.Win32Icon.Length > 0 && File.Exists (compilerparameters.Win32Icon)) {
				if (hasWin32Res)
					monitor.ReportWarning ("Both Win32 icon and Win32 resource cannot be specified. Ignoring the icon.");
				else
					writer.WriteLine("\"/win32icon:" + compilerparameters.Win32Icon + "\"");
			}
			
			if (compilerparameters.CodePage != 0) {
				writer.WriteLine ("/codepage:" + compilerparameters.CodePage);
			}
			
			if (compilerparameters.UnsafeCode) {
				writer.WriteLine("-unsafe");
			}
			
			if (compilerparameters.DefineSymbols.Length > 0) {
				writer.WriteLine("/define:" + '"' + compilerparameters.DefineSymbols + '"');
			}
			
			if (compilerparameters.MainClass != null && compilerparameters.MainClass.Length > 0) {
				writer.WriteLine("/main:" + compilerparameters.MainClass);
			}
			
			switch (configuration.CompileTarget) {
				case CompileTarget.Exe:
					writer.WriteLine("/t:exe");
					break;
				case CompileTarget.WinExe:
					writer.WriteLine("/t:winexe");
					break;
				case CompileTarget.Library:
					writer.WriteLine("/t:library");
					break;
			}
			
			string resgen = "resgen";
			//ClrVersion.Default ?
			if (configuration.ClrVersion == ClrVersion.Net_2_0)
				resgen = "resgen2";

			foreach (ProjectFile finfo in projectFiles) {
				if (finfo.Subtype == Subtype.Directory)
					continue;

				switch (finfo.BuildAction) {
					case BuildAction.Compile:
						if (CanCompile (finfo.Name))
							writer.WriteLine('"' + finfo.Name + '"');
						break;
					case BuildAction.EmbedAsResource:
						//FIXME: Rationalize the use of monitor!
						string fname = finfo.Name;
						string resourceId;
						CompilerError ce = GetResourceId (finfo, ref fname, resgen, out resourceId, monitor);
						if (ce != null) {
							CompilerResults cr = new CompilerResults (new TempFileCollection ());
							cr.Errors.Add (ce);

							return new DefaultCompilerResult (cr, String.Empty);
						}
						writer.WriteLine(@"""/res:{0},{1}""", fname, resourceId);
						break;
					default:
						continue;
				}
			}
			if (compilerparameters.GenerateXmlDocumentation) {
				writer.WriteLine("\"/doc:" + Path.ChangeExtension(exe, ".xml") + '"');
			}

			writer.Close();

			string output = String.Empty;
			string error  = String.Empty;
			
			string mcs = configuration.ClrVersion == ClrVersion.Net_1_1 ? "mcs" : "gmcs";
			
			File.WriteAllText (responseFileName, writer.ToString ());
			
			string compilerName = compilerparameters.CsharpCompiler == CsharpCompiler.Csc ? GetCompilerName (configuration.ClrVersion) : mcs;
			string outstr = compilerName + " @" + responseFileName;
			TempFileCollection tf = new TempFileCollection();

			string workingDir = ".";
			if (projectFiles != null && projectFiles.Count > 0) {
				workingDir = projectFiles [0].Project.BaseDirectory;
				if (workingDir == null)
					// Dummy projects created for single files have no filename
					// and so no BaseDirectory.
					// This is a workaround for a bug in 
					// ProcessStartInfo.WorkingDirectory - not able to handle null
					workingDir = ".";
			}

			Runtime.LoggingService.Info (compilerName + " " + writer.ToString ());
			
			DoCompilation(outstr, tf, workingDir, gacRoots, ref output, ref error);

			ICompilerResult result = ParseOutput(tf, output, error);
			if (result.CompilerOutput.Trim () != "")
				monitor.Log.WriteLine (result.CompilerOutput);
			
			Runtime.FileService.DeleteFile (responseFileName);
			Runtime.FileService.DeleteFile (output);
			Runtime.FileService.DeleteFile (error);
			return result;
		}

		CompilerError GetResourceId (ProjectFile finfo, ref string fname, string resgen, out string resourceId, IProgressMonitor monitor)
		{
			resourceId = finfo.ResourceId;
			if (resourceId == null) {
				Console.WriteLine (GettextCatalog.GetString ("Error: Unable to build ResourceId for {0}.", fname));
				monitor.Log.WriteLine (GettextCatalog.GetString ("Error: Unable to build ResourceId for {0}.", fname));

				return new CompilerError (fname, 0, 0, String.Empty,
						GettextCatalog.GetString ("Unable to build ResourceId for {0}.", fname));
			}

			if (String.Compare (Path.GetExtension (fname), ".resx", true) != 0)
				return null;

			using (StringWriter sw = new StringWriter ()) {
				Console.WriteLine ("Compiling resources\n{0}$ {1} /compile {2}", Path.GetDirectoryName (fname), resgen, fname);
				monitor.Log.WriteLine (GettextCatalog.GetString (
					"Compiling resource {0} with {1}", fname, resgen));
				ProcessWrapper pw = null;
				try {
					pw = Runtime.ProcessService.StartProcess (
						resgen, String.Format ("/compile \"{0}\"", fname),
						Path.GetDirectoryName (fname),
						sw, sw, null);
				} catch (System.ComponentModel.Win32Exception ex) {
					Console.WriteLine (GettextCatalog.GetString (
						"Error while trying to invoke '{0}' to compile resource '{1}' :\n {2}", resgen, fname, ex.ToString ()));
					monitor.Log.WriteLine (GettextCatalog.GetString (
						"Error while trying to invoke '{0}' to compile resource '{1}' :\n {2}", resgen, fname, ex.Message));

					return new CompilerError (fname, 0, 0, String.Empty, ex.Message);
				}

				//FIXME: Handle exceptions
				pw.WaitForOutput ();

				if (pw.ExitCode == 0) {
					fname = Path.ChangeExtension (fname, ".resources");
				} else {
					string output = sw.ToString ();
					Console.WriteLine (GettextCatalog.GetString (
						"Unable to compile ({0}) {1} to .resources. \nReason: \n{2}\n",
						resgen, fname, output));
					monitor.Log.WriteLine (GettextCatalog.GetString (
						"Unable to compile ({0}) {1} to .resources. \nReason: \n{2}\n",
						resgen, fname, output));

					//Try to get the line/pos
					int line = 0;
					int pos = 0;
					Match match = RegexErrorLinePos.Match (output);
					if (match.Success && match.Groups.Count == 3) {
						try {
							line = int.Parse (match.Groups [1].Value);
						} catch (FormatException){
						}

						try {
							pos = int.Parse (match.Groups [2].Value);
						} catch (FormatException){
						}
					}

					return new CompilerError (fname, line, pos, String.Empty, output);
				}
			}

			return null;
		}

		string GetCompilerName (ClrVersion version)
		{
			string runtimeDir = Runtime.FileService.GetDirectoryNameWithSeparator(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
			// The following regex foo gets the index of the
			// last match of lib/lib32/lib64 and uses
			// the text before that as the 'prefix' in order
			// to find the right mcs to use.
			Regex regex = new Regex ("lib[32 64]?");
			MatchCollection matches = regex.Matches(runtimeDir);
			Match match = matches[matches.Count - 1];
			string mcs = version == ClrVersion.Net_1_1 ? "mcs" : "gmcs";
			string compilerName = runtimeDir.Substring(0, match.Index) + Path.Combine("bin", mcs);
			return compilerName;
		}
		
		ICompilerResult ParseOutput(TempFileCollection tf, string stdout, string stderr)
		{
			StringBuilder compilerOutput = new StringBuilder();
			
			CompilerResults cr = new CompilerResults(tf);
			
			// we have 2 formats for the error output the csc gives :
			//Regex normalError  = new Regex(@"(?<file>.*)\((?<line>\d+),(?<column>\d+)\):\s+(?<error>\w+)\s+(?<number>[\d\w]+):\s+(?<message>.*)", RegexOptions.Compiled);
			//Regex generalError = new Regex(@"(?<error>.+)\s+(?<number>[\d\w]+):\s+(?<message>.*)", RegexOptions.Compiled);
			
			foreach (string s in new string[] { stdout, stderr }) {
				StreamReader sr = File.OpenText (s);
				while (true) {
					string curLine = sr.ReadLine();
					compilerOutput.Append(curLine);
					compilerOutput.Append('\n');
					if (curLine == null) {
						break;
					}
					curLine = curLine.Trim();
					if (curLine.Length == 0) {
						continue;
					}
				
					CompilerError error = CreateErrorFromString (curLine);
					
					if (error != null)
						cr.Errors.Add (error);
				}
				sr.Close();
			}
			return new DefaultCompilerResult(cr, compilerOutput.ToString());
		}
		
		private void DoCompilation(string outstr, TempFileCollection tf, string working_dir, ArrayList gacRoots, ref string output, ref string error) {
			output = Path.GetTempFileName();
			error = Path.GetTempFileName();
			
			StreamWriter outwr = new StreamWriter(output);
			StreamWriter errwr = new StreamWriter(error);
			string[] tokens = outstr.Split(' ');
			
			outstr = outstr.Substring(tokens[0].Length+1);

			ProcessStartInfo pinfo = new ProcessStartInfo (tokens[0], "\"" + outstr + "\"");
			pinfo.WorkingDirectory = working_dir;
			
			if (gacRoots.Count > 0) {
				// Create the gac prefix string
				string gacPrefix = string.Join ("" + Path.PathSeparator, (string[])gacRoots.ToArray (typeof(string)));
				string oldGacVar = Environment.GetEnvironmentVariable ("MONO_GAC_PREFIX");
				if (!string.IsNullOrEmpty (oldGacVar))
					gacPrefix += Path.PathSeparator + oldGacVar;
				pinfo.EnvironmentVariables ["MONO_GAC_PREFIX"] = gacPrefix;
			}
			pinfo.UseShellExecute = false;
			pinfo.RedirectStandardOutput = true;
			pinfo.RedirectStandardError = true;
			
			ProcessWrapper pw = Runtime.ProcessService.StartProcess (pinfo, outwr, errwr, null);
			pw.WaitForOutput();
			outwr.Close();
			errwr.Close();
		}

		// Snatched from our codedom code :-).
		static Regex regexError = new Regex (@"^(\s*(?<file>.*)\((?<line>\d*)(,(?<column>\d*[\+]*))?\)(:|)\s+)*(?<level>\w+)\s*(?<number>.*):\s(?<message>.*)",
			RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		// Used for parsing "Line 123, position 5" errors from tools
		// like resgen, xamlg
		static Regex regexErrorLinePos;
		static Regex RegexErrorLinePos {
			get {
				if (regexErrorLinePos == null)
					regexErrorLinePos = new Regex (@"Line (\d*), position (\d*)");
				return regexErrorLinePos;
			}
		}
		
		private static CompilerError CreateErrorFromString(string error_string)
		{
			// When IncludeDebugInformation is true, prevents the debug symbols stats from braeking this.
			if (error_string.StartsWith ("WROTE SYMFILE") ||
			    error_string.StartsWith ("OffsetTable") ||
			    error_string.StartsWith ("Compilation succeeded") ||
			    error_string.StartsWith ("Compilation failed"))
				return null;

			CompilerError error = new CompilerError();

			Match match=regexError.Match(error_string);
			if (!match.Success) return null;
			if (String.Empty != match.Result("${file}"))
				error.FileName=match.Result("${file}");
			if (String.Empty != match.Result("${line}"))
				error.Line=Int32.Parse(match.Result("${line}"));
			if (String.Empty != match.Result("${column}")) {
				if (match.Result("${column}") == "255+")
					error.Column = -1;
				else
					error.Column=Int32.Parse(match.Result("${column}"));
			}
			if (match.Result("${level}")=="warning")
				error.IsWarning=true;
			error.ErrorNumber=match.Result("${number}");
			error.ErrorText=match.Result("${message}");
			return error;
		}
	}
}
