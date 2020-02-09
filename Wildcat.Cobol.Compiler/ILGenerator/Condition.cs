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
        private string EmitCondition(Condition condition, string trueAddr, string falseAddr)
        {
            //if falseAddr == null, then don't implement a jump for false condition
            //always implement a jump for true condition

            //TODO: This code is stolen from EmitLoopCondition below.
            // EmitLoopCondition should call this

            string r = "";
            if (condition.GetType() == typeof(RelationCondition))
            {
                RelationCondition cond = condition as RelationCondition;

				//Remove the "if (cond.Combined.Count>0)" stuff.
				//This code handles both situations
					
				RelationCondition last = cond;
				RelationCondition orig = cond;
				int conditionNumber = 0;
				int conditionCount = cond.Combined.Count;
				//Console.WriteLine("conditionCount = "+conditionCount);
				r+="\n";
				
				//If we have multiple conditions, set the 1st one's AND/OR
				//type to be the same as the next one as it's not initally set.
				if (conditionCount>0)
				{
					cond = cond.Combined[0] as RelationCondition;
					last.CombinedWithAnd = cond.CombinedWithAnd;
					last.CombinedWithOr = cond.CombinedWithOr;
					cond = last;
				}
				
				List<string> ifSegments = new List<string>();
				List<bool> needsToBreakToNext = new List<bool>();
				
				while(conditionNumber <= conditionCount)
				{
					//Console.WriteLine("conditionNumber = "+conditionNumber);
					//cond.CombinedWithAnd - break only on false
					//cond.CombinedWithOr  - break only on true
					
					string s = "";
	                s += EmitExpression(cond.LeftExpression);
	                s += EmitExpression(cond.RightExpression);
	                if (cond.RelationalOperator.Relation == RelationType.EqualTo)
	                {
	                	//true, false, breakOnTrueOnly, breakOnFalseOnly
	                	s+=EmitConditionEqualTo(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else if (cond.RelationalOperator.Relation == RelationType.GreaterThan)
	                {
	                	s+=EmitConditionGreaterThan(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else if (cond.RelationalOperator.Relation == RelationType.LessThan)
	                {
	                	s+=EmitConditionLessThan(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else if (cond.RelationalOperator.Relation == RelationType.GreaterThanOrEqualTo)
	                {
	                	s+=EmitConditionGreaterThanOrEqualTo(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else if (cond.RelationalOperator.Relation == RelationType.LessThanOrEqualTo)
	                {
	                	s+=EmitConditionLessThanOrEqualTo(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else if (cond.RelationalOperator.Relation == RelationType.NotEqualTo)
	                {
	                	s+=EmitConditionNotEqualTo(trueAddr,falseAddr,last.CombinedWithOr,last.CombinedWithAnd);
	                }
	                else
					{
						throw new CompilerException("Relation type not supported yet");
					}
					
					ifSegments.Add(s);
					if (last.CombinedWithAnd && String.IsNullOrEmpty(falseAddr))
					{
						needsToBreakToNext.Add(true);
					}else{
						needsToBreakToNext.Add(false);
					}

					if (conditionNumber == conditionCount)
					{
						//Console.WriteLine("Last condition");
						//For the last condition in the list, we have to branch to somewhere...
						if (last.CombinedWithAnd)
						{
							//Jump to trueAddr
							s = "";
							s += "        //last jump trueAddr\n";
			                s += "        " + ILAddress(5);
			                s += "br " + trueAddr + "\n";
							ifSegments.Add(s);
							needsToBreakToNext.Add(false);
						}
						if (last.CombinedWithOr)
						{
							//Jump to falseAddr
							s = "";
							s += "        //last jump falseAddr\n";
			                s += "        " + ILAddress(5);
		                	s += "br " + falseAddr + "\n";
							ifSegments.Add(s);
			                if (String.IsNullOrEmpty(falseAddr))
			                {
								needsToBreakToNext.Add(true);
			                }else{
								needsToBreakToNext.Add(false);
			                }
						}
					}
					if (conditionNumber < conditionCount)
					{
						last = cond;
						cond = orig.Combined[conditionNumber] as RelationCondition;
					}
					conditionNumber++;
				}
				
				string nextAddr = ILAddress();
				for (int n = 0; n<ifSegments.Count; n++)
				{
					string ifSegment = ifSegments[n];
					if (needsToBreakToNext[n])
					{
						ifSegment = ifSegment.Substring(0,ifSegment.Length-1);
						ifSegment += nextAddr + "\n";
					}
					r+=ifSegment;
				}
            }
            else
            {
            	//TODO: Implement this
                throw new CompilerException("Other condition types not supported yet");
            }
            return r;
        }

        private string EmitConditionEqualTo(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //EqualTo\n";
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
            if (trueOnly)
            {
	            r += "        " + ILAddress(5);
	            r += "brfalse " + trueAddr + "\n";  //Is Equal
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5);
	            r += "brtrue " + falseAddr + "\n";  //Is Not Equal
            }else{
	            r += "        " + ILAddress(5);
	            r += "brfalse " + trueAddr + "\n";  //Is Equal
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";  //Is Not Equal
	            }
            }
            return r;
        }
        
        private string EmitConditionNotEqualTo(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //NotEqualTo\n";
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
            if (trueOnly)
            {
	            r += "        " + ILAddress(5);
	            r += "brtrue " + trueAddr + "\n";  //Is Not Equal
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5);
	            r += "brfalse " + falseAddr + "\n";  //Is Equal
            }else{
	            r += "        " + ILAddress(5);
	            r += "brtrue " + trueAddr + "\n";  //Is Not Equal
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";  //Is Equal
	            }
            }
            return r;
        }
        
        private string EmitConditionGreaterThan(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //GreaterThan\n";
            //Stop looping when left > right
            if (trueOnly)
            {
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "bgt.un " + trueAddr + "\n";    //Is Greater than
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "ble.un " + falseAddr + "\n";    //Is not greater than
            }else{
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "bgt.un " + trueAddr + "\n";    //Is greater than
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";   //Is not greater than
	            }
            }
            return r;
        }       
        
        private string EmitConditionLessThan(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //LessThan\n";
            //Stop looping when left < right
            if (trueOnly)
            {
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "blt.un " + trueAddr + "\n";    //Is less than
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "bge.un " + falseAddr + "\n";    //Is not less than
            }else{
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "blt.un " + trueAddr + "\n";    //Is less than
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";   //Is not less than
	            }
            }
            return r;
        } 
        
        private string EmitConditionGreaterThanOrEqualTo(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //GreaterThanOrEqualTo\n";
            //Stop looping when left > right
            if (trueOnly)
            {
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "bge.un " + trueAddr + "\n";    //Is Greater than or equal to
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "blt.un " + falseAddr + "\n";    //Is not greater than or equal to
            }else{
	            r += "        " + ILAddress(5); //1 for ble, and 4 for the address
	            r += "bge.un " + trueAddr + "\n";    //Is greater than or equal to
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";   //Is not greater than
	            }
            }
            return r;
        }

        private string EmitConditionLessThanOrEqualTo(string trueAddr, string falseAddr, bool trueOnly, bool falseOnly)
        {
        	string r = "";
			r += "        //LessThanOrEqualTo\n";
            //Stop looping when left < right
            if (trueOnly)
            {
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "ble.un " + trueAddr + "\n";    //Is less than or equal to
            }else if (falseOnly)
            {
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "bgt.un " + falseAddr + "\n";    //Is not less than or equal to
            }else{
	            r += "        " + ILAddress(5); //1 for bge, and 4 for the address
	            r += "ble.un " + trueAddr + "\n";    //Is less than or equal to
	            if (falseAddr != null)
	            {
	                //jump to falseAddr
	                r += "        " + ILAddress(5);
	                r += "br " + falseAddr + "\n";   //Is not less than or equal to
	            }
            }
        	return r;
        }
    }
}
