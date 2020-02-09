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
using System.Collections;
using System.Text;
using System.Reflection;
using Wildcat.Cobol.Compiler.References;
using Wildcat.Cobol.Compiler.Structure;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler.Analyzer
{
    public class ContextualAnalyzer
    {
        Program _program;
        ReferenceManager _referenceManager;

        public void Analyze(Program program, ReferenceManager referenceManager)
        {
            _program = program;
            _referenceManager = referenceManager;
          	AggregateDataDefinitions();
            CheckVariableDeclarations();
           	if (_program.Environment!=null)
           	{
           		if (_program.Environment.ConfigurationSection!=null)
           		{
           			if (_program.Environment.ConfigurationSection.Repository!=null)
           			{
			            if (_program.Data!=null)
			            {	           				
			            	ArrangeClassDefinitions();
			            }
           				FindClassReferences();
           			}
           		}
           	}
            if (_program.Data!=null)
            {
            	ArrangeFiles();
	            ArrangeDDEGroups();
	            NameAnonymousDDEs();
	            ArrangeRedefines();
            }else{
           		//TODO: Exception?
            	Console.WriteLine("WARNING: Data Division doesn't exist");
            }
        }
        
        private void FindClassReferences()
        {
        	Repository rep = _program.Environment.ConfigurationSection.Repository;
        	foreach (ClassDefinition def in rep.Classes)
        	{
        		string fxClassName = def.NetName.Value;
//        		Console.WriteLine("Class: "+fxClassName);
                def.Type = _referenceManager.GetTypeFromName(fxClassName);
                if (def.Type==null)
                {
                	throw new Compiler.Exceptions.CompilerException("The type `"+fxClassName+"' could not be found. Are you missing an assembly reference?");
                }
                def.Assembly = def.Type.Assembly;
                def.CILAssemblyName = _referenceManager.GetAssemblyName(fxClassName);
                if (def.CILAssemblyName.IndexOf("-")>-1)
                {
                	def.CILAssemblyName = "'"+def.CILAssemblyName+"'";
                }
//                Console.WriteLine("Type: "+def.Type);
//                Console.WriteLine("Assembly: "+def.Assembly);
//                Console.WriteLine("CILAssemblyName: "+def.CILAssemblyName);
        	}
        }

        private void ArrangeClassDefinitions()
        {
        	Repository rep = _program.Environment.ConfigurationSection.Repository;
            foreach (DataDescription dde in _program.Data.DataDescriptions)
            {
            	if (dde.IsClass)
            	{
            		//Console.WriteLine("Dealing with class for "+dde.Name);
            		ClassDefinition classDef = null;
		        	foreach (ClassDefinition def in rep.Classes)
		        	{
	            		//Console.WriteLine("Trying "+def.Name.Name);
		        		if (def.Name.Name == dde.ClassId.Name)
		        		{
		        			classDef = def;
		        		}
		        	}
		        	if (classDef!=null)
		        	{
		        		dde.ClassDefinition = classDef;
		        		//Console.WriteLine("Setting DDE "+dde.Name+" class "+classDef.NetName.Value);
		        	}
		        	else
		        	{
		        		//TODO: Exception
		        		Console.WriteLine("Undefined class in data division: "+dde.ClassId);
		        	}
            	}
            }
        }

        private void CheckVariableDeclarations()
        {
            // Find all source/sources and give them types or references to definitions
            foreach (Identifier id in _program.VariableReferences)
            {
                //Identifier id = program.VariableReferences[key] as Identifier;
                DataDescription definition = _program.VariableDeclarations[id.Name] as DataDescription;
                ClassDefinition classDef;
                if (definition == null)
                {
                	//Console.WriteLine("No VariableDeclaration for "+id.Name);
                    //Check class defs
                    if (_program.Environment != null)
                    {
                        if (_program.Environment.ConfigurationSection != null)
                        {
                            if (_program.Environment.ConfigurationSection.Repository != null)
                            {
                                classDef = _program.ClassDeclarations[id.Name] as ClassDefinition;
                                if (classDef != null)
                                {
                                    id.ClassDefinition = classDef;
                                    continue;
                                }
                            }
                        }
                    }
                    //Built-in identifiers...
                    switch (id.Name.ToUpper())
                    {
                    	case "ZERO":
                    		break;
                    	default:
		                    throw new UndefinedVariableException(id.LineNumber, id.Name);
                    }
                }
                id.Definition = definition;
            }
        }

        private void ArrangeRedefines()
        {
            foreach (DataDescription dde in _program.Data.DataDescriptions)
            {
                if (dde.Redefines != null)
                {
                    foreach (DataDescription dde2 in _program.Data.DataDescriptions)
                    {
                        if (dde2.Name == dde.Redefines)
                        {
                            dde.RedefinesDefinition = dde2;
                        }
                    }
                }
            }
        }

        private void AggregateDataDefinitions()
        {
            if (_program.Data.WorkingStorage!=null)
            {
			    foreach (DataDescription dde in _program.Data.WorkingStorage.DataDescriptions)
			    {
			    	_program.Data.DataDescriptions.Add(dde);
			    }
            }
            if (_program.Data.FileSection!=null)
            {
            	foreach (FileAndSortDescriptionEntry fsde in _program.Data.FileSection.Entries)
            	{
				    foreach (DataDescription dde in fsde.DataDescriptions)
				    {
				    	_program.Data.DataDescriptions.Add(dde);
				    }
            	}
            }
        }
        
        private void ArrangeFiles()
        {
        	//TODO: Check that FILE-CONTROL entries exist for each FD
        	FileControlParagraph fcp = null;
        	if (_program.Environment !=null &&
                _program.Environment.InputOutputSection !=null &&
                _program.Environment.InputOutputSection.FileControl !=null)
        	{
        		fcp = _program.Environment.InputOutputSection.FileControl;
        	}
            if (_program.Data.FileSection!=null)
            {
            	foreach (FileAndSortDescriptionEntry fsde in _program.Data.FileSection.Entries)
            	{
            	    //Let the DataDescription know which FSDE it is part of
            	    foreach (DataDescription dde in fsde.DataDescriptions)
            	    {
            	       dde.FSDE = fsde;
            	    }
            	    
			    	//Find in environment's INPUT-OUTPUT SECTION's FILE-CONTROL list
			    	bool found = false;
			    	if (fcp!=null)
			    	{
					    foreach (FileControlEntry entry in fcp.Entries)
					    {
					    	if (fsde.Name.Name == entry.Select.Filename.Name)
					    	{
					    		fsde.FileControlEntry = entry;
					    		fsde.Name.Definition = new DataDescription();
					    		fsde.Name.Definition.Type = DataType.String;
					    		found = true;
					    		break;
					    	}
					    }
			    	}
			    	if (!found)
			    	{
			    		throw new Compiler.Exceptions.CompilerException("Could not find FILE-CONTROL entry for "+fsde.Name);
			    	}
            	}
            }
            
        }
        
        private void ArrangeDDEGroups()
        {
            //Arrange DDEs into groups
            ArrayList stack = new ArrayList();
            DataDescription prev = null;
            DataDescription group = null;
            foreach (DataDescription dde in _program.Data.DataDescriptions)
            {
                //Console.WriteLine("DDE Level = "+dde.Level+" name = "+dde.Name);
                if (prev != null && dde.Level < prev.Level)
                {
                    //Console.WriteLine("Ending Group");
                    //Pop while level >= dde.Level
                    while (stack.Count > 0 && (stack[stack.Count - 1] as DataDescription).Level >= dde.Level)
                    {
                        stack.RemoveAt(stack.Count - 1);
                    }
                    if (stack.Count == 0)
                    {
                        group = null;
                        //Console.WriteLine("Ended Group. Stack is empty");
                    }
                    else
                    {
                        group = (stack[stack.Count - 1] as DataDescription);
                        //Console.WriteLine("Ended Group. Top of stack is level "+(stack[stack.Count-1] as DataDescription).Level);
                    }
                }
                if (dde.IsGroup)
                {
                    if (group != null)
                    {
                        group.Elements.Add(dde);
                        dde.ParentGroup = group;
                        //Console.WriteLine("Added group to group with level "+group.Level);
                    }
                    stack.Add(dde);
                    group = dde;
                    //Console.WriteLine("Created group with level "+group.Level);
                }
                else
                {
                    if (group != null)
                    {
                        group.Elements.Add(dde);
                        dde.ParentGroup = group;
                        if (group.Type == DataType.Unknown)
                        {
                            group.Type = dde.Type;
                        }
                        //Console.WriteLine("Added element to group with level "+group.Level);
                    }
                    else
                    {
                        //Console.WriteLine("WARNING: DDE not being added to a group at line " + dde.LineNumber);
                    }
                }
                prev = dde;
            }
        }

        private void NameAnonymousDDEs()
        {
            //Give names to anonymous DDEs
            foreach (DataDescription dde in _program.Data.DataDescriptions)
            {
                if (dde.Name == null)
                {
                    dde.Name = GenerateDDEName();
                    dde.IsAnonymous = true;
                }
            }
        }

        int anonNumber = 0;
        private string GenerateDDEName()
        {
            anonNumber++;
            return "__anonDDE_" + anonNumber;
        }
    }
}
