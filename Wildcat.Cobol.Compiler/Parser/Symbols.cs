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

namespace Wildcat.Cobol.Compiler.Parser
{
	public class Symbols
	{
		Hashtable _symbols;
		int _longest = 0;
		
		public Symbols()
		{
			_symbols = new Hashtable();
			//From longest name to shortest name...
            _symbols.Add("WORKING-STORAGE", SymType.WorkingStorage);
            _symbols.Add("SOURCE-COMPUTER", SymType.SourceComputer);
            _symbols.Add("OBJECT-COMPUTER", SymType.ObjectComputer);
            _symbols.Add("IDENTIFICATION", SymType.IdentificationDivision);
            _symbols.Add("CORRESPONDING", SymType.Corresponding);
            _symbols.Add("CONFIGURATION", SymType.Configuration);
            _symbols.Add("COMPUTATIONAL", SymType.Computational);
            _symbols.Add("END-SUBTRACT", SymType.EndSubtract);
            _symbols.Add("INPUT-OUTPUT", SymType.InputOutput);
            _symbols.Add("END-MULTIPLY", SymType.EndMultiply);
            _symbols.Add("FILE-CONTROL", SymType.FileControl);
            _symbols.Add("ORGANIZATION", SymType.Organization);
			_symbols.Add("END-PERFORM",SymType.EndPerform);
            _symbols.Add("I-O-CONTROL", SymType.IOControl);
            _symbols.Add("HIGH-VALUES", SymType.HighValues);
            _symbols.Add("ENVIRONMENT", SymType.EnvironmentDivision);
            _symbols.Add("PROGRAM-ID", SymType.ProgramID);
            _symbols.Add("HIGH-VALUE", SymType.HighValues);
            _symbols.Add("ATTRIBUTES", SymType.Attributes);
            _symbols.Add("UPPER-CASE", SymType.UpperCase);
            _symbols.Add("END-DIVIDE", SymType.EndDivide);
            _symbols.Add("REPOSITORY", SymType.Repository);
            _symbols.Add("SEQUENTIAL", SymType.Sequential);
            _symbols.Add("LOW-VALUES", SymType.LowValues);
            _symbols.Add("LOW-VALUE", SymType.LowValues);
            _symbols.Add("PROCEDURE", SymType.ProcedureDivision);
            _symbols.Add("REMAINDER", SymType.Remainder);
            _symbols.Add("REDEFINES", SymType.Redefines);
            _symbols.Add("DELIMITED", SymType.Delimited);
            _symbols.Add("REFERENCE", SymType.Reference);
            _symbols.Add("RETURNING", SymType.Returning);
            _symbols.Add("ADVANCING", SymType.Advancing);
            _symbols.Add("END-WRITE", SymType.EndWrite);
            _symbols.Add("MULTIPLY", SymType.Multiply);
            _symbols.Add("SUBTRACT", SymType.Subtract);
            _symbols.Add("FUNCTION", SymType.Function);
            _symbols.Add("DIVISION", SymType.Division);
            _symbols.Add("END-READ", SymType.EndRead);
            _symbols.Add("OPTIONAL", SymType.Optional);
            _symbols.Add("PICTURE", SymType.Pic);
			_symbols.Add("PERFORM",SymType.Perform);
            _symbols.Add("THROUGH", SymType.Through);
            _symbols.Add("END-ADD", SymType.EndAdd);
			_symbols.Add("ROUNDED",SymType.Rounded);
            _symbols.Add("DISPLAY", SymType.DisplayVerb);
            _symbols.Add("VARYING", SymType.Varying);
            _symbols.Add("PROGRAM", SymType.Program);
            _symbols.Add("SECTION", SymType.Section);
            _symbols.Add("POINTER", SymType.Pointer);
            _symbols.Add("INVALID", SymType.Invalid);
            _symbols.Add("SELECT", SymType.Select);
            _symbols.Add("ASSIGN", SymType.Assign);
            _symbols.Add("AUTHOR", SymType.Author);
            _symbols.Add("END-IF", SymType.EndIf);
            _symbols.Add("DIVIDE", SymType.Divide);
            _symbols.Add("BINARY", SymType.Binary);
            _symbols.Add("SPACES", SymType.Spaces);
            _symbols.Add("ACCEPT", SymType.AcceptVerb);
            _symbols.Add("STRING", SymType.StringVerb);
            _symbols.Add("GIVING", SymType.Giving);
            _symbols.Add("FILLER", SymType.Filler);
            _symbols.Add("OCCURS", SymType.Occurs);
            _symbols.Add("STATIC", SymType.Static);
            _symbols.Add("OBJECT", SymType.Object);
            _symbols.Add("INVOKE", SymType.Invoke);
            _symbols.Add("EXTEND", SymType.Extend);
            _symbols.Add("OUTPUT", SymType.Output);
            _symbols.Add("RECORD", SymType.Record);
            _symbols.Add("BEFORE", SymType.Before);
            _symbols.Add("ZEROES", SymType.Zeros);
            _symbols.Add("ZEROS", SymType.Zeros);
            _symbols.Add("QUOTES", SymType.Quotes);
            _symbols.Add("QUOTE", SymType.Quotes);
            _symbols.Add("AFTER", SymType.After);
            _symbols.Add("INPUT", SymType.Input);
            _symbols.Add("USING", SymType.Using);
            _symbols.Add("UNTIL", SymType.Until);
            _symbols.Add("CLASS", SymType.Class);
            _symbols.Add("VALUE", SymType.Value);
            _symbols.Add("ERROR", SymType.Error);
            _symbols.Add("SPACE", SymType.Spaces);
            _symbols.Add("CLOSE", SymType.Close);
            _symbols.Add("WRITE", SymType.Write);
            _symbols.Add("LINES", SymType.Lines);
            _symbols.Add("FALSE", SymType.False);
            _symbols.Add("LINE", SymType.Line);
            _symbols.Add("TRUE", SymType.True);
            _symbols.Add("READ", SymType.Read);
            _symbols.Add("NEXT", SymType.Next);
            _symbols.Add("LOCK", SymType.Lock);
            _symbols.Add("ZERO", SymType.Zeros);
            _symbols.Add("DATA", SymType.DataDivision);
            _symbols.Add("PAGE", SymType.Page);
            _symbols.Add("INTO", SymType.Into);
            _symbols.Add("MOVE", SymType.Move);
            _symbols.Add("CORR", SymType.Corresponding);
			_symbols.Add("THRU",SymType.Through);
            _symbols.Add("FROM", SymType.From);
            _symbols.Add("THEN", SymType.Then);
            _symbols.Add("ELSE", SymType.Else);
			_symbols.Add("SIZE",SymType.Size);
			_symbols.Add("FILE",SymType.File);
            _symbols.Add("WITH", SymType.With);
            _symbols.Add("STOP", SymType.Stop);
            _symbols.Add("COMP", SymType.Computational);
            _symbols.Add("OPEN", SymType.Open);
            _symbols.Add("EXIT", SymType.Exit);
            _symbols.Add("RUN", SymType.Run);
            _symbols.Add("PIC", SymType.Pic);
			_symbols.Add("ADD",SymType.Add);
			_symbols.Add("ARE",SymType.Are);
            _symbols.Add("NOT", SymType.Not);
            _symbols.Add("END", SymType.End);
            _symbols.Add("SET", SymType.Set);
            _symbols.Add("AND", SymType.And);
            _symbols.Add("KEY", SymType.Key);
            _symbols.Add("I-O", SymType.IO);
            _symbols.Add("ALL", SymType.All);
            _symbols.Add("AS", SymType.As);
            _symbols.Add("AT", SymType.At);
            _symbols.Add("IS", SymType.Is);
            _symbols.Add("BY", SymType.By);
            _symbols.Add("FD", SymType.FD);
            _symbols.Add("SD", SymType.SD);
            _symbols.Add("NO", SymType.No);
            _symbols.Add("TO", SymType.To);
            _symbols.Add("ID", SymType.IdentificationDivision);
            _symbols.Add("ON", SymType.On);
            _symbols.Add("IF", SymType.If);
            _symbols.Add("OR", SymType.Or);
            _symbols.Add("**", SymType.PowerSign);
			_symbols.Add("(",SymType.LBracket);
			_symbols.Add(")",SymType.RBracket);
			_symbols.Add("'",SymType.SingleQuote);
			_symbols.Add("\"",SymType.DoubleQuote);
			_symbols.Add(".",SymType.Dot);
			_symbols.Add(",",SymType.Comma);
			_symbols.Add("=",SymType.EqualTo);
			_symbols.Add("<",SymType.LessThan);
			_symbols.Add(">",SymType.GreaterThan);
			_symbols.Add("+",SymType.PlusSign);
			_symbols.Add("-",SymType.MinusSign);
			_symbols.Add("*",SymType.MultiplySign);
            _symbols.Add("/", SymType.DivideSign);
            _symbols.Add(":", SymType.Colon);
        }
		
		public int Longest
		{
			get{
				if (_longest==0){
					foreach (string s in _symbols.Keys)
					{
						if (s.Length>_longest){
							_longest = s.Length;
						}
					}
				}
				return _longest;
			}
		}
		
		public SymType Find(string sub)
		{
			sub = sub.ToUpper();
			if (_symbols.Contains(sub)){
				return (SymType)_symbols[sub];
			}else{
				return SymType.Undefined;
			}
		}
	}
}