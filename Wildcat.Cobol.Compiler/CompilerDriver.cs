// Copyright (C) 2006-2007 Sandy Dunlop (sandy@sorn.net)
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using Wildcat.Cobol.Compiler.References;
using Wildcat.Cobol.Compiler.Parser;
using Wildcat.Cobol.Compiler.Structure;
using Wildcat.Cobol.Compiler.Analyzer;
using Wildcat.Cobol.Compiler.ILGenerator;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler
{
	public class CompilerDriver
	{
		string _filename;
		ArrayList _references;
		bool _verbose;
		
		static void Main(string[] args)
		{
			Assembly exeFile = System.Reflection.Assembly.GetExecutingAssembly();
            Console.WriteLine("Wildcat COBOL Compiler for .NET version " +
                exeFile.GetName().Version);
            Console.WriteLine("Copyright (C)2006-2023 Sandy Dunlop (http://dunlop.dev)");
            if (args.Length < 1)
            {
				Console.WriteLine("Usage: cobolc.exe [options] program.cbl");
				Console.WriteLine("Options include:");
				Console.WriteLine("  /ref assemblyname.dll  = reference an assembly");
				Console.WriteLine("  /pkg packagename       = use a package");
				Console.WriteLine("  /verbose               = run in verbose mode");
			}else{
				CompilerDriver compiler = new CompilerDriver(args);
				compiler.Compile();
			}
		}
		
		public CompilerDriver()
		{
		}
		
		public CompilerDriver(string[] args)
		{
			_verbose = false;
			_references = new ArrayList();
			int p = 0;
			
			while (p<args.Length)
			{
				string current = args[p];
				string next = "";
				if (p<args.Length-1){
					next = args[p+1];
				}
				int c;
				c = current.IndexOf(":");
				if (c>-1){
					next = current.Substring(c+1);
					current = current.Substring(0,c+1);
				}
				switch(current)
				{
					case "/r:":
					case "/ref:":
					case "/reference:":
						_references.Add(next);
						break;
					case "/pkg:":
					case "/package:":
						ProcessPackage(next);
						break;
					case "/v":
					case "/verbose":
						_verbose = true;
						break;
					default:
						_filename = current;
						break;
				}
				p++;
			}
		}
		
		private void ProcessPackage(string packageName)
		{
			string[] packagePaths = GetPackageAssemblyPaths(packageName);
			foreach (string packagePath in packagePaths)
			{
				_references.Add(packagePath);
			}
		}
		
		public void Compile()
		{
			string filename = _filename;
			string suffix = "";
			int dot = filename.IndexOf(".");
			try{
				if (dot>-1)
				{
					suffix = filename.Substring(dot+1);
				}
				
				if (dot==-1 || suffix.ToLower()!="cbl")
				{
					Console.WriteLine("Program filename must end in .cbl");
					return;
				}
				
				Console.WriteLine("Compiling "+filename);
				string assemblyName = filename.Substring(0,dot);			
				
				//Reading...
				StreamReader sr = new StreamReader(filename);
				string program = "";
				while (sr.Peek()!=-1)
				{
					program += sr.ReadLine()+"\n";
				}
				sr.Close();

                ReferenceManager referenceManager = new ReferenceManager(_references);

				//Parsing...
				Parser.Parser parser = new Parser.Parser();
				Program ast = parser.Parse(program, _verbose);
				ast.References = _references;
				
				//Contextual analysis...
	            ContextualAnalyzer analyzer = new ContextualAnalyzer();
                analyzer.Analyze(ast, referenceManager);
				
				//Code generation...
                Generator generator = new Generator(ast, referenceManager);
				generator.GenerateIL(assemblyName);
				
				Assemble(assemblyName);
			}
			catch(CompilerException e)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine(e.ToString());
				if (_verbose)
					Console.WriteLine("Stack trace:\n" + e.StackTrace);
			}
			catch(System.IO.FileNotFoundException e)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine("File not found: "+e.Message);
				Console.WriteLine(e.ToString());
			}
		}
		
		private string GetAssemblerPath()
		{
			string path = GetExecutablePath("ilasm2");
			if (path==null)
			{
				path = GetExecutablePath("ilasm");
			}
			return path;
		}

		private string GetPkgconfigPath()
		{
			return GetExecutablePath("pkg-config");
		}

		private string GetExecutablePath(string executableName)
		{
			int i;
			string ret = null;
			IDictionary env = Environment.GetEnvironmentVariables();
			foreach(DictionaryEntry var in env)
			{
				if (var.Key as string =="PATH"){
					//ILASM in later versions of Mono on OS X doesn't seem to 
					//work with the IL code generated by the compiler.
					string PATH= "/Library/Frameworks/Mono.framework/Versions/1.1.17.1/bin:"+var.Value;
					string[] paths = (PATH as string).Split(new char[] {':'});
					for (i=0;i<paths.Length;i++)
					{
						string t = paths[i]+"/"+executableName;
						if (File.Exists(t)){
							return t;
						}
					}
				}
				if (var.Key as string =="Path"){
					string[] paths = (var.Value as string).Split(new char[] {';'});
					for (i=0;i<paths.Length;i++)
					{
						string t = paths[i]+"\\"+executableName+".exe";
						if (File.Exists(t)){
							return t;
						}
						t = paths[i]+"\\Microsoft.NET\\Framework";
						if (Directory.Exists(t)){
							string[] dirs = System.IO.Directory.GetDirectories(t);
                            for (int j=dirs.Length-1;j>-1;j--)
                            {
                                string ilasm = dirs[j]+"\\"+executableName+".exe";
        						if (File.Exists(ilasm)){
                                    return ilasm;
                                }
                            }
						}
					}
				}
			}
			return ret;
		}
		
		private void Assemble(string assemblyName)
		{
			string path = GetAssemblerPath();
            if (path==null)
            {
                Console.WriteLine("ERROR: ilasm is not in your current path");
                return;
            }
            if (_verbose)
				Console.WriteLine("Assembling IL code with "+path);
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = path;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.Arguments = "/debug " + assemblyName+".il";
			process.StartInfo.RedirectStandardOutput = true;
			process.Start();
			StreamReader fromAssembler = process.StandardOutput;
			bool wasSuccessful = false;
			string buffer = "";
			while (!wasSuccessful)
			{
				string line = fromAssembler.ReadLine();
				if (line==null)
				{
					Console.WriteLine("Error");
					break;
				}
				buffer += line;
				buffer += "\n";
				if (line == "Operation completed successfully"){
					wasSuccessful = true;
				}
				if (line.IndexOf("Error at")==0){
					Console.WriteLine("Error: "+line);
					break;
				}
			}
			if (wasSuccessful){
				Console.WriteLine("Compilation succeeded");
			}else{
				Console.WriteLine("Compilation failed");
			}
		}
		
		private string[] GetPackageAssemblyPaths(string packageName)
		{
			string packagePath = null;
			string path = GetPkgconfigPath();
			if (path==null)
			{
                Console.WriteLine("ERROR: pkgconfig is not in your current path");
                return null;
			}
			//Console.WriteLine("Using pkgconfig in "+path);
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = path;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.Arguments = "--libs " + packageName;
			process.StartInfo.RedirectStandardOutput = true;
			process.Start();
			StreamReader fromAssembler = process.StandardOutput;
			bool wasSuccessful = false;
			string buffer = "";
			while (!wasSuccessful)
			{
				string line = fromAssembler.ReadLine();
				if (line==null)
				{
					Console.WriteLine("Error");
					break;
				}
				if (line.Length>3)
				{
					if (line.Substring(0,3) == "-r:")
					{
						string[] splitter = new string[1];
						splitter[0]="-r:";
						string[] r = line.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
						for (int i=0;i<r.Length;i++)
						{
							r[i]=r[i].Trim();
							if (r[i].Length>3 && r[i].Substring(0,3)=="-r:")
							{
								r[i]=r[i].Substring(3);
							}
						}
						return r;
					}
				}
				if (line.IndexOf("not found")>-1)
				{
					Console.WriteLine(line);
					Console.WriteLine("Perhaps you should add the directory containing `cocoa-sharpasa.pc'");
					Console.WriteLine(" to the PKG_CONFIG_PATH environment variable");
					return null;
				}
			}
			return null;
		}
	}
}
