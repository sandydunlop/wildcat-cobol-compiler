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
using System.Collections.Generic;
using System.Text;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler.Parser
{
    public class Tokenizer
    {
		Symbols _symbols;
		Symbol _sym;
		Symbol _backupSym;
		int _symP;
        int _backupSymP;
		string _program;
        string ident;
        string _lineNumber;
        string _quoteChar;
		bool _newLine;
        bool _inQuotes;
        bool _backupNewLine;
        bool _verbose;

		
		public bool InQuotes
		{
			get{
				return _inQuotes;
			}
			set{
				_inQuotes = value;
			}
		}
		
		public string QuoteCharacter
		{
			get{
				return _quoteChar;
			}
			set{
				_quoteChar = value;
			}
		}
		
		public Symbol CurrentSymbol
		{
			get{
				return _sym;
			}
		}
		
		public string LineNumber
		{
			get{
				return _lineNumber;
			}
		}

    	public Tokenizer(string program, bool verbose)
    	{
			_program = program;
			_verbose = verbose;
            _quoteChar = null;
            _inQuotes = false;
            _sym = null;
            _backupSym = null;
            _symP = 0;
            _backupSymP = 0;
            _newLine = true;
            _backupNewLine = false;
            _symbols = new Symbols();
            ident = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-+#";
    	}
    	
		public void getsym()
		{
			if (_symP>=_program.Length){
				_sym = null;
				return;
			}
			string c = " ";
			bool parseNextLine = true;
            bool changedLine = false;
			while (parseNextLine){
				parseNextLine = false;
				while (_newLine)
				{
                    changedLine = true;
					if (_symP+6>=_program.Length){
						_sym = null;
						return;
					}
                    _lineNumber = _program.Substring(_symP, 6);
					_symP+=6;
					_newLine = false;
					c = _program.Substring(_symP,1);
					if (c=="*"){
                        //This line is a comment
						while (c!="\n"){
							_symP++;
							c = _program.Substring(_symP,1);
						}
						_newLine = true;
						_symP++;
					}
                    else if (c=="-")
                    {
                        //This line is a continuation of the previous line
                        //TODO: This should never be handled here. 
                        //See further down getsym in literal/identifier parsing code.
                    }
				}
				c = _program.Substring(_symP,1);
                if (!_inQuotes || _sym == null || changedLine /*|| _sym.Type != SymType.Text*/)
                {
                    while (c == " " || c == "\t")
                    {
                        //Console.WriteLine("skipping space");
                        _symP++;
                        c = _program.Substring(_symP, 1);
                    }
                }
				int start = _symP;
				int len = 0;
				string sub = null;
				_sym = null;
				int backup = _symP;
				string subN = "";
                SymType typ;
				while (c!="\n")
				{
					len++;
					_symP++;
					c = _program.Substring(_symP,1);
					sub = _program.Substring(start,_symP-start);

					//Get the next char and check if it's whitespace
                    if (sub.Length > 0)
                    {
                        subN = sub.Substring(0, 1);
                    }
                    else
                    {
                        subN = "";
                    }

                    if (ident.IndexOf(subN)==-1)
                    {
                        typ = _symbols.Find(sub);
                        if (typ != SymType.Undefined && (_quoteChar == null || _quoteChar == subN))
                        {
                            _sym = new Symbol(sub, typ, _verbose);
                            break;
                        }
                    }
                    if (ident.IndexOf(c) == -1)
                    {
                        if ((len <= _symbols.Longest))
                        { 
					        typ = _symbols.Find(sub);
                            if (typ != SymType.Undefined && (_quoteChar == null || _quoteChar == subN))
                            {
						        _sym = new Symbol(sub,typ, _verbose);
						        break;
					        }
                            if (ident.IndexOf(c) == -1 && ident.IndexOf(subN) == -1)
                            {
                                break;
                            }
                        }
                        else
                        {
						    if (ident.IndexOf(c)==-1){
							    break;
						    }
					    }
                    }
                }
                if (sub == null)
                {
					parseNextLine = true;
					c = _program.Substring(_symP,1);
					while (c!="\n"){
						_symP++;
						c = _program.Substring(_symP,1);
					}
					_newLine = true;
					_symP++;
				}else{
                    if (sub == "")
                    {
						//I think this is just a blank line and should be ignored.
					}
                    if (_sym == null)
                    {
                        sub = "";
						//Console.WriteLine("rolling back to: "+_program.Substring(backup));
						_symP = backup;
						start = _symP;
						len = 0;
                        bool endQuoteDetected = false;
                        _symP--;

                        //Parsing a literal or identifier
                        do {
                            _symP++;
                            c = _program.Substring(_symP, 1);
                            if (_inQuotes && c == "\n")
                            {
                                //Newline in string
                                //TODO: This next line possibly makes the string include the quote character
                                //Console.WriteLine("** Adding Sub part: " + _program.Substring(start, _symP - start));
                                sub += _program.Substring(start, _symP - start);
                                _symP++;
                                changedLine = true;
                                _lineNumber = _program.Substring(_symP, 6);
                                _symP += 6;
                                _newLine = false;
                                c = _program.Substring(_symP, 1);
                                if (c == "-")
                                {
                                    //This line is a continuation of the previous line
                                    while (_symP < _program.Length && c != _quoteChar)
                                    {
                                        _symP++;
                                        c = _program.Substring(_symP, 1);
                                    }
                                    if (c == _quoteChar)
                                    {
                                        start = _symP + 1;
                                        c = "";
                                    }
                                    else
                                    {
                                    	//TODO: Replace this with the proper COBOL error message
                                        throw new Compiler.Exceptions.UnexpectedTokenException(_lineNumber,
                                        		"'" + c + "'. Quote character expected.");
                                    }
                                }
                                else
                                {
                                    //TODO: Error?
                                    //This should be a line continued form the previous one and therefore have a dash here
                                    Console.WriteLine("ERROR: Unexpected newline in string at line "+_lineNumber);
                                }
                            }
                            if (_inQuotes && c == _quoteChar)
                            {
                                endQuoteDetected = true;
                            }
                        } while (ident.IndexOf(c) != -1 || (_inQuotes && _quoteChar != null && !endQuoteDetected));

						sub += _program.Substring(start,_symP-start);
                        if (sub == "")
                        {
							sub = c;
							_symP++;
						}
						if (sub.Substring(sub.Length-1,1)==".")
						    sub = sub.Substring(0,sub.Length-1);
						SymType t = SymType.Text;
						int tempInt;
						if (Int32.TryParse(sub, out tempInt))
						{
							t = SymType.Number;
						}
						_sym = new Symbol(sub,t, _verbose);
					}
					
					string dsp = " \t";
					int p = _symP;
					
					while (dsp.IndexOf(c)!=-1){
						p++;
						c = _program.Substring(p,1);
					}
					if (c=="\n"){
						_newLine = true;
						_symP=p+1;
					}
				}
			}
            if (!_inQuotes)
            {
                if (_sym.Type == SymType.SingleQuote)
                {
                    _quoteChar = "'";
                    _inQuotes = true;
                }
                if (_sym.Type == SymType.DoubleQuote)
                {
                    _quoteChar = "\"";
                    _inQuotes = true;
                }
            }
            else
            {
                if (_sym.Spelling == _quoteChar)
                {
                    _inQuotes = false;
                    _quoteChar = null;
                }
            }
		}
		
		public void SavePosition()
		{
			//Console.WriteLine("Saving position: "+_symP);
			//Console.WriteLine("Saving sym: "+_sym.Spelling);
			_backupSymP = _symP;
			_backupSym = _sym;
			_backupNewLine = _newLine;
		}
		
		public void RestorePosition()
		{
			_symP = _backupSymP;
			_sym = _backupSym;
			_newLine = _backupNewLine;
			//Console.WriteLine("Restored Position: "+_symP);
			//Console.WriteLine("Restored Sym: "+_sym.Spelling);
		}
		
		public bool endOfLine()
		{
			//test if we're at the end of a line
			string dsp = ". \t";
			int p = _symP;
			string c = _program.Substring(p,1);
			while (dsp.IndexOf(c)!=-1){
				p++;
				c = _program.Substring(p,1);
				if (dsp.IndexOf(c)>-1)
					return false;
			}
			return true;
		}
		
		public void GoToNextLine()
		{
			//Used as a hack to allow parsing of author name in Identification Division
			//Is this because this is a "predictive parser" rather than a standard
			//recursive descent parser?
			string c = _program.Substring(_symP,1);
			while (c!="\n"){
				_symP++;
				c = _program.Substring(_symP,1);
			}
			_newLine = true;
			_symP++;
			getsym();
		}
		
		public bool Accept(SymType symtype)
		{
			if ((_sym!=null)&&_sym.Type==symtype)
			{
				getsym();
				return true;
			}
			return false;
		}
		
		public bool Expect(SymType symtype)
		{
			Symbol current = _sym;
		    if (Accept(symtype))
        		return true;
        	
    		throw new UnexpectedTokenException(_lineNumber, current.Spelling);
		}
    }
}
