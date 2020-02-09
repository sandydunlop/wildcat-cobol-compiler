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

namespace Wildcat.Cobol.Compiler.Parser
{
	public class Symbol
	{
		private SymType _type;
		private string _ident;
		
		public Symbol()
		{
			_type = SymType.Undefined;
		}
		
		public Symbol(string ident, SymType typ, bool verbose)
			:this()
		{
			_ident = ident;
			_type = typ;
			if (verbose)
			{
			    Console.WriteLine("New symbol: '"+ident+"' ("+_type+")");
			}
		}
		
		public SymType Type {
			get { return _type; }
		}
		
		public string Spelling {
			get{ return _ident; }
		}
	}
	
	public enum SymType
	{
		Undefined,
		Comment,
		Text,
		Number,
		Identifier,
		IdentificationDivision,
		ProcedureDivision,
		Program,
		ProgramID,
		Author,
		EnvironmentDivision,
		DataDivision,
		WorkingStorage,
        Section,
		Pic,
		LBracket,
		RBracket,
		Value,
		SingleQuote,
		DoubleQuote,
		Dot,
		DisplayVerb,
        AcceptVerb,
        StringVerb,
        Delimited,
        Into,
        With,
        Perform,
        Through,
        EndPerform,
        Varying,
        From,
        Until,
        By,
        EqualTo,
        LessThan,
        GreaterThan,
        PlusSign,
        MinusSign,
        PowerSign,
        MultiplySign,
        DivideSign,
		Stop,
        Run,
        If,
        Then,
        Else,
        EndIf,
        Move,
        To,
        Corresponding,
        Function,
        Comma,
        UpperCase,
        Add,
        EndAdd,
        Rounded,
        Size,
        Error,
        Giving,
        Not,
        On,
        Divide,
        EndDivide,
        Division,
        Spaces,
        Filler,
        Redefines,
        Occurs,
        Binary,
        Remainder,
        Colon,
        Pointer,
        End,
        Configuration,
        Repository,
        Class,
        As,
        Set,
        Static,
        Object,
        Reference,
        Invoke,
        Returning,
        Using,
        Attributes,
        SourceComputer,
        ObjectComputer,
        Subtract,
        Multiply,
        EndSubtract,
        EndMultiply,
        Computational,
        Or,
        And,
        Exit,
        No,
        Advancing,
        Zeros,
        Open,
        Close,
        Read,
        Write,
        Input,
        Output,
        IO,
        Extend,
        Lock,
        Record,
        At,
        EndRead,
        Before,
        After,
        Line,
        Lines,
        Page,
        Invalid,
        Key,
        EndWrite,
        FileControl,
        Select,
        Organization,
        Is,
        True,
        False,
        Sequential,
        Assign,
        File,
        FD,
        InputOutput,
        IOControl,
        Optional,
        SD,
        HighValues,
        LowValues,
        Next,
        Are,
        Quotes,
        All,
	}	
}