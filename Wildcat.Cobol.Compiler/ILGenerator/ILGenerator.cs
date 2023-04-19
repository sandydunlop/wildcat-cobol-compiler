// Copyright (C) 2006-2023 Sandy Dunlop (sandy@sorn.net)
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
using System.Collections;
using System.Collections.Generic;
using Wildcat.Cobol.Compiler.References;
using Wildcat.Cobol.Compiler.Structure;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler.ILGenerator
{
    public partial class Generator
    {
        private Program _program;
        private bool _stringVerbUsed;
        private int _instructionCounter;
        private ReferenceManager _referenceManager;
        private string _structDefinitions;
        private const string _level88prefix = "_level88bool_";

        public Generator(Program program, ReferenceManager referenceManager)
        {
            _program = program;
            _stringVerbUsed = false;
            _instructionCounter = 0;
            _referenceManager = referenceManager;
        }

        public void GenerateIL(string assemblyName)
        {
            string intermediateFile = assemblyName + ".il";
            StreamWriter sw = File.CreateText(intermediateFile);
			Hashtable assemblyAlreadyDefined = new Hashtable();
			
			bool referencedCorlib = false;
			if (_program.Environment!=null &&
			    _program.Environment.ConfigurationSection!=null &&
			    _program.Environment.ConfigurationSection.Repository!=null)
			{
	        	Repository rep = _program.Environment.ConfigurationSection.Repository;
	        	foreach (ClassDefinition classDef in rep.Classes)
	        	{
	        		if (classDef!=null)
	        		{
	        			if (classDef.CILAssemblyName==null)
	        			{
	        				//TODO: Better exception
	        				//This probably happens when an assembly or packaged isn't specified
	        				throw new Compiler.Exceptions.CompilerException("Unknown assembly for class "+classDef.Name);
	        			}
		           		if (assemblyAlreadyDefined.Contains(classDef.CILAssemblyName) == false)
		           		{
		           			if (classDef.CILAssemblyName=="mscorlib")
		           			{
		           				referencedCorlib = true;
		           			}
		           			assemblyAlreadyDefined.Add(classDef.CILAssemblyName,true);
		           			string ver = classDef.Assembly.GetName().Version.ToString();
		           			ver = ver.Replace(".",":");
		           			sw.WriteLine(".assembly extern {0}", classDef.CILAssemblyName);
		           			sw.WriteLine("{");
		           			sw.WriteLine("  .ver {0}",ver);
		                    AssemblyName asmName = classDef.Assembly.GetName();
		                    if (asmName != null)
		                    {
		                        string pubKeyStr = "";
		                        int kp = asmName.FullName.LastIndexOf("=");
		                        string tmp = asmName.FullName.Substring(kp + 1).ToUpper();
		                        for (int i = 0; i < tmp.Length-1; i += 2)
		                        {
		                            pubKeyStr += tmp.Substring(i, 2);
		                            pubKeyStr += " ";
		                        }
		                        sw.WriteLine("  .publickeytoken = ({0})", pubKeyStr);
		                    }
		           			sw.WriteLine("}");
		           		}
	        		}
	        	}
			}
			if (!referencedCorlib)
			{
				//TODO: Dynamically get publickeytoken for corlib
				sw.WriteLine(".assembly extern mscorlib");
				sw.WriteLine("{");
				sw.WriteLine("  .ver 2:0:0:0");
				sw.WriteLine("  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )");
				sw.WriteLine("}");
			}
            
            sw.WriteLine(".assembly {0} {{}}", assemblyName);

			_structDefinitions = "";
			string dataDescriptions = "";
			if (_program.Data!=null)
			{
				dataDescriptions = EmitDataDescriptions(_program.Data.DataDescriptions);
				sw.WriteLine("\n");
				sw.WriteLine(_structDefinitions);
				sw.WriteLine("\n");
			}
			//TODO: If the FileSection contains data descriptions, then it needs to be emitted here too

            sw.WriteLine(".class public auto ansi beforefieldinit __CobolProgram");
            sw.WriteLine("       extends [mscorlib]System.Object");
            sw.WriteLine("{");
            
            //Add attributes...
            if (_program.Environment!=null)
            {
            	if (_program.Environment.ConfigurationSection!=null)
	            {
	            	if (_program.Environment.ConfigurationSection.Attributes!=null)
		            {
			            sw.WriteLine("    "+EmitAttributes(_program.Environment.ConfigurationSection.Attributes.Value));
		            }
	            }
            }

			sw.WriteLine(dataDescriptions);

            sw.WriteLine("\n    .method public hidebysig specialname rtspecialname");
            sw.WriteLine("    instance void .ctor() cil managed");
            sw.WriteLine("    {");
            int maxStack;
            if (_program.Data==null)
            {
            	maxStack = 8;
            }
            else
            {
            	maxStack = _program.Data.DataDescriptions.Count * 2;
            }
            sw.WriteLine("        .maxstack {0}", maxStack);

            //Variables declared here
			if (_program.Data!=null)
			{
	            foreach (DataDescription data in _program.Data.DataDescriptions)
	            {
	                if (data.IsSpaces)
	                {
	                	//Console.WriteLine("*** SPACES: "+data.Size);
	                    data.Type = DataType.String;
	                    string tmp = "";
	                    for (int i = 0; i < data.Size; i++)
	                    {
	                        tmp += " ";
	                    }
	                    data.Value = tmp;
	                }
	            }
	            foreach (DataDescription data in _program.Data.DataDescriptions)
	            {
	               	if (data.Name != null && data.Type != DataType.Unknown)
	                {
	                    if (data.Type == DataType.String && data.Value == null && data.IsGroup)
	                    {
	                        //TODO: Make up the group value from all of its members
	                        //TODO: This might fail for multiple levels of groups
	                        string total = "";
	                        foreach (DataDescription data2 in data.Elements)
	                        {
	                            if (data2.ParentGroup == data)
	                            {
	                                //TODO: This is wrong when data2.IsSpaces is true
	                                total += data2.Value;
	                            }
	                        }
	                        data.Value = total;
	                    }
	                }
	            }
	
	            foreach (DataDescription data in _program.Data.DataDescriptions)
	            {
	                //Console.WriteLine("  " + data.Name);
	                if (data.Name != null && data.Type != DataType.Unknown && data.Level!=88)
	                {
	                	if (data.IsArray)
	                	{
							sw.WriteLine("        ldarg.0 ");
							sw.WriteLine("        ldc.i4 0x{0:x8}",data.Occurs);
							sw.WriteLine("        newarr _"+ILIdentifier(data.Name));
							sw.WriteLine("        stfld valuetype _"+
									ILIdentifier(data.Name)+"[] __CobolProgram::"+
									ILIdentifier(data.Name));
	                	}
	                	else
	                	{
	                		if (data.ParentGroup==null || data.ParentGroup.IsArray==false)
	                		{
	                			//TODO: Contents of array should be set
			                    if (data.Type == DataType.String)
			                    {
			                        sw.WriteLine("        ldarg.0");
			                        sw.WriteLine("        ldstr \"{0}\"", data.Value);
			                    }
			                    else if (data.Type == DataType.Integer)
			                    {
			                        string v;
			                        //Console.WriteLine("DEBUG: wsi = "+wsi.Value);
			                        if (data.Value == null)
			                        {
			                            v = String.Format("0x{0:x8}", 0);
			                        }
			                        else
			                        {
			                            v = String.Format("0x{0:x8}", Int32.Parse(data.Value));
			                        }
			                        if (v != "")
			                        {
			                            sw.WriteLine("        ldarg.0"); //Hidden 'this' pointer
			                            sw.WriteLine("        ldc.i4 {0}", v);
			                        }
			                    }
			                    else
			                    {
			                        //TODO: More data types here
			                    }
			                    string type = ILType(data.Type);
			                    string name = ILIdentifier(data.Name);
			                    sw.WriteLine("        stfld {0} __CobolProgram::{1}", type, name);
	                		}
	                	}
	                }
	            }
			}

            sw.WriteLine("        ret");
            sw.WriteLine("    }");

            foreach (Paragraph para in _program.Procedure.Paragraphs)
            {
                sw.WriteLine(EmitParagraph(para));
            }

            //Support method for COBOL's STRING verb
            if (_stringVerbUsed)
            {
                sw.WriteLine(EmitStringVerbMethod());
            }

            sw.WriteLine("}\n");

            string mainParagraph = null;
            if (_program.Procedure.Paragraphs.Count > 0)
            {
                Paragraph para = _program.Procedure.Paragraphs[0] as Paragraph;
                mainParagraph = ILIdentifier(para.Name);
            }
            sw.WriteLine(".method static public void main() il managed");
            sw.WriteLine("{");
            sw.WriteLine("    .entrypoint");
            sw.WriteLine("    .maxstack {0}", 8);
            sw.WriteLine("    newobj instance void __CobolProgram::.ctor()");
            sw.WriteLine("    call instance void __CobolProgram::{0}()", mainParagraph);
            sw.WriteLine("    ret");
            sw.WriteLine("}");
            sw.Flush();
            sw.Close();
        }
        
        private Identifier GetIdentifier(string name)
        {
        	//Need:
        	// id.Definition
        	// id.Name
        	foreach (DataDescription data in _program.Data.DataDescriptions)
        	{
        		if (data.Name == name)
        		{
        			Identifier id = new Identifier();
        			id.Name = name;
        			id.Definition = data;
        			return id;
        		}
        	}
        	Console.WriteLine("TODO: Exception");
        	return null;
        }

		private string EmitFileDescriptors()
		{
			string r = "";
			if (_program.Data.FileSection!=null)
			{
				foreach (FileAndSortDescriptionEntry fsde in _program.Data.FileSection.Entries)
            	{
            		//Emit file descriptor declaration
            		r+="    .field private class [mscorlib]System.IO.StreamReader _reader_"+ILIdentifier(fsde.Name.Name)+"\n";
            		r+="    .field private class [mscorlib]System.IO.StreamWriter _writer_"+ILIdentifier(fsde.Name.Name)+"\n";
            	}
			}
			return r;
		}
		
        private string EmitDataDescriptions(ArrayList descriptions)
        {
        	string r = "";
        	r+=EmitFileDescriptors();
            foreach (DataDescription data in descriptions)
            {
                //Console.WriteLine("DDE: "+data.Level+":"+data.Name);
                if (data.ParentGroup==null || data.ParentGroup.IsArray==false)
                {
                	if (data.Level == 88)
                	{
                		//Need to store declared value of level 88 variables?
                		//Probably not. The compiler always knows what they should be.
                		if (data.IsHighValues)
                		{
                			//TODO: Should this even exist?
	                        r+="    .field public bool "+_level88prefix + ILIdentifier(data.Name) + "\n";
                		}
                		else
                		{
                			//TODO: Implement this
	                        r+="    .field public bool "+_level88prefix + ILIdentifier(data.Name) + "\n";
                		}
                	}
                	else
                	{
		               	if (data.IsClass)
		               	{
		               		ClassDefinition classDef = data.ClassDefinition;
		               		//TODO: Some types may require an assembly name?
		                    r+="    .field public " + ILType(classDef) + " " + ILIdentifier(data.Name) + "\n";
		               	}
		                else if (data.Name != null && data.Type != DataType.Unknown)
		                {
		                    if (data.Occurs>0) 
		                    {
		                        //This should be declared differently.
		                        //
		                        // If it's an array with multiple elements on the same
		                        // level,then it needs to be an array of structs
		                        //
		                        // If it's the only item, then it needsto be a normal 
		                        // array
		                        
		                        string newStruct = "";
		                        newStruct+=".class private sequential ansi sealed beforefieldinit _"+ILIdentifier(data.Name)+"\n";
		                        newStruct+="extends [mscorlib]System.ValueType\n";
		                        newStruct+="{\n";
	
	  	                        newStruct+=EmitDataDescriptions(data.Elements);
		                        newStruct+="}\n";
		                        _structDefinitions+=newStruct;
		                        data.IsArray = true;
		                        r+="    .field public valuetype _" + ILIdentifier(data.Name) + "[] " + ILIdentifier(data.Name) + "\n";
		                    }else{
		                        r+="    .field public " + ILType(data.Type) + " " + ILIdentifier(data.Name) + "\n";
		                        if (data.IsGroup){
		                            //This is a group, so it needs a boolean flag to indicate if its empty
    		                        r+="    .field public bool _hasData_" + ILIdentifier(data.Name) + "\n";
		                        }
		                    }
		                }
		                else
		                {
		                	//TODO: Should this be an error or just a declaration of a struct/record?
		                	//throw new Compiler.Exceptions.CompilerException("Unknown data type: "+data.Name);
		                	Console.WriteLine("WARNING: Data type of "+data.Name+" is unknown");
		                }
                	}
                }
            }
            return r;
        }

        private string EmitAttributes(string attributes)
        {
            //TODO: THat only deals with one attribute. We could have a list.

            string r = "";
            string ass = _referenceManager.GetAttributeCILName(attributes);
            r += ".custom instance void class " + ass + "::.ctor() =  (01 00 00 00)\n";
            return r;
        }

        private bool IsGroup(Source source)
        {
          	if (source.GetType() == typeof(Identifier))
          	{
          		Identifier id = source as Identifier;
          		return IsGroup(id);
          	}
          	return false;
        }
        
        private bool IsGroup(Identifier id)
        {
      		if (id.Definition.IsGroup)
      		{
      			return true;
      		}
      		return false;
        }

        private List<DataDescription> ListElements(DataDescription dd)
        {
   			List<DataDescription> ddes = new List<DataDescription>();
   			ListElements(dd, ddes);
   			return ddes;
        }
        
        private void ListElements(DataDescription dd, List<DataDescription> ddes)
        {
        	if (dd.IsGroup)
        	{
        		foreach (DataDescription elem in dd.Elements)
        		{
	        		ListElements(elem, ddes);
        		}
        	}
        	else
        	{
        		ddes.Add(dd);
        	}
        }

        private string ILIdentifier(string cobolIdentifier)
        {
            string num = "0123456789";
            string r = cobolIdentifier.Replace("-", "_");
            r = r.Replace("#", "__hash__");
            if (r.Length > 0)
            {
                if (num.IndexOf(r.Substring(0, 1)) != -1)
                {
                    r = "_" + r;
                }
            }
            return r;
        }

		//TODO: There is no need to have DataType and VariableType being seperate
        private string ILType(DataType type)
        {
            if (type == DataType.String)
            {
                return "string";
            }
            else if (type == DataType.Integer)
            {
                return "int32";
            }
            else if (type == DataType.Boolean)
            {
                return "bool";
            }
            else
            {
            	throw new Compiler.Exceptions.CompilerException("Unknown type");
            	//TODO: Put a line number in this error message
            }
        }
        private string ILType(VariableType type)
        {
            if (type == VariableType.String)
            {
                return "string";
            }
            else if (type == VariableType.Integer)
            {
                return "int32";
            }
            else if (type == VariableType.Boolean)
            {
                return "bool";
            }else{
            	throw new Compiler.Exceptions.CompilerException("Unknown type");
            	//TODO: Put a line number in this error message
            }
        }
        private string ILType(string type)
        {
            if (type == "System.String")
            {
                return "string";
            }
            else if (type == "System.Int32")
            {
                return "int32";
            }
            else if (type == "System.Void")
            {
                return "void";
            }
            else if (type == "System.Boolean")
            {
                return "bool";
            }
            else if (type == "System.Object")
            {
                return "object";
            }
            else
            {
                //TODO: assembly name needs to go in front of this
                //like [assembly]type
                string t = "class ['" + GetAssemblyNameForType(type) + "']" + type;
                return t;
            }
        }
        private string ILType(ClassDefinition classDef)
        {
            //TODO: assembly name needs to go in front of this
            //like [assembly]type
            string t = "class ['" + GetAssemblyNameForType(classDef.NetName.Value) + "']" + classDef.Type;
            return t;
        }

        private string GetAssemblyNameForType(string typeName)
        {
            return _referenceManager.GetAssemblyName(typeName);
        }

        private string FXType(DataType type)
        {
            if (type == DataType.String)
            {
                return "System.String";
            }
            else if (type == DataType.Integer)
            {
                return "System.Int32";
            }
            return null;
        }
        private string FXType(VariableType type)
        {
            if (type == VariableType.String)
            {
                return "System.String";
            }
            else if (type == VariableType.Integer)
            {
                return "System.Int32";
            }
            return null;
        }
        
        private Type GetType(VariableType type)
        {
            if (type == VariableType.String)
            {
                return typeof(System.String);
            }
            else if (type == VariableType.Integer)
            {
                return typeof(System.Int32);
            }
            return null;
        }
        
        private Type GetType(DataType type)
        {
            if (type == DataType.String)
            {
                return typeof(System.String);
            }
            else if (type == DataType.Integer)
            {
                return typeof(System.Int32);
            }
            return null;
        }
        
        //Return current IL Address and increment counter
        private string ILAddress(int inc)
        {
            string r = String.Format("IL_{0:x4}: ", _instructionCounter);
            _instructionCounter += inc;
            return r;
        }

        private string ILAddress()
        {
            string r = String.Format("IL_{0:x4}", _instructionCounter);
            return r;
        }

        private string EmitParagraph(Paragraph para)
        {
            string r = "";

            r += "    .method public void " + ILIdentifier(para.Name) + "() cil managed\n";
            r += "    {\n";
            if (para.Attributes != null)
            {
            	r+="        ";
            	r+=EmitAttributes(para.Attributes.Value);
            }
            r += "        ";
            r += ".maxstack 8"; //TODO: Make dynamic
            r += "\n";
            r += "        ";
            r += ".locals init (\n";
            r += "        ";
            r += "        [0] bool CS$4$0000,\n";
            r += "        ";
            r += "        [1] string __cobolInputTemp,\n";
            r += "        ";
            r += "        [2] int32 __cobolIntTemp)\n";
            _instructionCounter = 0;

            foreach (Sentence sentence in para.Sentences)
            {
                r += EmitSentence(sentence);
            }

            r += "\n";
            r += "        " + ILAddress(1) + "ret\n";
            r += "    }\n";
            return r;
        }

        private string EmitSentence(Sentence sentence)
        {
            string r = "";
            //r+="// Line "+sentence.Command.LineNumber+"\n";
            if (sentence.Command.GetType() == typeof(DisplayVerb))
            {
                r += EmitDisplayVerb(sentence.Command as DisplayVerb);
            }
            else if (sentence.Command.GetType() == typeof(AcceptVerb))
            {
                r += EmitAcceptVerb(sentence.Command as AcceptVerb);
            }
            else if (sentence.Command.GetType() == typeof(StringVerb))
            {
                r += EmitStringVerb(sentence.Command as StringVerb);
            }
            else if (sentence.Command.GetType() == typeof(PerformVerb))
            {
                r += EmitPerformVerb(sentence.Command as PerformVerb);
            }
            else if (sentence.Command.GetType() == typeof(IfStatement))
            {
                r += EmitIfStatement(sentence.Command as IfStatement);
            }
            else if (sentence.Command.GetType() == typeof(MoveStatement))
            {
                r += EmitMoveStatement(sentence.Command as MoveStatement);
            }
            else if (sentence.Command.GetType() == typeof(AddStatement))
            {
                r += EmitAddStatement(sentence.Command as AddStatement);
            }
            else if (sentence.Command.GetType() == typeof(SubtractStatement))
            {
                r += EmitSubtractStatement(sentence.Command as SubtractStatement);
            }
            else if (sentence.Command.GetType() == typeof(MultiplyStatement))
            {
                r += EmitMultiplyStatement(sentence.Command as MultiplyStatement);
            }
            else if (sentence.Command.GetType() == typeof(DivideStatement))
            {
                r += EmitDivideStatement(sentence.Command as DivideStatement);
            }
            else if (sentence.Command.GetType() == typeof(SetStatement))
            {
                r += EmitSetStatement(sentence.Command as SetStatement);
            }
            else if (sentence.Command.GetType() == typeof(InvokeStatement))
            {
                r += EmitInvokeStatement(sentence.Command as InvokeStatement);
            }
            else if (sentence.Command.GetType() == typeof(ExitStatement))
            {
                r += EmitExitStatement(sentence.Command as ExitStatement);
            }
            else if (sentence.Command.GetType() == typeof(OpenStatement))
            {
                r += EmitOpenStatement(sentence.Command as OpenStatement);
            }
            else if (sentence.Command.GetType() == typeof(CloseStatement))
            {
                r += EmitCloseStatement(sentence.Command as CloseStatement);
            }
            else if (sentence.Command.GetType() == typeof(ReadStatement))
            {
                r += EmitReadStatement(sentence.Command as ReadStatement);
            }
            else if (sentence.Command.GetType() == typeof(WriteStatement))
            {
                r += EmitWriteStatement(sentence.Command as WriteStatement);
            }
            else
            {
                throw new Compiler.Exceptions.NotImplementedException(sentence.Command.LineNumber, "" + sentence.Command);
            }
            return r;
        }

        private string EmitExitStatement(ExitStatement exitstatement)
        {
        	string r = "";
			r += "        " + ILAddress(1) + "ldc.i4.0\n";
			r += "        " + ILAddress(5) + "call void class [mscorlib]System.Environment::Exit(int32)\n";
        	return r;
        }
        
        private string EmitSetStatement(SetStatement set)
        {
        	string r = "";
        	//TODO: Make this work with things other than System.String
			//TODO: Type checking
            r += "        " + ILAddress(1) + "ldarg.0\n";
			
            //Load set.From
            r+=EmitLoadSource(set.From, false, false);
            
            //Store set.To
            r+=EmitStore(set.To);
            
            return r;
        }

        private string EmitAddStatement(AddStatement add)
        {
            string r = "";
            //TODO: Corresponding
            //Plan:
            //  If UsingGiving:
            //    Add all of From and To, and save in Giving variables
            //  Else:
            //    Add all of From, and save in To variables

            //This 'this' pointer is for the set field at the end.
            //We may need one for each item in the 'To' list?
            //Or giving list?
            r += "        ";
            r += ILAddress(1);
            r += "ldarg.0"; //Hidden pointer to 'this'
            r += "\n";

            r += "        " + ILAddress(1);
            r += "ldc.i4.0\n";
            foreach (Source source in add.From)
            {
                r += EmitLoadSource(source, false, false);
                r += "        " + ILAddress(1);
                r += "add\n";
            }
            //Add the value to the current value...
            //TODO: Current value could be a list
            r += EmitLoadSource(add.To[0] as Source, false, false);
            r += "        " + ILAddress(1);
            r += "add\n";
            //TODO: Also add TO to the total before saving
			if (add.UsingGiving)
			{
				//TODO: There can be more than one giving item
			    Identifier id = add.Giving[0] as Identifier;
			    r += EmitStore(id);
			    //TODO: Make sub, div, mul like this
			}else
			{
			    Identifier id = add.To[0] as Identifier;
			    r += EmitStore(id);
			}
            //TODO: Expand this to meet grammar requirements
            return r;
        }
        
        private string EmitSubtractStatement(SubtractStatement sub)
        {
            string r = "";
            //TODO: Corresponding
            //Plan:
            //  If UsingGiving:
            //    Add all of From and To, and save in Giving variables
            //  Else:
            //    Add all of From, and save in To variables


            {
                //This 'this' pointer is for the set field at the end.
                //We may need one for each item in the 'To' list?
                //Or giving list?
                r += "        ";
                r += ILAddress(1);
                r += "ldarg.0"; //Hidden pointer to 'this'
                r += "\n";

                r += EmitLoadSource(sub.Right[0] as Source, false, false);
                //r += "        " + ILAddress(1);

                r += "        " + ILAddress(1);
                r += "ldc.i4.0\n";
                foreach (Source source in sub.Left)
                {
                    r += EmitLoadSource(source, false, false);
                    r += "        " + ILAddress(1);
                    r += "add\n";
                }
                //Add the value to the current value...
                //TODO: Current value could be a list
                r += "        " + ILAddress(1);
                r += "sub\n";
                //TODO: Also add TO to the total before saving
	            if (sub.UsingGiving)
	            {
	            	//TODO: There could be more than one igving item
	                Identifier id = sub.Giving[0] as Identifier;
	                r += EmitStore(id);
	            }else{
	                Identifier id = sub.Right[0] as Identifier;
	                r += EmitStore(id);
	            }
                //TODO: Expand this to meet grammar requirements
            }
        	return r;
        }

        private string EmitMultiplyStatement(MultiplyStatement mul)
        {
            //TODO: This is very basic. Expand it.
            string r = "";
            r += "        " + ILAddress(1) + "ldarg.0\n";
            r += EmitLoadSource(mul.Left, false, false);
            r += EmitLoadSource(mul.Right[0] as Source, false, false);
            r += "        " + ILAddress(1) + "mul\n";
            if (mul.UsingGiving){
            	r += EmitStore(mul.Giving[0] as Identifier);
            }else{
            	//TODO: Check that this isn't a literal
            	r += EmitStore(mul.Left as Identifier);
            }
            return r;
        }
        
        private string EmitDivideStatement(DivideStatement divide)
        {
            //TODO: This is very basic. Expand it.
            string r = "";
            r += "        " + ILAddress(1) + "ldarg.0\n";
            r += EmitLoadSource(divide.Left, false, false);
            r += EmitLoadSource(divide.Right[0] as Source, false, false);
            r += "        " + ILAddress(1) + "div\n";
            r += EmitStore(divide.Giving[0] as Identifier);

            if (divide.Remainder != null)
            {
	            //TODO: Does DIVIDE have an INTO word?
                r += "        " + ILAddress(1) + "ldarg.0\n";
                r += EmitLoadSource(divide.Left, false, false);
                r += EmitLoadSource(divide.Right[0] as Source, false, false);
                r += "        " + ILAddress(1) + "rem\n";
                r += EmitStore(divide.Remainder);
            }
            return r;
        }

        private string EmitMoveStatement(MoveStatement move)
        {
            string r = "";
            //TODO: Implement"corresponding"
            //TODO: Implement "MOVE SPACES..."
            if (move.From.GetType() == typeof(FigurativeConstant))
            {
                FigurativeConstant constant = move.From as FigurativeConstant;
                if (constant.Type == FigurativeConstantType.Spaces)
                {
                    //TODO: Type checking
                    //TODO: Line number in exception
                    //TODO: Look variable up in definitions and get size, then move n spaces into it.
                    foreach (Identifier toIdentifier in move.To)
                    {
                        int size = toIdentifier.Definition.Size;
                        //Console.WriteLine("MOVE SPACES TO variable of size "+size);
                        r += "        ";
                        r += ILAddress(1) + "ldarg.0"; //Hidden pointer to 'this'
                        r += "\n";
                        Literal literal = new Literal();
                        literal.Type = VariableType.String;
                        literal.Value = "";
                        for (int i = 0; i < size; i++)
                        {
                            literal.Value += " ";
                        }
                        r += EmitLiteral(literal);
                        r += EmitStore(toIdentifier); //TODO: This needs changed for subscripts
                        if (IsGroup(toIdentifier))
                        {
                            r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
                            r += "        " + ILAddress(1) + "ldc.i4.0\n"; //Zero, used for 'false'
                            r += "        " + ILAddress(5) + "stfld bool __CobolProgram::_hasData_"+ILIdentifier(toIdentifier.Definition.Name)+"\n";
                        }
                    }
                }else{
                    Console.WriteLine("TODO: Other figurative constants not supported yet");
                }
            }
            else
            {
                foreach (Identifier toIdentifier in move.To)
                {
                	bool isMovingToGroup = false;
                	bool isMovingToArray = false;
                	bool isMovingFromArray = false;
                	Identifier fromIdentifier = null;
                	
                	//TODO: 
                	// The following code identifies moving structs to structs
                	// The first bit of code identifies arrays
                	// Need to implement the actual moving for all possible combinations
                	//
                	// At the moment an array of structs does not appear as an array in the IL code
                	// That needs fixed before this will work properly
                	
                	if (toIdentifier.Subscript!=null)
                	{
	                	isMovingToArray = true;
                	}
                	if (move.From.IsLiteral==false)
                	{
                		fromIdentifier = move.From as Identifier;
                		if (fromIdentifier.Subscript!=null)
	                	{
		                	isMovingFromArray = true;
	                	}
                	}
                   	if (toIdentifier.Definition.IsGroup)
                   	{
                		//TODO: moving a non-group into a group and vice-versa
                   		//Console.WriteLine("Moving to a group");
	                   	if (move.From.IsLiteral==false)
	                   	{
	                   		fromIdentifier = move.From as Identifier;
	                   		if (fromIdentifier.Definition.IsGroup){
		                   		//Console.WriteLine("Moving from a group");
		                   		isMovingToGroup =true;
	                   		}
	                   	}
	                   	//TODO: Literals to groups
                   	}
                	
                	if (isMovingToGroup)
                	{
                		//TODO: This needs to support arrays
                		
                		int n;
               			fromIdentifier = move.From as Identifier;
                		for (n=0;n<toIdentifier.Definition.Elements.Count;n++)
                		{
                			DataDescription fromData = fromIdentifier.Definition.Elements[n] as DataDescription;
                			Identifier idFrom = new Identifier();
                			idFrom.Name = fromData.Name;
                			idFrom.Definition = fromData;
                			DataDescription toData = toIdentifier.Definition.Elements[n] as DataDescription;
		                    
		                    if (isMovingToArray)
		                    {
		                    	//Console.WriteLine("TODO: Move to array");
		                    	//TODO: ALso need to implement moving from an array back to a normal struct
			                    r += "        "+ILAddress(1) + "ldarg.0\n";
			                    r += "        "+ILAddress(5) + "ldfld valuetype _"+ILIdentifier(toIdentifier.Name)+
			                    						"[] __CobolProgram::"+ILIdentifier(toIdentifier.Name)+"\n";
			                    r+=EmitExpression(toIdentifier.Subscript);
			                    r += "        "+ILAddress(5) + "ldelema _"+ILIdentifier(toIdentifier.Name)+"\n";
			                    r += EmitLoadSource(idFrom,false,false);
	                			Identifier idTo = new Identifier();
	                			idTo.Name = toData.Name;
	                			idTo.Definition = toData;
			                    r += EmitStore(idTo,"_"+ILIdentifier(toIdentifier.Name));
		                    }
		                    else
		                    {
	                			//Load the source data...
		                    	if (isMovingFromArray)
		                    	{
		                    		//TODO: Checl that fromIdentifier is not null
				                    r += "        "+ILAddress(1) + "ldarg.0\n";
				                    r += "        "+ILAddress(1) + "ldarg.0\n";
				                    r += "        "+ILAddress(5) + "ldfld valuetype _"+ILIdentifier(fromIdentifier.Name)+
				                    						"[] __CobolProgram::"+ILIdentifier(fromIdentifier.Name)+"\n";
			                    	r+=EmitExpression(fromIdentifier.Subscript);
				                    r += "        "+ILAddress(5) + "ldelema _"+ILIdentifier(fromIdentifier.Name)+"\n";
		                    		r += EmitLoadField(idFrom,false,false,"_"+ILIdentifier(fromIdentifier.Name));
		                    		//r+=EmitStore(toIdentifier);
		                    	}
		                    	else
		                    	{
				                    r += "        ";
				                    r += ILAddress(1) + "ldarg.0"; //for emitstore
				                    r += "\n";
				                    r += EmitLoadSource(idFrom,false,false);
		                    	}
	
			                    //Store in target variable...
	                			Identifier idTo = new Identifier();
	                			idTo.Name = toData.Name;
	                			idTo.Definition = toData;
		                        r += EmitStore(idTo);
		                    }
                		}
                	}
                	else
                	{
                		//TODO: This needs to support arrays
	                    r += "        ";
	                    r += ILAddress(1) + "ldarg.0"; //for emitstore
	                    r += "\n";
	                    r += EmitLoadSource(move.From,false,false);
	
	                    if (toIdentifier.SubstringLeft != null)
	                    {
	                        //empty string:
	                        //TODO: This needs to be null instead of empty string
	                        //Literal literal = new Literal();
	                        //literal.Type = VariableType.String;
	                        //literal.Value = "";
	                        //r += EmitLiteral(literal);
	
	                        r += "        " + ILAddress(1) + "ldnull\n";
	
	                        //ref string (output):
	                        r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to this
	                        r += EmitLoadField(toIdentifier, true, true); //load pointer to target
	
	                        //Now laod the subscript
	                        r += EmitExpression(toIdentifier.SubstringLeft);
	
	                        //Now laod a pointer to a tmp string
	                        r += "        " + ILAddress(2);
	                        r += "ldloca.s __cobolIntTemp\n";  //TODO: THis needs to load the aress of the local variable
	
	                        //Now call __COBOLSTRING
	                        r += "        " + ILAddress(5) + "call instance void class __CobolProgram::__CobolString(string, string, [out] string&, int32, [out] int32&)\n";
	                    }
	                    else
	                    {
	                        r += EmitStore(toIdentifier);
	                    }
                	}
                }
            }
            //Console.WriteLine("MOVE\n"+r);
            return r;
        }

        private string EmitStore(Identifier identifier)
        {
        	return EmitStore(identifier,null);
        }
        
        private string EmitStore(Identifier identifier, string className)
        {
            string r = "";
            string typ = null;
            if (identifier.Definition.Level == 88)
            {
            	//21:51 on 21st June 2007
            	//Listening to: Trivium's The Crusade album
            	//Console.WriteLine("Storing at Level 88");
            	//TODO: Implement and test this
            	
            	if(identifier.Definition.IsHighValues)
            	{
            		//Set value of entire record and set this to true/false?
            		//TODO: I've commented this warning out. Can't remember why it was there.
            		//Console.WriteLine("Warning: SET with HIGH-VALUES isn't fully implemented yet");
		            r += "        " + ILAddress(5);
		            if (className==null)
		            	className = "__CobolProgram";
		            r += "stfld bool "+className+"::"+ _level88prefix + ILIdentifier(identifier.Name) + "\n";
            	}
            	else
            	{
            		throw new Compiler.Exceptions.CompilerException("Level 88 only supports HIGH-VALUE and HIGH-VALUES at present");
            	}
            }
            else
            {
            	if (identifier.Definition.IsGroup)
            	{
            		int p = 0;
					r+=EmitStoreGroup(identifier, ref p);
            	}
            	else
            	{
		            if (identifier.Definition.IsClass)
		            {
		            	typ = ILType(identifier.Definition.ClassDefinition.NetName.Value);
		            }
		            else
		            {
		            	typ = ILType(identifier.Definition.Type);
		            }
		            r += "        " + ILAddress(5);
		            if (className==null)
		            	className = "__CobolProgram";
		            r += "stfld " + typ + " "+className+"::" + ILIdentifier(identifier.Name) + "\n";
            	}
            }
            return r;
        }
        
        private string EmitStoreGroup(Identifier identifier, ref int p)
        {
        	string r = "";
       		//This only works when it's not coming from a group...
  			if (p==0)
  			{
  				//If p>0, then the string is already stored
	            //Store...
		        r += "        " + ILAddress(1);
	            r += "stloc.1\n";
  			}
            //TODO: That breaks when groups are inside groups as the second one
            //      overwrites the string of the first one
          
      		//TODO: Split up string and store parts in group's variables
      		foreach (DataDescription dd in identifier.Definition.Elements)
      		{
      			//only doing this when IsGroup is false prevents recursion
      			//it stops the next level of elements being set though
      			if (dd.Level!=88)
      			{
      				if (dd.IsGroup==false)
      				{
		       			r+="\n";
		       			
		       			//Load p
		       			//Load length of string
		       			//if p>=length, skip this
			            r += "        " + ILAddress(5);
			            r += "ldc.i4 "+p+"\n";
			            r += "        " + ILAddress(1);
			            r += "ldloc.1\n";
			            r += "        " + ILAddress(5);
                        r += "callvirt instance int32 string::get_Length()\n"; 
			            r += "        " + ILAddress(5);
			            r += "bge "; //Address will be added here later

		       			string r2="";
		       			
		       			//Set boolean flag to indicat group is not empty here
                        r2 += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
                        r2 += "        " + ILAddress(1) + "ldc.i4.1\n"; //Zero, used for 'false'
                        r2 += "        " + ILAddress(5) + "stfld bool __CobolProgram::_hasData_"+ILIdentifier(identifier.Definition.Name)+"\n";
		       			
		       			//Pointer to this...
				        r2 += "        " + ILAddress(1);
			            r2 += "ldarg.0\n";
			         			
			         	//Load the string...
				        r2 += "        " + ILAddress(1);
			            r2 += "ldloc.1\n";
			            
			            //Take a substring out of it (based on DDE's size)...
			            // ldc.i4.n
			            r2+= "        " + ILAddress(5);
			            r2+= "ldc.i4 "+p+"\n";
			            // ldc.i4.n
			            r2+= "        " + ILAddress(5);
			            
			            if (dd.Size==0)
			            {
			            	Console.WriteLine("*** SIZE IS ZERO FOR "+dd.Name+" ***");
			            	//If size is zero, probably throw an exception 
			            	//or skip to next element of the group
			            }
			            
			            r2+= "ldc.i4 "+dd.Size+"\n";
			            // callvirt instance string string::Substring(int32, int32)
			            r2+= "        " + ILAddress(5);
			            r2+= "callvirt instance string string::Substring(int32, int32)\n";
			            
			            p+=dd.Size;
			            
			            //TODO: May have to convert string to something else here
			            //      e.g. Integer
			            if (dd.Type == DataType.Integer)
			            {
				            r2+= "        " + ILAddress(5);
			            	r2+="call int32 int32::Parse(string)\n";
			            }
			            
			            //Save the substring to an element...
			            Identifier elem = new Identifier();
			            elem.Definition = dd;
			            elem.Name = dd.Name;
			            r2+=EmitStore(elem,null);
			            
			            //Skips to here if p>=length of string
			            r+=ILAddress()+" //<-------- \n";
			            r+=r2;
      				}else{
			            Identifier elem = new Identifier();
			            elem.Definition = dd;
			            elem.Name = dd.Name;
			            r+=EmitStoreGroup(elem,ref p);
      				}
       			}
       		}
       		//Console.WriteLine(r);  
       		return r;      
        }

        private string EmitIntrinsicFunction(IntrinsicFunction function)
        {
            string r = "";
            //Evaluate function here
            if (function.Function == IntrFunc.UpperCase)
            {
                Source param = function.Parameters[0] as Source;
                //TODO: Check amount of params?
                //r += "        ";
                //r += ILAddress(1);
                //r += "ldarg.0"; //Hidden pointer to 'this'
                //r += "\n";
                //Contextual analysis should set the type of the source here
                r += EmitLoadSource(param, false, true);
                //TODO: ^^^ This doesn't work if param has a substring value
                r += "        ";
                r += ILAddress(5) + "callvirt instance string string::ToUpper()";
                r += "\n";
            }
            else
            {
                //TODO: Exception
                Console.WriteLine("Unknown function call");
            }
            return r;
        }
        
        private string EmitLoadSource(Source src, bool address, bool boxIfPossible)
        {
            if (src.GetType() == typeof(Identifier))
            {
                string r = "";
                Identifier id = src as Identifier;
	           	if (id.Delimiter!=null){
	           		//Console.WriteLine("IDENTIFIER "+id.Name+" has delimiter "+id.Delimiter);
	           	}
                if (id.Subscript != null)
                {
                    //TODO: Implement subscript
                    //TODO: This may not be right
                    if (id.Definition.Occurs > 0)
                    {
                        DataDescription group = id.Definition.ParentGroup;
                        int arraySize = id.Definition.Occurs;
                        int elemLength = id.Definition.Size;
                        //Idea:
                        //The value to return is group.Value.Substring((subscript-1)*elemLength)
                        //Error if that+elemLength is greater than arraySize
                        //Need to call String.Substring here
                        //Use the temp input string

                        //For stfld:
                        //  IL_0001:  ldarg.0
                        //r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'

                        //The group:
                        //  IL_0002:  ldarg.0
                        //  IL_0003:  ldfld string WorkingStorageTest.Program::anon
                        while (group.RedefinesDefinition != null)
                        {
                            group = group.RedefinesDefinition;
                        }
                        Identifier groupId = new Identifier();
                        groupId.Name = group.Name;
                        
                        groupId.Definition = group;
                        r += EmitIdentifier(groupId);

                        //The subscript expression:
                        //  IL_0008:  ldarg.0
                        //  IL_0009:  ldfld int32 WorkingStorageTest.Program::k
                        //  IL_000e:  ldc.i4.1
                        //  IL_000f:  add
                        r += EmitExpression(id.Subscript);
                        Literal literal = new Literal();
                        literal.Value = "" + elemLength;
                        literal.Type = VariableType.Integer;
                        r += EmitLiteral(literal);
                        r += "        " + ILAddress(1) + "mul\n";

                        //TODO: Attempting to get correct output for test.cbl
                        r += EmitLiteral(literal);
                        r += "        " + ILAddress(1) + "sub\n";

                        //The elemLength:
                        //  IL_0010:  ldc.i4.7
                        r += EmitLiteral(literal);

                        //Call substring:
                        //  IL_0011:  callvirt instance string string::Substring(int32, int32)
                        r += "        " + ILAddress(5) + "callvirt instance string string::Substring(int32, int32)\n";

                        //Store result:
                        //  IL_0016:  stfld string WorkingStorageTest.Program::bb1
                        //Don't store this. We want it to remain at the top of the stack.
                    }
                    else
                    {
                        throw new Compiler.Exceptions.CompilerException("Subscript used with non-array at line " + id.LineNumber);
                    }
                    //More here...
                }
                else if (id.SubstringLeft!=null)
                {
                	//Console.WriteLine("*** SOURCE with SUBSTRING");
                	
                	//Load field
                    r += "        ";
                    r += ILAddress(1) + "ldarg.0"; //Hidden pointer to 'this'
                    r += "\n";
                    r += EmitLoadField(id, address, boxIfPossible);
                	
                	//Load start
                    r += EmitExpression(id.SubstringLeft);
                    Literal lit = new Literal();
                    lit.Type = VariableType.Integer;
                    lit.Value = "1";
                    r += EmitLiteral(lit);
                    r += "        " + ILAddress(1) + "sub\n";
                	
                	if (id.SubstringRight!=null)
                	{
		                r+="//With two params\n";
	                	//Load length
	                	//TODO: Change COBOL's 'end' to 'length'
	                	
	                	//Load end
	                	//Load start
	                	//Subtract
	                    r += EmitExpression(id.SubstringLeft);
	                    r += EmitExpression(id.SubstringRight);
                        r += "        " + ILAddress(1) + "sub\n";
                        lit = new Literal();
                        lit.Type = VariableType.Integer;
                        lit.Value = "1";
	                    r += EmitLiteral(lit);
                        r += "        " + ILAddress(1) + "add\n";
                        //TODO: Above needs optimized
	                	
	                	//Call substring
                        //  IL_0011:  callvirt instance string string::Substring(int32, int32)
                        r += "        " + ILAddress(5) + "callvirt instance string string::Substring(int32, int32)\n";
                	}
                	else
                	{
	                	//Call substring
                        //  IL_0011:  callvirt instance string string::Substring(int32, int32)
                        //TODO: This doesn't work
                        //      two params appear in the il code
                        //TODO: SHould it be 4 or 5
                        r += "        " + ILAddress(4) + "callvirt instance string string::Substring(int32)\n";
                	}
                }
                else
                {
                    r += "        ";
                    r += ILAddress(1) + "ldarg.0"; //Hidden pointer to 'this'
                    r += "\n";
                    r += EmitLoadField(id, address, boxIfPossible);
                }
                return r;
            }
            else if (src.GetType() == typeof(Literal))
            {
            	Literal lit = src as Literal;
                return EmitLiteral(lit,boxIfPossible);
            }
            else if (src.GetType() == typeof(IntrinsicFunction))
            {
                return EmitIntrinsicFunction(src as IntrinsicFunction);
            }
            else
            {
                throw new Compiler.Exceptions.CompilerException(src.LineNumber, "Cannot identify sentence type");
            }
        }
        
        private string EmitLoadField(Identifier id, bool address, bool boxIfPossible)
        {
        	return EmitLoadField(id,address,boxIfPossible,null);
        }
        
        private string EmitLoadField(Identifier id, bool address, bool boxIfPossible, string className)
        {
        	if (id==null)
        	{
        		Console.WriteLine("ID is Null");
        	}
        	//Console.WriteLine("load field id = "+id.Name);
            string r = "";
            //string typ = null;
            string a = "";
            if (address)
            {
                a = "a";
            }
            if (id.Definition.IsGroup && !id.Definition.IsAnonymous)
            {
                //TODO: Make this work
            	//Console.WriteLine("EmitLoadField Groups/Records not supported yet");
    			
                r += "        " + ILAddress(1) + "pop\n";
                
                //Build the format string...
    			List<DataDescription> ddes = ListElements(id.Definition);
    			int obj = 0;
    			string fmt = "";
    			foreach (DataDescription dd in ddes)
    			{
                	fmt += "{" + obj + ",-" + dd.Size + "}";
		            obj++;
    			}
    			//Load the format string...
                r += "        " + ILAddress(5);
                r += "ldstr \"" + fmt + "\"\n";
    			Console.WriteLine("fmt = "+fmt);
                
                //Create an array
                r += "        " + ILAddress(5);
                r += "ldc.i4 " + ddes.Count + "\n";
                r += "        " + ILAddress(5);
                r += "newarr [mscorlib]System.Object\n";
    
                //Load elements into array
    			obj = 0;
    			foreach (DataDescription dd in ddes)
    			{
    				Identifier tmp = new Identifier();
    				tmp.Name = dd.Name;
    				tmp.Definition = dd;
		            r += "        " + ILAddress(1) + "dup\n";
		            r += "        " + ILAddress(5) + "ldc.i4 "+obj+"\n";
		            r += EmitLoadSource(tmp as Source, false, true);
		            r += "        " + ILAddress(1) + "stelem.ref\n";
                    obj++;		            
    			}
	            r += "        " + ILAddress(5) + "call string string::Format(string,object[])\n";
            }else{
	            string typ = null;
	            try{
		            if (id.Definition.IsClass)
		            {
		            	typ = ILType(id.Definition.ClassDefinition.NetName.Value);
		            }
		            else
		            {
		            	typ = ILType(id.Definition.Type);
		            }
	            }
	            catch(CompilerException e)
	            {
	            	Console.WriteLine("Variable: "+id.Name);
	            	throw e;
	            }
	            r += "        " + ILAddress(5);
	            if (className==null)
	            	className = "__CobolProgram";
	            r += "ldfld" + a + " " + typ + " "+className+"::" + ILIdentifier(id.Name);
	            r += "\n";
	            if (boxIfPossible)
	            {
	                if (typ == "int32")
	                {
	                    r += "        " + ILAddress(5);
	                    r += "box [mscorlib]System.Int32\n";
	                }
	            }
            }
            return r;
        }

        private string EmitStringVerb(StringVerb stringverb)
        {
            Source source;
            string r = "";
            //Console.WriteLine("STRING at line "+stringverb.LineNumber);
            _stringVerbUsed = true;

            if (stringverb.Sources.Count == 1)
            {
                r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to this
                source = stringverb.Sources[0] as Source;
                r += EmitLoadSource(source, false, true);
            }
            else
            {
                //Build one string from all sources and use that when storing

                //If there's more than one source...
                // ldstr ""
                // stloc.1
                //Then for each source...
                // ldloc.1
                // load source
                // call string string::Concat(string, string)
                // stloc.1
                //Then loc.1 will contain the combined source string
                r += "        " + ILAddress(5) + "ldstr \"\"\n";
                r += "        " + ILAddress(1) + "stloc.1\n";
                foreach (Source src in stringverb.Sources)
                {
                    r += "        " + ILAddress(1) + "ldloc.1\n";
                    r += EmitLoadSource(src, false, true);
                    r += "        " + ILAddress(5) + "call string string::Concat(string, string)\n";
                    r += "        " + ILAddress(1) + "stloc.1\n";
                }
                r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to this
                r += "        " + ILAddress(1) + "ldloc.1\n";
            }

            //TODO: If its possible to miss out Delimited, then we should pass in NULL
            if (stringverb.Delimited == null)
            {
                //TODO: Should we default to single space delmitation?
                //TODO: NULL
                //r += "        " + ILAddress(5) + "ldstr \" \"\n";
                r += "        " + ILAddress(5) + "ldnull\n";
            }
            else
            {
                r += EmitLoadSource(stringverb.Delimited, false, true);
                //Console.WriteLine("DELIMITED = '"+stringverb.Delimited+"'");
            }

			//Console.WriteLine("LOADING INTO "+stringverb.IntoIdentifier.Name);
			//If this crashes, parser hasn't parsed string sources properly
            r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to this
            r += EmitLoadField(stringverb.IntoIdentifier, true, true); //load pointer to target

            if (stringverb.IntoIdentifier.SubstringLeft != null)
            {
                r += EmitExpression(stringverb.IntoIdentifier.SubstringLeft);
            }
            else
            {
                r += "        " + ILAddress(2) + "ldc.i4.s 0xFFFFFFFF\n"; //-1 = no offset
            }
            //TODO:
            //If the INTO identifier has an offset, load it instead of -1
            //Replacing the above with the offset specified in brackets.
            //I think INTO will have to be parsed as a Source object.
            //So that its offset is parsed.
            //ALTERNATIVELY, maybe there can't be an offset here :-)
            //That is probably meant to be in MOVE, not STRING.


            //
            //CobolString has one more param (out int ptr) which isn't use here yet.
            //Need to get its value and save to the 'pointer' identifier specfied in the StringVerb object

            // .Pointer nees to be an identifier
            //TODO: If .pointer doesn't exist (null) then this breaks. Need to make a temp var to pass in.
            if (stringverb.Pointer == null)
            {
                r += "        " + ILAddress(2); //This was 5
                r += "ldloca.s __cobolIntTemp\n";  //TODO: THis needs to load the aress of the local variable
            }
            else
            {
                r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to this
                r += EmitLoadField(stringverb.Pointer, true, false);
            }

            r += "        " + ILAddress(5) + "call instance void class __CobolProgram::__CobolString(string, string, [out] string&, int32, [out] int32&)\n";

            return r;
        }

        private string EmitPerformVerb(PerformVerb perform)
        {
            string r = "";
            string initialValue;
            string beginAddr = "";
            string inner = "";
            if (perform.CallsParagraph)
            {
                if (perform.Until!=null)
                {
                	//r+="//beginning perform until\n";
                    r += "        " + ILAddress(5); //1 for br, and 4 for the address
                    r += "br ";
                    beginAddr = ILAddress();
                }
                string targetType = "void";
                inner += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
                inner += "        " + ILAddress(5) + "call instance " + targetType + " class __CobolProgram::" + ILIdentifier(perform.ProcedureIdentifier.Name) + "()\n";
                if (perform.Until!=null)
                {
                    r += ILAddress() + "\n";
                    r += inner;
                    r += EmitLoopCondition(perform, beginAddr);
                	//r+="//ended perform until\n";
                }else{
                	r+=inner;
                }
            }
            else
            {
                if (perform.IsLoop)
                {
                	if (perform.Varying!=null)
                	{
	                    r += "        " + ILAddress(1);
	                    r += "ldarg.0\n";
	                    if (perform.Varying.From.IsLiteral)
	                    {
	                        Literal literal = perform.Varying.From as Literal;
	                        initialValue = String.Format("0x{0:x8}", Int32.Parse(literal.Value));
	                        r += "        " + ILAddress(2);
	                        r += "ldc.i4.s " + initialValue + "\n";
	                    }
	                    else
	                    {
	                        //TODO: Non-literal
	                        Console.WriteLine("TODO: PERFORM VARYING with non-literal");
	                    }
	                    r += "        " + ILAddress(5);
	                    r += "stfld int32 __CobolProgram::" + ILIdentifier(perform.Varying.Counter.Name) + "\n";
	                    r += EmitIncrementer(perform.Varying, true);
                	}
                    r += "        " + ILAddress(5); //1 for br, and 4 for the address
                    r += "br ";
                    beginAddr = ILAddress();
                    //inner += EmitIncrementer(perform.Varying);
                }

                foreach (Sentence sentence in perform.Sentences)
                {
                    inner += EmitSentence(sentence);
                }

                if (perform.IsLoop)
                {
                    r += ILAddress() + "\n";
                    r += inner;
                    if (perform.Varying!=null)
                    {
	                    r += EmitIncrementer(perform.Varying);
                    }
                    r += EmitLoopCondition(perform, beginAddr);
                }
                else
                {
                    //No loop
                    r += inner;
                }
            }
            return r;
        }

        private string EmitIncrementer(PerformVaryingPhrase varying)
        {
            return EmitIncrementer(varying, false);
        }

        private string EmitIncrementer(PerformVaryingPhrase varying, bool backwards)
        {
            string inner = "";
            inner = "";
            inner += "        " + ILAddress(1);
            inner += "ldarg.0\n";
            inner += "        " + ILAddress(1);
            inner += "dup\n";
            inner += "        " + ILAddress(5);
            inner += "ldfld int32 __CobolProgram::" + ILIdentifier(varying.Counter.Name) + "\n";
            if (varying.By.IsLiteral)
            {
                inner += EmitLiteral(varying.By as Literal);
            }
            else
            {
                inner += EmitIdentifier(varying.By as Identifier);
            }
            inner += "        " + ILAddress(1);
            if (backwards)
            {
                inner += "sub\n";
            }
            else
            {
                inner += "add\n";
            }
            inner += "        " + ILAddress(5);
            inner += "stfld int32 __CobolProgram::" + ILIdentifier(varying.Counter.Name) + "\n";
            return inner;
        }

        private string EmitIfStatement(IfStatement ifstatement)
        {
            //Idea:
            //  IL0 : Jump to IL5                //r1
            //
            //  IL1 : perform content of "THEN"  //r2
            //  IL2 : Jump to IL8
            //
            //  IL3 : perform content of "ELSE"  //r3
            //  IL4 : Jump to IL8
            //
            //  IL5 : Evaluate condition         //r4
            //  IL6 : If true, jump to IL1
            //  IL7 : Jump to IL3
            //
            //  IL8 : nop                        //r5
            //
            string r = "";

            string r1 = "";
            r1 += "        " + ILAddress(5); //1 for br, and 4 for the address
            r1 += "br "; //TODO: Add addr later
            //beginAddr = ILAddress();

            string r2 = "";
            string il_1 = ILAddress();
            foreach (Sentence sentence in ifstatement.Then)
            {
                r2 += EmitSentence(sentence);
            }
            r2 += "        " + ILAddress(5); //1 for br, and 4 for the address
            r2 += "br "; //TODO: Add addr later

            string il_3 = ILAddress();
            string r3 = "";
            if (ifstatement.Else.Count != 0)
            {
                foreach (Sentence sentence in ifstatement.Else)
                {
                    r3 += EmitSentence(sentence);
                }
                r3 += "        " + ILAddress(5); //1 for br, and 4 for the address
                r3 += "br "; //TODO: Add addr later
            }

            string r4 = "";
            r1 += ILAddress() + "\n";
            r4 += EmitIfCondition(ifstatement, il_1, il_3);
            r2 += ILAddress() + "\n";
            if (r3 != "")
            {
                r3 += ILAddress() + "\n";
            }

            r = r1 + r2 + r3 + r4;
            return r;
        }

        private string EmitIfCondition(IfStatement ifstatement, string il_1, string il_3)
        {
            //Plan (part of the above plan):
            //  IL5 : Evaluate condition         //r4
            //  IL6 : If true, jump to IL1
            //  IL7 : Jump to IL3
            string r = "";
            if (ifstatement.Else.Count > 0)
            {
                r += EmitCondition(ifstatement.Condition, il_1, il_3);
            }
            else
            {
                r += EmitCondition(ifstatement.Condition, il_1, null);
            }
            //TODO: Implementation of EmitCondition
            //  Should there be a generic EmitCondition that
            //  can be used by this and EmitLoopCOndition?
            return r;
        }

        private string EmitLoopBooleanCondition(Condition condition, string startAddress)
        {
        	string r = "";
			//TODO: This needs integrated with EmitCondition
			Source source = condition.BooleanValue;
			if (source.GetType()==typeof(Identifier))
			{
				Identifier id = source as Identifier;
				DataDescription dde = new DataDescription();
				dde.Type = DataType.Boolean;
				dde.Name = _level88prefix + id.Name;
				id.Name = dde.Name;
				id.Definition = dde;
                r += "        ";
                r += ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
				r += EmitLoadField(id, false, false);
	            r += "        " + ILAddress(5);
	            r += "brfalse " + startAddress + "\n";
			}
			else
			{
				 throw new Compiler.Exceptions.CompilerException("Literal booleans in loop conditions not supported yet");
			}
			
        	return r;
        }
        
        private string EmitLoopStringComparisonCondition(RelationCondition condition, string startAddress)
        {
        	string r = "";

            Source left = condition.LeftExpression.TimesDiv.Power.Basis.Source;
            Source right = condition.RightExpression.TimesDiv.Power.Basis.Source;
            
            if (left.GetType() == typeof(FigurativeConstant) ||
                right.GetType() == typeof(FigurativeConstant))
            {
                //Work out which side is the figurative constant...
                Source other;
                FigurativeConstant constant;
                if (left.GetType() == typeof(FigurativeConstant)){
                    constant = left as FigurativeConstant;
                    other = right;
                }else{
                    constant = right as FigurativeConstant;
                    other = left;
                }
                
                if (constant.Type == FigurativeConstantType.Spaces)
                {
                    Identifier id = other as Identifier;
                    if (id == null){
                        Console.WriteLine("Identifier expected when comparing to figurative constant");
                    }
                    r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
                    r += "        " + ILAddress(5) + "ldfld bool __CobolProgram::_hasData_"+ILIdentifier(id.Definition.Name)+"\n";
                    r += "        " + ILAddress(5) + "brtrue " + startAddress + "\n";
                }else{
                    Console.WriteLine("Other figurative constants not supported yet");
                }
            }
            else
            {
                r += EmitLoadSource(left, false, false);
                r += EmitLoadSource(right, false, false);
                if (condition.RelationalOperator.Relation == RelationType.EqualTo)
                {
                    //Stop looping when left = right
                    r += "        " + ILAddress(2);
                    r += "ceq\n";
                    r += "        " + ILAddress(1);
                    r += "ldc.i4.0\n";
                    r += "        " + ILAddress(2);
                    r += "ceq\n";
                    r += "        " + ILAddress(1);
                    r += "stloc.0\n";
                    r += "        " + ILAddress(1);
                    r += "ldloc.0\n";
                    r += "        " + ILAddress(5);
                    r += "brtrue " + startAddress + "\n";
                }else{
                    Console.WriteLine("Other relation types not supported yet");
                }
            }
            return r;
        }
        
        private string EmitLoopRelationCondition(RelationCondition condition, string startAddress)
        {
        	string r = "";
            r += EmitExpression(condition.LeftExpression);
            r += EmitExpression(condition.RightExpression);

            if (condition.RelationalOperator.Relation == RelationType.EqualTo)
            {
                //Stop looping when left = right
                r += "        " + ILAddress(2);
                r += "ceq\n";
                r += "        " + ILAddress(1);
                r += "ldc.i4.0\n";
                r += "        " + ILAddress(2);
                r += "ceq\n";
                r += "        " + ILAddress(1);
                r += "stloc.0\n";
                r += "        " + ILAddress(1);
                r += "ldloc.0\n";
                r += "        " + ILAddress(5);
                r += "brtrue " + startAddress + "\n";
            }
            else if (condition.RelationalOperator.Relation == RelationType.GreaterThan)
            {
                //Stop looping when left > right
                r += "        " + ILAddress(5); //1 for ble, and 4 for the address
                r += "ble.un " + startAddress + "\n";
            }
            else if (condition.RelationalOperator.Relation == RelationType.LessThan)
            {
                //Stop looping when left < right
                r += "        " + ILAddress(5); //1 for bge, and 4 for the address
                r += "bge.un " + startAddress + "\n";
            }
            else
            {
                //TODO:
                Console.WriteLine("Other relation types not supported yet");
                //Some CIL Branch instructions:
                //3.5              beq.<length>  branch on equal 32
                //3.6              bge.<length>  branch on greater than or equal to  33
                //3.7              bge.un.<length>  branch on greater than or equal to, unsigned or unordered  34
                //3.8              bgt.<length>  branch on greater than  35
                //3.9              bgt.un.<length>  branch on greater than, unsigned or unordered  36
                //3.10             ble.<length>  branch on less than or equal to  37
                //3.11             ble.un.<length>  branch on less than or equal to, unsigned or unordered  38
                //3.12             blt.<length>  branch on less than  39
                //3.13             blt.un.<length>  branch on less than, unsigned or unordered  40
                //3.14             bne.un<length>  branch on not equal or unordered  41
                //3.15             br.<length>  unconditional branch
            }
            //====================================================================
        	return r;
        }
        
        private string EmitLoopCondition(PerformVerb perform, string startAddress)
        {
            //TODO: Replace most of this with a call to EmitCondition
            string r = "";
            if (perform.Varying!=null)
            {
	            if (perform.Varying.Condition.GetType() == typeof(RelationCondition))
	            {
	                RelationCondition condition = perform.Varying.Condition as RelationCondition;
	                try{
	                    r+=EmitLoopRelationCondition(condition, startAddress);
	                }
	                catch(BasisException)
	                {
	                    r+=EmitLoopStringComparisonCondition(condition, startAddress);
	                }
	            }
	            else
	            {
	                //TODO:
	                Console.WriteLine("Other condition types not supported yet");
	            }
            }
            else if (perform.Until!=null)
           	{
           		if (perform.Until.IsBoolean)
           		{
		            r+=EmitLoopBooleanCondition(perform.Until, startAddress);
           		}
           		else
           		{
	           		RelationCondition condition = perform.Until as RelationCondition;
	           		try{
		                r+=EmitLoopRelationCondition(condition, startAddress);
	           		}
	           		catch(BasisException)
	           		{
                        r+=EmitLoopStringComparisonCondition(condition, startAddress);
	           		}
           		}
           	}
           	else
           	{
	                //TODO: Exception
	                Console.WriteLine("Error in EmitLoopCondition");
            }
            return r;
        }

        private string EmitIdentifier(Identifier id)
        {
            string r = "";
            r += "        ";
            r += ILAddress(1);
            r += "ldarg.0"; //Hidden pointer to 'this'
            r += "\n";
            //TODO: Contextual analysis should set the type of the source here
            r += EmitLoadField(id, false, false);
            return r;
        }

		private string EmitLiteral(Literal literal, bool boxIfPossible)
		{
            //Console.WriteLine("Type = "+literal.Type);
            string r = "";
            if (literal.Type == VariableType.String)
            {
                r += "        ";
                r += ILAddress(5);
                r += "ldstr \"" + literal.Value + "\"";
                r += "\n";
            }
            else if (literal.Type == VariableType.Integer)
            {
                string v;
                if (literal.Value == null)
                {
                    v = String.Format("0x{0:x8}", 0);
                }
                else
                {
                    v = String.Format("0x{0:x8}", Int32.Parse(literal.Value));
                }
                r += "        " + ILAddress(5);
                r += "ldc.i4 " + v + "\n";
              	if (boxIfPossible){
                   r += "        " + ILAddress(5);
                   r += "box [mscorlib]System.Int32\n";
              	}
            }
            else if (literal.Type == VariableType.Boolean)
            {
            	//TODO: Emit boolean
                r += "        ";
                r += ILAddress(1);
                if (literal.BooleanValue==true)
                {
	                r += "ldc.i4.1\n";
                }else{
	                r += "ldc.i4.0\n";
                }
            }
            else
            {
                //TODO: Implement floats, etc
                Console.WriteLine("Only booleans, integers and strings currently supported");
                throw new CompilerException("");
            }
            return r;
		}
        private string EmitLiteral(Literal literal)
        {
        	return EmitLiteral(literal,false);
        }
        
        private string EmitBasis(Basis basis)
        {
            //TODO: Variable Types
            //( identifier | literal | "(" arithmetic-expression ")" ) .
            string r = "";
            if (basis.Source != null)
            {
                if (basis.Source.GetType() == typeof(Literal))
                {
                    Literal literal = basis.Source as Literal;
                    r += EmitLiteral(literal);
                }
                else if (basis.Source.GetType() == typeof(Identifier))
                {
                    Identifier id = basis.Source as Identifier;
                    if (id.Definition.Type == DataType.String)
                    {
                        throw new BasisException();
                    }
                    r += EmitIdentifier(id);
                }
                else
                {
					throw new Compiler.Exceptions.CompilerException("Literal or Identifier expected");
					//TODO: Add a line number to this exception                    
                }
            }
            else if (basis.Expression != null)
            {
                r += EmitExpression(basis.Expression);
            }
            else
            {
				throw new Compiler.Exceptions.CompilerException("Error parsing expression");
                //TODO: Find this error in the parser and report a line number
            }
            return r;
        }

        private string EmitPowerOf()
        {
            string r = "";
            //TODO: Emit "to the power of" instruction
            return r;
        }

        private string EmitPower(Power power)
        {
            //[ ( "+" | "-" ) ] basis { "**" basis }* .
            string r = "";
            r += EmitBasis(power.Basis);
            if (power.Basises != null)
            {
                foreach (Basis b in power.Basises)
                {
                    r += EmitBasis(b);
                    r += EmitPowerOf();
                }
            }
            if (power.Sign == PowerSign.Add)
            {
                r += "        " + ILAddress(1);
                r += "add\n";
            }
            else if (power.Sign == PowerSign.Subtract)
            {
                r += "        " + ILAddress(1);
                r += "sub\n";
            }
            else
            {
                //TODO
                //Console.WriteLine("Invalid Power");
            }
            return r;
        }

        private string EmitTimesDiv(TimesDiv td)
        {
            string r = "";
            Power p = td.Power;
            r += EmitPower(p);
            while ((td = td.Next) != null)
            {
                p = td.Power;
                r += EmitPower(p);
                if (td.Sign == TimesDivSign.Multiply)
                {
                    r += "        " + ILAddress(1);
                    r += "mul\n";
                }
                else if (td.Sign == TimesDivSign.Divide)
                {
                    r += "        " + ILAddress(1);
                    r += "div\n";
                }
                else
                {
                    //TODO
                    Console.WriteLine("Invalid ArithmeticExpression");
                }
            }
            return r;
        }

        private string EmitExpression(ArithmeticExpression expression)
        {
            string r = "";
            TimesDiv td = expression.TimesDiv;
            r += EmitTimesDiv(td);
            while ((expression = expression.Next) != null)
            {
                td = expression.TimesDiv;
                r += EmitTimesDiv(td);
                if (expression.Sign == ExpressionSign.Add)
                {
                    r += "        " + ILAddress(1);
                    r += "add\n";
                }
                else if (expression.Sign == ExpressionSign.Subtract)
                {
                    r += "        " + ILAddress(1);
                    r += "sub\n";
                }
                else
                {
                    //Error
                    Console.WriteLine("Invalid ArithmeticExpression");
                }
            }
            //Console.WriteLine("expr: "+r);
            return r;
        }

        private string EmitStringVerbMethod()
        {
        	string r = "";
            r+="    .method private hidebysig \n";
            r+="           instance default void __CobolString (string from, string delimiter, string& 'to', int32 offset, [out] int32& ptr)  cil managed \n";
            r+="    {\n";
            r+="        // Method begins at RVA 0x2164\n";
            r+="	// Code size 365 (0x16d)\n";
            r+="	.maxstack 4\n";
            r+="	.locals init (\n";
            r+="		int32	V_0,\n";
            r+="		string	V_1,\n";
            r+="		int32	V_2,\n";
            r+="		string	V_3,\n";
            r+="		string	V_4,\n";
            r+="		bool	V_5)\n";
            r+="	IL_0000:  nop \n";
            r+="	IL_0001:  ldarg.2 \n";
            r+="	IL_0002:  ldnull \n";
            r+="	IL_0003:  ceq \n";
            r+="	IL_0005:  ldc.i4.0 \n";
            r+="	IL_0006:  ceq \n";
            r+="	IL_0008:  stloc.s 5\n";
            r+="	IL_000a:  ldloc.s 5\n";
            r+="	IL_000c:  brtrue.s IL_0012\n";
            r+="\n";
            r+="	IL_000e:  ldarg.1 \n";
            r+="	IL_000f:  stloc.1 \n";
            r+="	IL_0010:  br.s IL_0036\n";
            r+="\n";
            r+="	IL_0012:  nop \n";
            r+="	IL_0013:  ldarg.1 \n";
            r+="	IL_0014:  ldarg.2 \n";
            r+="	IL_0015:  callvirt instance int32 string::IndexOf(string)\n";
            r+="	IL_001a:  stloc.2 \n";
            r+="	IL_001b:  ldloc.2 \n";
            r+="	IL_001c:  ldc.i4.m1 \n";
            r+="	IL_001d:  ceq \n";
            r+="	IL_001f:  ldc.i4.0 \n";
            r+="	IL_0020:  ceq \n";
            r+="	IL_0022:  stloc.s 5\n";
            r+="	IL_0024:  ldloc.s 5\n";
            r+="	IL_0026:  brtrue.s IL_002c\n";
            r+="\n";
            r+="	IL_0028:  ldarg.1 \n";
            r+="	IL_0029:  stloc.1 \n";
            r+="	IL_002a:  br.s IL_0035\n";
            r+="\n";
            r+="	IL_002c:  ldarg.1 \n";
            r+="	IL_002d:  ldc.i4.0 \n";
            r+="	IL_002e:  ldloc.2 \n";
            r+="	IL_002f:  callvirt instance string string::Substring(int32, int32)\n";
            r+="	IL_0034:  stloc.1 \n";
            r+="	IL_0035:  nop \n";
            r+="	IL_0036:  ldarg.s 4\n";
            r+="	IL_0038:  ldc.i4.m1 \n";
            r+="	IL_0039:  ceq \n";
            r+="	IL_003b:  stloc.s 5\n";
            r+="	IL_003d:  ldloc.s 5\n";
            r+="	IL_003f:  brtrue IL_015b\n";
            r+="\n";
            r+="	IL_0044:  nop \n";
            r+="	IL_0045:  ldarg.s 4\n";
            r+="	IL_0047:  ldc.i4.1 \n";
            r+="	IL_0048:  sub \n";
            r+="	IL_0049:  starg.s 4\n";
            r+="	IL_004b:  ldarg.s 4\n";
            r+="	IL_004d:  ldarg.3 \n";
            r+="	IL_004e:  ldind.ref \n";
            r+="	IL_004f:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0054:  cgt \n";
            r+="	IL_0056:  ldc.i4.0 \n";
            r+="	IL_0057:  ceq \n";
            r+="	IL_0059:  stloc.s 5\n";
            r+="	IL_005b:  ldloc.s 5\n";
            r+="	IL_005d:  brtrue.s IL_009d\n";
            r+="\n";
            r+="	IL_005f:  nop \n";
            r+="	IL_0060:  ldarg.s 4\n";
            r+="	IL_0062:  ldarg.3 \n";
            r+="	IL_0063:  ldind.ref \n";
            r+="	IL_0064:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0069:  sub \n";
            r+="	IL_006a:  ldarg.1 \n";
            r+="	IL_006b:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0070:  add \n";
            r+="	IL_0071:  ldarg.3 \n";
            r+="	IL_0072:  ldind.ref \n";
            r+="	IL_0073:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0078:  sub \n";
            r+="	IL_0079:  stloc.0 \n";
            r+="	IL_007a:  br.s IL_0090\n";
            r+="\n";
            r+="	IL_007c:  nop \n";
            r+="	IL_007d:  ldarg.3 \n";
            r+="	IL_007e:  dup \n";
            r+="	IL_007f:  ldind.ref \n";
            r+="	IL_0080:  ldstr \" \"\n";
            r+="	IL_0085:  call string string::Concat(string, string)\n";
            r+="	IL_008a:  stind.ref \n";
            r+="	IL_008b:  nop \n";
            r+="	IL_008c:  ldloc.0 \n";
            r+="	IL_008d:  ldc.i4.1 \n";
            r+="	IL_008e:  sub \n";
            r+="	IL_008f:  stloc.0 \n";
            r+="	IL_0090:  ldloc.0 \n";
            r+="	IL_0091:  ldc.i4.0 \n";
            r+="	IL_0092:  cgt \n";
            r+="	IL_0094:  stloc.s 5\n";
            r+="	IL_0096:  ldloc.s 5\n";
            r+="	IL_0098:  brtrue.s IL_007c\n";
            r+="\n";
            r+="	IL_009a:  nop \n";
            r+="	IL_009b:  br.s IL_00ec\n";
            r+="\n";
            r+="	IL_009d:  ldarg.s 4\n";
            r+="	IL_009f:  ldarg.1 \n";
            r+="	IL_00a0:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_00a5:  add \n";
            r+="	IL_00a6:  ldarg.3 \n";
            r+="	IL_00a7:  ldind.ref \n";
            r+="	IL_00a8:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_00ad:  cgt \n";
            r+="	IL_00af:  ldc.i4.0 \n";
            r+="	IL_00b0:  ceq \n";
            r+="	IL_00b2:  stloc.s 5\n";
            r+="	IL_00b4:  ldloc.s 5\n";
            r+="	IL_00b6:  brtrue.s IL_00ec\n";
            r+="\n";
            r+="	IL_00b8:  nop \n";
            r+="	IL_00b9:  ldarg.s 4\n";
            r+="	IL_00bb:  ldarg.1 \n";
            r+="	IL_00bc:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_00c1:  add \n";
            r+="	IL_00c2:  ldarg.3 \n";
            r+="	IL_00c3:  ldind.ref \n";
            r+="	IL_00c4:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_00c9:  sub \n";
            r+="	IL_00ca:  stloc.0 \n";
            r+="	IL_00cb:  br.s IL_00e1\n";
            r+="\n";
            r+="	IL_00cd:  nop \n";
            r+="	IL_00ce:  ldarg.3 \n";
            r+="	IL_00cf:  dup \n";
            r+="	IL_00d0:  ldind.ref \n";
            r+="	IL_00d1:  ldstr \" \"\n";
            r+="	IL_00d6:  call string string::Concat(string, string)\n";
            r+="	IL_00db:  stind.ref \n";
            r+="	IL_00dc:  nop \n";
            r+="	IL_00dd:  ldloc.0 \n";
            r+="	IL_00de:  ldc.i4.1 \n";
            r+="	IL_00df:  sub \n";
            r+="	IL_00e0:  stloc.0 \n";
            r+="	IL_00e1:  ldloc.0 \n";
            r+="	IL_00e2:  ldc.i4.0 \n";
            r+="	IL_00e3:  cgt \n";
            r+="	IL_00e5:  stloc.s 5\n";
            r+="	IL_00e7:  ldloc.s 5\n";
            r+="	IL_00e9:  brtrue.s IL_00cd\n";
            r+="\n";
            r+="	IL_00eb:  nop \n";
            r+="	IL_00ec:  ldnull \n";
            r+="	IL_00ed:  stloc.3 \n";
            r+="	IL_00ee:  ldarg.3 \n";
            r+="	IL_00ef:  ldind.ref \n";
            r+="	IL_00f0:  ldc.i4.0 \n";
            r+="	IL_00f1:  ldarg.s 4\n";
            r+="	IL_00f3:  callvirt instance string string::Substring(int32, int32)\n";
            r+="	IL_00f8:  stloc.s 4\n";
            r+="	IL_00fa:  ldarg.s 4\n";
            r+="	IL_00fc:  ldloc.1 \n";
            r+="	IL_00fd:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0102:  add \n";
            r+="	IL_0103:  ldarg.3 \n";
            r+="	IL_0104:  ldind.ref \n";
            r+="	IL_0105:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_010a:  clt \n";
            r+="	IL_010c:  stloc.s 5\n";
            r+="	IL_010e:  ldloc.s 5\n";
            r+="	IL_0110:  brtrue.s IL_012c\n";
            r+="\n";
            r+="	IL_0112:  nop \n";
            r+="	IL_0113:  ldarg.3 \n";
            r+="	IL_0114:  ldloc.s 4\n";
            r+="	IL_0116:  ldloc.1 \n";
            r+="	IL_0117:  call string string::Concat(string, string)\n";
            r+="	IL_011c:  stind.ref \n";
            r+="	IL_011d:  ldarg.s 5\n";
            r+="	IL_011f:  ldarg.3 \n";
            r+="	IL_0120:  ldind.ref \n";
            r+="	IL_0121:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0126:  ldc.i4.1 \n";
            r+="	IL_0127:  add \n";
            r+="	IL_0128:  stind.i4 \n";
            r+="	IL_0129:  nop \n";
            r+="	IL_012a:  br.s IL_0158\n";
            r+="\n";
            r+="	IL_012c:  nop \n";
            r+="	IL_012d:  ldarg.3 \n";
            r+="	IL_012e:  ldind.ref \n";
            r+="	IL_012f:  ldarg.s 4\n";
            r+="	IL_0131:  ldloc.1 \n";
            r+="	IL_0132:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0137:  add \n";
            r+="	IL_0138:  callvirt instance string string::Substring(int32)\n";
            r+="	IL_013d:  stloc.3 \n";
            r+="	IL_013e:  ldarg.3 \n";
            r+="	IL_013f:  ldloc.s 4\n";
            r+="	IL_0141:  ldloc.1 \n";
            r+="	IL_0142:  ldloc.3 \n";
            r+="	IL_0143:  call string string::Concat(string, string, string)\n";
            r+="	IL_0148:  stind.ref \n";
            r+="	IL_0149:  ldarg.s 5\n";
            r+="	IL_014b:  ldarg.s 4\n";
            r+="	IL_014d:  ldloc.1 \n";
            r+="	IL_014e:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0153:  add \n";
            r+="	IL_0154:  ldc.i4.1 \n";
            r+="	IL_0155:  add \n";
            r+="	IL_0156:  stind.i4 \n";
            r+="	IL_0157:  nop \n";
            r+="	IL_0158:  nop \n";
            r+="	IL_0159:  br.s IL_016c\n";
            r+="\n";
            r+="	IL_015b:  nop \n";
            r+="	IL_015c:  ldarg.3 \n";
            r+="	IL_015d:  ldloc.1 \n";
            r+="	IL_015e:  stind.ref \n";
            r+="	IL_015f:  ldarg.s 5\n";
            r+="	IL_0161:  ldarg.3 \n";
            r+="	IL_0162:  ldind.ref \n";
            r+="	IL_0163:  callvirt instance int32 string::get_Length()\n";
            r+="	IL_0168:  ldc.i4.1 \n";
            r+="	IL_0169:  add \n";
            r+="	IL_016a:  stind.i4 \n";
            r+="	IL_016b:  nop \n";
            r+="	IL_016c:  ret \n";
            r+="    } // end of method Test::__CobolString\n";
        	return r;
        }
    }
}

