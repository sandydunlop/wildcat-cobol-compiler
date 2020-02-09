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
using System.Collections;
using System.Collections.Generic;
using Wildcat.Cobol.Compiler.References;
using Wildcat.Cobol.Compiler.Structure;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler.ILGenerator
{
    public partial class Generator
    {
    	private FileAndSortDescriptionEntry GetFSDE(string name)
    	{
    		if (_program.Data!=null && _program.Data.FileSection!=null)
    		{
    			foreach (FileAndSortDescriptionEntry fsde in _program.Data.FileSection.Entries)
    			{
    				if (fsde.Name.Name == name)
    				{
    					return fsde;
    				}
    			}
    		}
    		return null;
    	}
    	
    	private DataDescription GetDDEByName(string name)
    	{
    	   return _program.VariableDeclarations[name] as DataDescription;
    	}
    	
        private string EmitOpenStatement(OpenStatement openStatement)
        {
        	string r = "";
		    string className = "__CobolProgram";
        	//Console.WriteLine("Emitting Open Statement");
        	//fsde.FileControlEntry;
        	foreach (string fd in openStatement.Files)
        	{
        		FileAndSortDescriptionEntry fsde = GetFSDE(fd);
        		//Console.WriteLine("FD = "+fd);
        		
        		//Pointer to this...
	            r += "        " + ILAddress(1) + "ldarg.0\n";
	            
	            //TODO: Load filename
	            r+=EmitLoadSource(fsde.FileControlEntry.Assign.Source, false, false);

				if (openStatement.IsInput)
				{
		            //Open for read...
		            r += "        " + ILAddress(5);
		            r += "newobj instance void class [mscorlib]System.IO.StreamReader::.ctor(string)\n";
		            r += "        " + ILAddress(5);
		            r += "stfld class [mscorlib]System.IO.StreamReader "+className+"::_reader_"+ILIdentifier(fd)+"\n";
				}else{
		            //Open for write...
		            r += "        " + ILAddress(5);
		            r += "newobj instance void class [mscorlib]System.IO.StreamWriter::.ctor(string)\n";
		            r += "        " + ILAddress(5);
		            r += "stfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(fd)+"\n";
				}
        	}
        	return r;
        }
        
        private string EmitCloseStatement(CloseStatement closeStatement)
        {
        	string r = "";
		    string className = "__CobolProgram";
        	//Console.WriteLine("Emitting Close Statement");
        	foreach (string fd in closeStatement.Files)
        	{
        		FileAndSortDescriptionEntry fsde = GetFSDE(fd);
        		
        		//Work out if it's read or write mode
	            r += "        " + ILAddress(1) + "ldarg.0\n";
	            r += "        " + ILAddress(5);
		        r += "ldfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(fd)+"\n";
        		r += "        " + ILAddress(1) + "ldnull\n";
        		r += "        " + ILAddress(5) + "bne.un ";
        		
        		//Read mode...
        		string r2 = "";
	            r2 += "        " + ILAddress(1) + "ldarg.0\n";
	            r2 += "        " + ILAddress(5);
		        r2 += "ldfld class [mscorlib]System.IO.StreamReader "+className+"::_reader_"+ILIdentifier(fd)+"\n";
	            r2 += "        " + ILAddress(5);
	            r2 += "callvirt instance void class [mscorlib]System.IO.StreamReader::Close()\n";
	            r2 += "        " + ILAddress(1) + "ldarg.0\n";
	            r2 += "        " + ILAddress(1) + "ldnull\n";
	            r2 += "        " + ILAddress(5);
	            r2 += "stfld class [mscorlib]System.IO.StreamReader "+className+"::_reader_"+ILIdentifier(fd)+"\n";
	            r2 += "        " + ILAddress(5) + "br ";

				//Write mode...	
				r += ILAddress() + "\n";            
	            string r3 = "";
	            r3 += "        " + ILAddress(1) + "ldarg.0\n";
	            r3 += "        " + ILAddress(5);
		        r3 += "ldfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(fd)+"\n";
	            r3 += "        " + ILAddress(5);
	            r3 += "callvirt instance void class [mscorlib]System.IO.StreamWriter::Close()\n";
	            r3 += "        " + ILAddress(1) + "ldarg.0\n";
	            r3 += "        " + ILAddress(1) + "ldnull\n";
	            r3 += "        " + ILAddress(5);
	            r3 += "stfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(fd)+"\n";
	            
	            r2 += ILAddress() + "\n";
	            r += r2;
	            r += r3;
        	}
        	return r;
        }
        
        private string EmitReadStatement(ReadStatement readStatement)
        {
        	string r = "\n        //Begin read statement...\n";
		    string className = "__CobolProgram";
        	
        	FileAndSortDescriptionEntry fsde = GetFSDE(readStatement.Filename.Name);
        	//fsde.DataDescriptions is a list of items that the data should be read into
        	
        	//Check for End of Stream...
            r += "        " + ILAddress(1) + "ldarg.0\n";
	        r += "        " + ILAddress(5);
            r += "ldfld class [mscorlib]System.IO.StreamReader "+className+"::_reader_"+ILIdentifier(readStatement.Filename.Name)+"\n";
	        r += "        " + ILAddress(5);
            r += "callvirt instance bool class [mscorlib]System.IO.StreamReader::get_EndOfStream()\n";
	        r += "        " + ILAddress(5);
            r += "brtrue "; //Jump to IsAtEndOfStream

			string r2 = "";
			
			//Execute NotEndStatements...
			if (readStatement.NotEndStatements.Count>0)
			{
                foreach (Sentence sentence in readStatement.NotEndStatements)
                {
                    r2 += EmitSentence(sentence);
                }
			}            

			//Read from file...
        	r2 += "        " + ILAddress(1) + "ldarg.0\n";
	        r2 += "        " + ILAddress(5);
            r2 += "ldfld class [mscorlib]System.IO.StreamReader "+className+"::_reader_"+ILIdentifier(readStatement.Filename.Name)+"\n";
	        r2 += "        " + ILAddress(5);
            r2 += "callvirt instance string class [mscorlib]System.IO.StreamReader::ReadLine()\n"; 
            Identifier id = new Identifier();
            id.Definition = fsde.DataDescriptions[0] as DataDescription;
            r2 += EmitStore(id,null);
            //EmitStore calls EmitStoreGroup which needs to loop through the elements of a record.
            //This should be changed to call ListElements like DISPLAY does.
            
            //Jump to EndRead
	        r2 += "        " + ILAddress(5);
            r2 += "br ";

			//If we're at the end of the stream, the first branch jumps to here
			r+=ILAddress()+"\n";            
            
			//Execute NotEndStatements...
			string r3 = "";
			if (readStatement.EndStatements.Count>0)
			{
                foreach (Sentence sentence in readStatement.EndStatements)
                {
                    r3 += EmitSentence(sentence);
                }
			}
			
			//End Read
			//The branch after reading data jumps to here
			r2 += ILAddress() + "\n";   
			r+=r2;
			r+=r3;

            r+="        //End of read statement\n";
        	return r;
        }
        
        private string EmitWriteStatement(WriteStatement writeStatement)
        {
        	string r = "\n        //Begin write statement...\n";
        	//string r = "";
            
            //throw new Compiler.Exceptions.NotImplementedException(
            //        writeStatement.LineNumber, "WRITE statement");
        	
        	//Base this on EmitDisplayVerb rather than on the style that
        	//EmitReadStatement uses. EmitDisplayVerb and this should probably
        	//both call a common lower level method that works out what's to 
        	//be dispayed or written to a file, and have a parameter that states
        	//whether or not it's being written to a file (and which file?) or
        	//if it's going to the screen.

            r+= EmitOutputBase(null, writeStatement);
            r+="        //End of write statement\n";
            return r;
        }
        
        private string EmitDisplayVerb(DisplayVerb display)
        {
            return EmitOutputBase(display, null);
        }
        
        private string EmitOutputBase(DisplayVerb display, WriteStatement writeStatement)
        {
            string r = "";
		    string className = "__CobolProgram";
		    DataDescription dde = null;
            
            //Build the format string and param types list...
            string fmt = "";
            int objects = 0;
            
            ArrayList sources;
            if (display!=null)
            {
                sources = display.Sources;
            }else{
                dde = GetDDEByName(writeStatement.RecordName.Name);
                if (dde==null)
                {
                    Console.WriteLine("dde is NULL");
                }
                
            	r += "        " + ILAddress(1) + "ldarg.0\n";
    	        r += "        " + ILAddress(5);
                r += "ldfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(dde.FSDE.Name.Name)+"\n";
                
                sources = new ArrayList();
                if (writeStatement.RecordName!=null)
                {
                    sources.Add(writeStatement.RecordName);
                }
                //TODO: What should be done when there are no FROM identifiers?
            }
            
            for (int i = 0; i < sources.Count; i++)
            {
            	Source source = sources[i] as Source;
            	if (IsGroup(source))
            	{
            		Identifier id = source as Identifier;
        			List<DataDescription> ddes = ListElements(id.Definition);
        			foreach (DataDescription dd in ddes)
        			{
		                fmt += "{" + objects + ",-" + dd.Size + "}";
        				objects++;
        			}
            	}
            	else
            	{
            		//TODO:
            		//If the source specifies a substring, then that size should be used in the 
            		//brackets rather than the definition's size. Simply using the definition's
            		//size breaks the 99 bottles output because the buffer variable is large
            		//and we only want one character from it.
            		//See testdisp.cbl and its output.

            		if (source.GetType()==typeof(Identifier))
            		{
            			Identifier id = source as Identifier;
            			if (id.UsesSubstring)
            			{
            				Console.WriteLine("UsesSubstring = true : "+id.Name);
		                	fmt += "{" + objects + "}";
            			}
            			else
            			{
            				//Console.WriteLine("UsesSubstring = false : "+id.Name);
		                	fmt += "{" + objects + ",-" + id.Definition.Size + "}";
            			}
            		}
            		else
            		{
		                fmt += "{" + objects + "}";
            		}
	                objects++;
            	}
            }

			//Load the format string...
            r += "        " + ILAddress(5);
            r += "ldstr \"" + fmt + "\"\n";
            
            //Create an array
            r += "        " + ILAddress(5);
            r += "ldc.i4 " + objects + "\n";
            r += "        " + ILAddress(5);
            r += "newarr [mscorlib]System.Object\n";

            //Load the values...
            int obj = 0;
            foreach (Source source in sources)
            {
            	if (IsGroup(source))
            	{
            		Identifier id = source as Identifier;
        			//Plan:
        			// 1. Load a format string based on all of the group element sizes (and types?)
        			// 2. Load each element
        			// 3. String.Format
        			// 4. Build an object array of the elements
        			// 5. Use the format string and array in WriteLine
        			List<DataDescription> ddes = ListElements(id.Definition);
        			
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
            	}
            	else
            	{
		            r += "        " + ILAddress(1) + "dup\n";
		            r += "        " + ILAddress(5) + "ldc.i4 "+obj+"\n";
                	r += EmitLoadSource(source, false, true);
		            r += "        " + ILAddress(1) + "stelem.ref\n";
		            obj++;
            	}
            }
            
            if (display!=null)
            {
                //Call Write or WriteLine...
                r += "        " + ILAddress(5);
                string parms = "string, object[]";
                if (display.NoAdvancing)
                {
                	r += "call void class [mscorlib]System.Console::Write(" + parms + ")";
                }else{
                	r += "call void class [mscorlib]System.Console::WriteLine(" + parms + ")";
                }
                r += "\n";
            }else{
                //Write...
    	        r += "        " + ILAddress(5);
                r += "callvirt instance void class [mscorlib]System.IO.StreamWriter::WriteLine(string,object[])\n"; 
                
                //Flush...
            	r += "        " + ILAddress(1) + "ldarg.0\n";
    	        r += "        " + ILAddress(5);
                r += "ldfld class [mscorlib]System.IO.StreamWriter "+className+"::_writer_"+ILIdentifier(dde.FSDE.Name.Name)+"\n";
    	        r += "        " + ILAddress(5);
                r += "callvirt instance void class [mscorlib]System.IO.StreamWriter::Flush()\n"; 
            }
            return r;
        }
        
        private string EmitAcceptVerb(AcceptVerb accept)
        {
            string r = "";
            string v = ILIdentifier(accept.Identifier.Name);
            //TODO: Needs to work with floating point types...
            //Console.WriteLine("accept = "+accept);
            //Console.WriteLine("accept.Identifier = "+accept.Identifier);
            //Console.WriteLine("accept.Identifier.Definition = "+accept.Identifier.Definition);
            DataType type = accept.Identifier.Definition.Type;
            //string typ = _program.GetVarType(v);
            
            if (type == DataType.Integer)
            {
                r += "        " + ILAddress(5);
                r += "call string class [mscorlib]System.Console::ReadLine()";
                r += "\n";
                r += "        " + ILAddress(1);
                r += "stloc.1";
                r += "\n";
                //Now parse location 0 (__cobolInputTemp) and store in v
                r += "        " + ILAddress(1) + "ldarg.0\n";
                r += "        " + ILAddress(1) + "ldloc.1\n";
                r += "        " + ILAddress(5) + "call int32 int32::Parse(string)\n";
                r += "        ";
                r += ILAddress(5) + "stfld int32 __CobolProgram::" + v;
                r += "\n";
            }
            else
            {
                r += "        ";
                r += ILAddress(1) + "ldarg.0"; //Hidden pointer to 'this'
                r += "\n";
                r += "        ";
                r += ILAddress(5) + "call string class [mscorlib]System.Console::ReadLine()";
                r += "\n";
                r += "        ";
                r += ILAddress(5) + "stfld string __CobolProgram::" + v;
                r += "\n";

                if (IsGroup(accept.Identifier))
                {
                    //TOOD: Set flag to indicate group is empty to true
                    //      Then let EmitStore change it if necessary
                    r += "        " + ILAddress(1) + "ldarg.0\n"; //Hidden pointer to 'this'
                    r += "        " + ILAddress(1) + "ldc.i4.0\n"; //Zero, used for 'false'
                    r += "        " + ILAddress(5) + "stfld bool __CobolProgram::_hasData_"+v+"\n";
                    //laod false  _isEmpty_
                    
                    r += "        ";
                    r += ILAddress(1) + "ldarg.0"; //Hidden pointer to 'this'
                    r += "\n";
                    r += "        ";
                    r += ILAddress(5) + "ldfld string __CobolProgram::" + v;
                    r += "\n";
                    r += EmitStore(accept.Identifier,null);
                }
            }
            

            return r;
        }        
    }
}
