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
        private string EmitInvokeStatement(InvokeStatement invoke)
        {
        	string r= "";

        	//TODO: theAssembly should be set based on the CLASS definition for the object
            bool methodIsStatic = false;
        	
        	ClassDefinition classDef = null;
        	//Console.WriteLine("Name = "+invoke.Object.Name);
        	if (invoke.Object.Definition==null)
        	{
        		//It's a class rather than an object.
        		//(eg Static)
                methodIsStatic = true;
        		classDef = _program.ClassDeclarations[invoke.Object.Name] as ClassDefinition;
        		//Console.WriteLine("Type: Class");
        		if (classDef==null)
        		{
        			//TODO: Exception
        			//TODO: This should be done in contextual analyzer
        			Console.WriteLine("Class not defined: "+invoke.Object.Name);
        		}
        	}else{
	        	classDef = invoke.Object.Definition.ClassDefinition;
        		//Console.WriteLine("Type: Object");
        	}
        	string theClass = classDef.NetName.Value;
        	string theMethod = invoke.Method.Value;
        	//Console.WriteLine("theClass = "+theClass);
        	//Console.WriteLine("theMethod = "+theMethod);
        	
        	//Get a list of the types of parameters used in this call
        	List<Type> sourceTypes = new List<Type>();
        	foreach(Source source in invoke.Using)
        	{
        		Type thisType;
            	if (source.GetType() == typeof(Identifier))
            	{
	            	Identifier src = source as Identifier;
	            	if (src.Definition.IsClass)
	            	{
	            		//typeName = src.Definition.ClassDefinition.NetName.Value;
	            		thisType = src.Definition.ClassDefinition.Type;
	            	}
	            	else
	            	{
		            	thisType = GetType(src.Definition.Type);
	            	}
            	}
            	else
            	{
	            	Literal lit = source as Literal;
	            	thisType = GetType(lit.Type);
            	}
        		sourceTypes.Add(thisType);
        		//Console.WriteLine("  ParamType: "+thisType);
        	}
        	
        	Type type = classDef.Type;
        	string theAssembly = "mscorlib"; //The default
      		theAssembly = classDef.CILAssemblyName;

			if (invoke.Method.Value.ToUpper()=="NEW")
			{
        		r+=EmitInvokeConstructor(invoke, sourceTypes, type, theAssembly);
			}
			else
			{
        		r+=EmitInvokeMethod(invoke, sourceTypes, type, theAssembly, methodIsStatic);
			}
        	
            return r;
        }

		private string EmitInvokeConstructor(InvokeStatement invoke, List<Type> sourceTypes, Type type, string theAssembly)
		{
			string r = "";
			int i;
			string paramList = "";
			Type[] types = new Type[sourceTypes.Count];
			for(i=0;i<sourceTypes.Count;i++)
			{
				types[i] = sourceTypes[i];
				if (paramList.Length>0)
				{
					paramList+=",";
				}
				paramList += ILType(sourceTypes[i].FullName);
			}
			ConstructorInfo constructor = type.GetConstructor(types);
			if (constructor==null)
			{
				Console.WriteLine("Could not find constuctor with matching types for "+type.FullName+" in "+theAssembly);
			}
			if (invoke.Returning!=null)
			{
                    r += "        " + ILAddress(1) + "ldarg.0\n";
			}
            i = 0;
            ParameterInfo[] parameters = constructor.GetParameters();
            foreach (Source source in invoke.Using)
            {
            	ParameterInfo p = parameters[i] as ParameterInfo;
            	bool boxThisParam = true;
            	if (p.ParameterType.ToString().Equals(types[i].ToString()))
            	{
            		boxThisParam = false;
            	}
                r += EmitLoadSource(source, source.ByReference, boxThisParam);
                i++;
            }
            r += "        " + ILAddress(5);
            r += "newobj instance void class [" + theAssembly + "]" + type + "::.ctor("+paramList+")";
            r += "\n";

            if (invoke.Returning != null)
            {
                r += EmitStore(invoke.Returning);
            }

			return r;
		}
		
		private string EmitInvokeMethod(InvokeStatement invoke, List<Type> sourceTypes, Type type, string theAssembly, bool methodIsStatic)
		{
			string r = "";
        	ParameterInfo[] parameters;
            PropertyInfo matchingProperty = null;
            MethodInfo bestMatchingMethod = null;
        	string theMethod = invoke.Method.Value;

			Type[] types = new Type[sourceTypes.Count];
			for(int i =0;i<sourceTypes.Count;i++)
			{
				types[i]=sourceTypes[i];
				//Console.WriteLine("Type: "+types[i]);
			}
			MethodInfo mi = type.GetMethod(theMethod,types);
			if (mi==null)
			{
			}else{
				bestMatchingMethod = mi;
			}

            if (invoke.Returning != null)
            {
                //Need a pointer to this for storing the result
                r += "        " + ILAddress(1) + "ldarg.0\n";
            }

            if (bestMatchingMethod == null)
            {
                PropertyInfo[] properties = type.GetProperties();
                //Console.WriteLine("Looking at type: "+type.FullName);
                //Console.WriteLine("Looking for property: "+theMethod);
                foreach (PropertyInfo prop in properties)
                {
                	//Console.WriteLine("Property: "+prop.Name);
                    if (prop.Name == theMethod)
                    {
                        matchingProperty = prop;
                    }
                }
                if (matchingProperty == null)
                {
                    throw new Compiler.Exceptions.CompilerException("Could not find property or method '" + 
                        theMethod + "' with matching parameter types.");
                }
                //Test this with both static and non-static properties
                if (methodIsStatic)
                {
                    r+="        ";
                    r += ILAddress(5);
                    string returnType = ILType(matchingProperty.PropertyType.ToString());
                    r += "call " + returnType + " [" + theAssembly + "]" + type + "::get_" + theMethod + "()";
                    r += "\n";
                }
                else
                {
                    r += "        " + ILAddress(1) + "ldarg.0\n";
                    r += EmitLoadField(invoke.Object, false, true);
                    r += "        " + ILAddress(5);
                    string returnType = ILType(matchingProperty.PropertyType.ToString());
                    r += "callvirt instance " + returnType + " class [" + theAssembly + "]" + type + "::get_" + theMethod + "()";
                    r += "\n";
                }
            }
            else
            {
                //Console.WriteLine("Method: "+theMethod);
                string parms = "";
                parameters = bestMatchingMethod.GetParameters();
                int i = 0;
                foreach (ParameterInfo p in parameters)
                {
                    if (i > 0)
                    {
                        parms += ", ";
                    }
                    Type paramType = p.ParameterType;
                    parms += ILType(paramType.ToString());
                    i++;
                }
                if (!methodIsStatic)
                {
                    r += "        " + ILAddress(1) + "ldarg.0\n";
                    r += EmitLoadField(invoke.Object, false, true);
                }
                i = 0;
                foreach (Source source in invoke.Using)
                {
                	ParameterInfo p = parameters[i] as ParameterInfo;
                	bool boxThisParam = true;
                	//Console.WriteLine("  p="+p.ParameterType);
                	//Console.WriteLine("  s="+(sourceTypes[i] as string));
                	if (p.ParameterType.ToString().Equals(sourceTypes[i].ToString()))
                	{
                		boxThisParam = false;
                	}
                    r += EmitLoadSource(source, source.ByReference, boxThisParam);
                    i++;
                }
                
                //Test this with both static and non-static methods
                if (methodIsStatic)
                {
                    r+="        ";
                    r += ILAddress(5);
                    r += "call void [" + theAssembly + "]" + type + "::" + theMethod + "(" + parms + ")";
                    r += "\n";
                }
                else
                {
                    r += "        " + ILAddress(5);
                    string returnType = ILType(bestMatchingMethod.ReturnType.ToString());
                    r += "callvirt instance " + returnType + " [" + theAssembly + "]" + type + "::" + theMethod + "("+parms+")";
                    r += "\n";
                }
            }

            if (invoke.Returning != null)
            {
                //Method's return value is used
                //TODO: If this is not used in the COBOL program, but the 
                //      method does have one, we need to remove it from
                //      the stack.
                if (bestMatchingMethod!=null && bestMatchingMethod.ReturnType != null)
                {
                    r += EmitStore(invoke.Returning);
                }
                else if (matchingProperty != null && matchingProperty.PropertyType != null)
                {
                    r += EmitStore(invoke.Returning);
                }
                else
                {
                    //TODO: If the property/method has no return value, throw an exception
                }
            }
        	return r;    
		}
    }
    
}
		   