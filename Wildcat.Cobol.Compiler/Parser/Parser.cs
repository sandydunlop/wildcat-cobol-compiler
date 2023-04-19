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
using System.Collections;
using Wildcat.Cobol.Compiler.Structure;
using Wildcat.Cobol.Compiler.Exceptions;

namespace Wildcat.Cobol.Compiler.Parser
{
	public class Parser
	{
        Program _ast;
        Tokenizer _tokenizer;
        
        public Parser()
        {
            _ast = null;
        }
		
		public Program Parse(string program, bool verbose)
		{
			_ast = new Program();
            _tokenizer = new Tokenizer(program, verbose);
			_tokenizer.getsym();
			Divisions(ref _ast);
			return _ast;
		}
		
		private void Divisions(ref Program program)
		{
			do {
				if (_tokenizer.Accept(SymType.IdentificationDivision))
				{
                    _tokenizer.Expect(SymType.Division);
                    _tokenizer.Expect(SymType.Dot);
                    program.Divisions.Add(ParseIdentificationDivision());
				}
				else if (_tokenizer.Accept(SymType.Program))
				{
                    _tokenizer.Expect(SymType.Division);
                    _tokenizer.Expect(SymType.Dot);
					//TODO: Implement this
				}
				else if (_tokenizer.Accept(SymType.EnvironmentDivision))
				{
                    _tokenizer.Expect(SymType.Division);
                    _tokenizer.Expect(SymType.Dot);
					program.Divisions.Add(ParseEnvironmentDivision());
				}
				else if (_tokenizer.Accept(SymType.DataDivision))
				{
                    _tokenizer.Expect(SymType.Division);
                    _tokenizer.Expect(SymType.Dot);
                    program.Divisions.Add(ParseDataDivision());
				}
				else if (_tokenizer.Accept(SymType.ProcedureDivision))
				{
                    _tokenizer.Expect(SymType.Division);
                    _tokenizer.Expect(SymType.Dot);
                    program.Divisions.Add(ParseProcedureDivision());
                }else if (_tokenizer.Accept(SymType.End))
                {
                    //The end of the program
                    _tokenizer.Expect(SymType.Program);
                    return;
				}else{
					if (_tokenizer.CurrentSymbol!=null)
					{
               			throw new UnexpectedTokenException(_tokenizer.LineNumber,_tokenizer.CurrentSymbol.Spelling);
					}
					break;
				}
			}while(true);
		}
		
		private EnvironmentDivision ParseEnvironmentDivision()
		{
			EnvironmentDivision env = new EnvironmentDivision();
			
			bool gotConfigSection = false;
			bool gotIOSection = false;
            bool cont = true;
			do 
			{
                cont = false;
				if (!gotConfigSection && _tokenizer.Accept(SymType.Configuration))
				{
					_tokenizer.Expect(SymType.Section);
					_tokenizer.Expect(SymType.Dot);
					env.ConfigurationSection = ParseConfigurationSection();
					gotConfigSection = true;
                    cont = true;
				}
				else if (!gotIOSection && _tokenizer.Accept(SymType.InputOutput))
				{
					_tokenizer.Expect(SymType.Section);
					_tokenizer.Expect(SymType.Dot);
					env.InputOutputSection = ParseInputOutputSection();
					gotIOSection = true;
                    cont = true;
				}
			}while(cont);
			return env;
		}
		
		private InputOutputSection ParseInputOutputSection()
		{
			InputOutputSection io = new InputOutputSection();
			if ((io.FileControl = ParseFileControlParagraph())!=null)
			{
				io.IOControl = ParseIOControlParagraph();
			}
			else if ((io.IOControl = ParseIOControlParagraph())!=null)
			{
				io.FileControl = ParseFileControlParagraph();
			}
			else
			{
				throw new UnexpectedTokenException(_tokenizer.LineNumber,_tokenizer.CurrentSymbol.Spelling);
			}
			return io;
		}
		
		private FileControlParagraph ParseFileControlParagraph()
		{
			if (!_tokenizer.Accept(SymType.FileControl))
			{
				return null;
			}
			_tokenizer.Expect(SymType.Dot);
			FileControlParagraph para = new FileControlParagraph();
			FileControlEntry fce = null;
			while ((fce=ParseFileControlEntry())!=null)
			{
				para.Entries.Add(fce);
			}
			return para;
		}
		
		private IOControlParagraph ParseIOControlParagraph()
		{
			if (!_tokenizer.Accept(SymType.IOControl))
			{
				return null;
			}
      		throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"I-O-CONTROL");
		}
		
		private FileControlEntry ParseFileControlEntry()
		{
            if (_tokenizer.CurrentSymbol.Type != SymType.Select)
            {
                return null;
            }
			FileControlEntry entry = new FileControlEntry();
            entry.Select = ParseSelectClause();
            entry.Assign = ParseAssignClause();
            if (_tokenizer.Accept(SymType.Organization))
            {
                _tokenizer.Accept(SymType.Is);
                if (_tokenizer.Accept(SymType.Line))
                {
                    _tokenizer.Expect(SymType.Sequential);
                    entry.LineSequential = true;
                }
                else if (_tokenizer.Expect(SymType.Sequential))
                {
                    entry.LineSequential = true;
                }
                else
                {
                    throw new UnexpectedTokenException(_tokenizer.LineNumber, _tokenizer.CurrentSymbol.Spelling);
                }
            }
            else if (_tokenizer.Accept(SymType.Line))
            {
                _tokenizer.Expect(SymType.Sequential);
                entry.LineSequential = true;
            }
            else if (_tokenizer.Expect(SymType.Sequential))
            {
                entry.LineSequential = true;
            }
            _tokenizer.Expect(SymType.Dot);
			return entry;
		}

        private SelectClause ParseSelectClause()
        {
            SelectClause selectClause = new SelectClause();
            _tokenizer.Expect(SymType.Select);
            if (_tokenizer.Accept(SymType.Optional))
            {
                selectClause.Optional = true;
            }
            selectClause.Filename = ParseIdentifier();
            return selectClause;
        }

        private AssignClause ParseAssignClause()
        {
            AssignClause assignClause = new AssignClause();
            _tokenizer.Expect(SymType.Assign);
            _tokenizer.Accept(SymType.To);
            assignClause.Source = ParseSource();
            return assignClause;
        }

        private IOControlEntry ParseIOControlEntry()
		{
      		throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"I-O-CONTROL");
			// IOControlEntry entry = new IOControlEntry();
			// return entry;
		}
		
		private ConfigurationSection ParseConfigurationSection()
		{
			ConfigurationSection conf = new ConfigurationSection();
			while(true)
			{
				if (_tokenizer.Accept(SymType.Repository))
				{
					_tokenizer.Expect(SymType.Dot);
					conf.Repository = ParseRepository();
				}
				else if (_tokenizer.Accept(SymType.Attributes))
				{
					conf.Attributes = ParseLiteral();
					_tokenizer.Expect(SymType.Dot);
				}
				else if (_tokenizer.Accept(SymType.SourceComputer))
				{
					_tokenizer.GoToNextLine();
				}
				else if (_tokenizer.Accept(SymType.ObjectComputer))
				{
					_tokenizer.GoToNextLine();
				}
				else
				{
					break;
				}
			}
			return conf;
		}
		
		private Repository ParseRepository()
		{
			Repository rep = new Repository();
			while(true)
			{
				if (_tokenizer.Accept(SymType.Class))
				{
					ClassDefinition classDef = ParseClassDefinition();
					rep.Classes.Add(classDef);
					_ast.ClassDeclarations.Add(classDef.Name.Name,classDef);
				}else{
					break;
				}
			}
			return rep;
		}
		
		private ClassDefinition ParseClassDefinition()
		{
			ClassDefinition c = new ClassDefinition();
			c.Name = ParseIdentifier();
			_tokenizer.Expect(SymType.As);
			c.NetName = ParseLiteral();
			return c;
		}
		
		private IdentificationDivision ParseIdentificationDivision()
		{
			IdentificationDivision division = new IdentificationDivision();
			if (_tokenizer.Accept(SymType.ProgramID))
			{
                _tokenizer.Expect(SymType.Dot);
				Symbol current = _tokenizer.CurrentSymbol;
				_tokenizer.Expect(SymType.Text);
				_tokenizer.Expect(SymType.Dot);
				division.ProgramID = current.Spelling;
			}
			if (_tokenizer.Accept(SymType.Author))
			{
				_tokenizer.GoToNextLine();
				//_tokenizer.Expect(SymType.Comment);
				//_tokenizer.Expect(SymType.Text);
				//TODO: Save author in division object
			}
			return division;
		}

		private DataDivision ParseDataDivision()
		{
			DataDivision division = new DataDivision();
            bool gotWorkingStorageSection = false;
            bool gotFileSection = false;
            bool cont = true;
            do{
                cont = false;
			    if (!gotWorkingStorageSection && _tokenizer.Accept(SymType.WorkingStorage))
			    {
                    _tokenizer.Expect(SymType.Section);
                    _tokenizer.Expect(SymType.Dot);
				    division.WorkingStorage = ParseWorkingStorageSection();
                    cont = true;
                    gotWorkingStorageSection = true;
			    }
                if (!gotFileSection && _tokenizer.Accept(SymType.File))
                {
                    _tokenizer.Expect(SymType.Section);
                    _tokenizer.Expect(SymType.Dot);
                    division.FileSection = ParseFileSection();
                    cont = true;
                    gotFileSection = true;
                }
            } while(cont);
			return division;
		}

        private FileSection ParseFileSection()
        {
            FileSection fs = new FileSection();
            FileAndSortDescriptionEntry fsde = null;
            do
            {
                fsde = null;
                if (_tokenizer.Accept(SymType.FD))
                {
                    fsde = new FileAndSortDescriptionEntry();
                    fsde.Type = "file";
                    fsde.Name = ParseIdentifier();
                    _tokenizer.Expect(SymType.Dot);
                    //TODO: Expand this, based on: 
                    // http://www.cs.vu.nl/grammars/vs-cobol-ii/#gdef:file-and-sort-description-entry-clauses
                    
                    //TODO:
                    //label-records-clause	=	"LABEL" ( "RECORD" [ "IS" ] | "RECORDS" [ "ARE" ] ) ( "STANDARD" | "OMITTED" | { data-name }+ )
                    
					while(true)
					{
                    	DataDescription data = ParseDataDescription();
                    	if (data==null)
                    	{
                    		break;
                    	}else{
                    		//TODO:
                    		//Should ws.DataDescriptions be update here?
                    		//Or should the DDE be saved in the FileAndSortDescriptionEntry?
                    		//Or should the FSDE just have a reference to the WS DDE?
							//ws.DataDescriptions.Add(data);

                    		//Save variable declaration
							if (data.IsVariableDeclaration)
							{
								_ast.VariableDeclarations.Add(data.Name, data);
							}
							fsde.DataDescriptions.Add(data);
                    	}
					}
                    
                    fs.Entries.Add(fsde);
                }
                else if (_tokenizer.Accept(SymType.SD))
                {
                    throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber, "SD");
                }
            } while (fsde != null);
            return fs;
        }
        
        private DataDescription ParseDataDescription()
        {
            //Parse the level
            Symbol current;
            current = _tokenizer.CurrentSymbol;
			if (!_tokenizer.Accept(SymType.Number)){
				return null;
			}
            int level = Int32.Parse(current.Spelling);
            
			DataDescription data = new DataDescription();
			data.Level = level;
			data.LineNumber = _tokenizer.LineNumber;
            if (level==88)
            {
            	data.Type = DataType.Boolean;
            }

            //Parse the data name
            Identifier id = null;
            if (!_tokenizer.Accept(SymType.Filler))
            {
                id = ParseIdentifier();
            }
            if (id == null)
            {
                //Anonymous Data
                data.Name = null;
            }
            else
            {
                data.Name = id.Name;
            }
            
            if (_tokenizer.Accept(SymType.Object))
            {
            	//Object-oriented/.NET Extensions
            	if (_tokenizer.Accept(SymType.Reference))
            	{
            		data.IsClass = true;
            		data.ClassId = ParseIdentifier();
            		_tokenizer.Expect(SymType.Dot);
//=============================================================================
            		//ws.DataDescriptions.Add(data); 
              		//_ast.VariableDeclarations.Add(data.Name, data);
              		data.IsVariableDeclaration = true;
//=============================================================================
					return data;
            	}
            	else if (_tokenizer.Accept(SymType.Static))
            	{
            		//TODO: Static class reference
            		//Use for calling NUnit's Assert methods?
            		//I don't think this is needed though
            		throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"STATIC");
            	}
            	else
            	{
            		throw new UnexpectedTokenException(_tokenizer.LineNumber,_tokenizer.CurrentSymbol.Spelling);
            	}
            }

            if (_tokenizer.Accept(SymType.Binary))
            {
                data.IsBinary = true;
            }

            if (_tokenizer.Accept(SymType.Dot))
            {
                //This is a group item, rather than an elementary item
                data.IsGroup = true;
                data.Type = DataType.String;
//=============================================================================
                //ws.DataDescriptions.Add(data);
                if (data.Name!=null){
                	//_ast.VariableDeclarations.Add(data.Name, data);
              		data.IsVariableDeclaration = true;
                }
//=============================================================================
				return data;
            }

            data.Occurs = 0;
            if (_tokenizer.Accept(SymType.Occurs))
            {
                current = _tokenizer.CurrentSymbol;
                _tokenizer.Expect(SymType.Number);
                data.Occurs = Int32.Parse(current.Spelling);
            }

            //Parse the value
            if (_tokenizer.Accept(SymType.Value))
            {
                if (_tokenizer.Accept(SymType.Spaces))
                {
                    data.IsSpaces = true;
                }
                else if (_tokenizer.Accept(SymType.HighValues))
                {
                    data.IsHighValues = true;
                }
                else
                {
                    data.Value = ParseLiteral().Value;
                }
            }
            
            //Parse the data type
            if (_tokenizer.Accept(SymType.Redefines))
            {
                id = ParseIdentifier();
                data.Redefines = id.Name;
                if (_tokenizer.Accept(SymType.Dot))
                {
                    //This is a group item, rather than an elementary item
                    //TODO: Check if this is really a group
                    data.IsGroup = true;
//=============================================================================
                    //ws.DataDescriptions.Add(data);
//=============================================================================
					return data;
                }
            }

            if (_tokenizer.Accept(SymType.Dot))
            {
                //This is a group item with a value
                data.IsGroup = true;
//=============================================================================
                //ws.DataDescriptions.Add(data);
                if (data.Name!=null){
                	//_ast.VariableDeclarations.Add(data.Name, data);
              		data.IsVariableDeclaration = true;
                }
//=============================================================================
				return data;
            }

            Pic pic = ParsePic();
            if (pic != null)
            {
                data.Size = pic.Size;
                data.Type = pic.Type;
                data.ExtendedType = pic.ExtendedType;
            }
            
            //Parse the value
            if (_tokenizer.Accept(SymType.Value))
            {
                //TODO: Figurative constants...
                //ZERO, SPACE, QUOTES, or ALL
                if (_tokenizer.Accept(SymType.Spaces))
                {
                    data.IsSpaces = true;
                }
                else
                {
                    data.Value = ParseLiteral().Value;
                }
            }

            _tokenizer.Expect(SymType.Dot);
//=============================================================================
            if (data.Name != null)
            {
                //_ast.VariableDeclarations.Add(data.Name, data);
           		data.IsVariableDeclaration = true;
            }
			//ws.DataDescriptions.Add(data);
//=============================================================================
			return data;
        }
		
		private WorkingStorageSection ParseWorkingStorageSection()
		{
            //TODO: Refactor this
            //Parse DDEs (Data Description Entries)
			WorkingStorageSection ws = new WorkingStorageSection();
			while (true)
			{
				DataDescription data = ParseDataDescription();
				if (data==null)
				{
					break;
				}
				else
				{
					//TODO: Add data description to ws.DataDescriptions and _ast.VariableDeclarations
					ws.DataDescriptions.Add(data);
					if (data.IsVariableDeclaration)
					{
						_ast.VariableDeclarations.Add(data.Name, data);
					}
				}
			}
			return ws;
		}

        private Identifier ParseIdentifier()
        {
            Symbol current = _tokenizer.CurrentSymbol;
            if (current == null)
                return null;
            Identifier id = new Identifier();
            id.Name = current.Spelling;
            id.LineNumber = _tokenizer.LineNumber;
            if (_tokenizer.Accept(SymType.Text))
            {
                if (_tokenizer.Accept(SymType.LBracket))
                {
                    //TODO: There's a problem pasring "k+1" into this expression
                    ArithmeticExpression expr = ParseArithmeticExpression();
                    if (_tokenizer.Accept(SymType.Colon))
                    {
                        //Leftmost,Length
                        id.SubstringLeft = expr;
                        if (_tokenizer.CurrentSymbol.Type!=SymType.RBracket){
                        	id.SubstringRight = ParseArithmeticExpression();
                        }
                        //TODO: Should this be 'Right' or 'Length'?
                    }
                    else
                    {
                        //Subscript
                        //TODO: There should be the ability to be multiple subscripts
                        //Implement this: qualified-data-name { "(" subscript ")" }*
                        id.Subscript = expr;
                    }
                    _tokenizer.Expect(SymType.RBracket);
                }
                return id;
            }
            else
            {
                return null;
            }
        }

        private string ParseParagraphName()
        {
            Symbol current = _tokenizer.CurrentSymbol;
            if (_tokenizer.Accept(SymType.Text)||_tokenizer.Accept(SymType.Number))
            {
                return current.Spelling;
            }
            else
            {
                return null;
            }
        }

        private Pic ParsePic()
		{
			//TODO: This isn't parsing integer definitions properly
            Pic pic = new Pic();
            string number = null;
			_tokenizer.Expect(SymType.Pic);
            string str = _tokenizer.CurrentSymbol.Spelling;
            if (_tokenizer.Accept(SymType.Number))
            {
                number = _tokenizer.CurrentSymbol.Spelling;
                pic.Type = DataType.Integer;
                pic.Size = str.Length;
            }
            else if(_tokenizer.Accept(SymType.Text))
            {
                if (str.Substring(0,1).ToUpper() == "X")
                {
                    pic.Type = DataType.String;
                    pic.Size = str.Length;
                }
                else
                {
                    pic.Type = DataType.Integer;
                }
            }else{
            	//TODO: Unexpected token exception instead of this...?
                throw new Wildcat.Cobol.Compiler.Exceptions.InvalidSyntaxException(_tokenizer.LineNumber, _tokenizer.CurrentSymbol.Spelling);
            }
            if (_tokenizer.Accept(SymType.LBracket))
            {
                number = _tokenizer.CurrentSymbol.Spelling;
                _tokenizer.Expect(SymType.Number);
                pic.Size = Int32.Parse(number);
                _tokenizer.Expect(SymType.RBracket);
                if (_tokenizer.Accept(SymType.Computational))
                {
                	pic.ExtendedType = "comp";
                }
            }
            return pic;
		}
		
		private Literal ParseLiteral()
		{
            Literal literal = new Literal();
            Symbol current = _tokenizer.CurrentSymbol;
            string text = "";
			//Quoted string literal.
			//TODO: Am I doing this right?
			if (_tokenizer.Accept(SymType.SingleQuote))
			{
                _tokenizer.QuoteCharacter = "'";
                current = _tokenizer.CurrentSymbol;
                while (_tokenizer.Accept(SymType.Text))
                {
                    text += current.Spelling;
                    current = _tokenizer.CurrentSymbol;
                }
				_tokenizer.Expect(SymType.SingleQuote);
                literal.Type = VariableType.String;

            }
			else if (_tokenizer.Accept(SymType.DoubleQuote))
			{
                _tokenizer.QuoteCharacter = "\"";
                current = _tokenizer.CurrentSymbol;
                //TODO: Parsing of string literals needs some work...
                while (_tokenizer.Accept(SymType.Text)||_tokenizer.Accept(SymType.Number))
                {
                    text += current.Spelling;
                    current = _tokenizer.CurrentSymbol;
                }
                _tokenizer.Expect(SymType.DoubleQuote);
                literal.Type = VariableType.String;
            }
            else if (_tokenizer.Accept(SymType.PlusSign))
            {
                text += "+" + _tokenizer.CurrentSymbol.Spelling;
                _tokenizer.Expect(SymType.Number);
                literal.Type = VariableType.Integer;
            }
            else if (_tokenizer.Accept(SymType.MinusSign))
            {
                text += "-" + _tokenizer.CurrentSymbol.Spelling;
                _tokenizer.Expect(SymType.Number);
                literal.Type = VariableType.Integer;
            }
            else if (_tokenizer.Accept(SymType.Number))
            {
                //TODO: Floating point
                text += current.Spelling;
                literal.Type = VariableType.Integer;
            }
            else if (_tokenizer.Accept(SymType.True))
            {
            	literal.BooleanValue = true;
                literal.Type = VariableType.Boolean;
            }
            else if (_tokenizer.Accept(SymType.False))
            {
            	literal.BooleanValue = false;
                literal.Type = VariableType.Boolean;
            }
            else
            {
                _tokenizer.InQuotes = false;
                return null;
            }
            literal.Value = text;
            return literal;
		}
		
		private Source ParseSource()
		{
			return ParseSource(false);
		}
		
		private Source ParseSource(bool goBackOnDot)
		{
			Source source = null;

			//TODO: This is not the way to parse functions
			//      They need to be treated as different tokens in Tokenizer.getsym
            if (_tokenizer.Accept(SymType.Function))
            {
                source = ParseIntrinsicFunction();
            }
            else if (_tokenizer.Accept(SymType.Spaces))
            {
                //TODO: Implement this properly
                Literal literal = new Literal();
                literal.Type = VariableType.String;
                literal.Value = " ";
                source = literal as Source;
            }
            else if (_tokenizer.Accept(SymType.Zeros))
            {
                Literal literal = new Literal();
                literal.Type = VariableType.Integer;
                literal.Value = "0";
                source = literal as Source;
            }
            else
            {
                source = ParseFigurativeConstant() as Source;
                if (source == null)
                {
                    source = ParseLiteral() as Source;
                    if (source == null)
                    {
                        source = ParseIdentifier() as Source;
                        if (source == null)
                            return null;
                        source.LineNumber = _tokenizer.LineNumber;
//                      TODO: Fix this
//    	            	if (_tokenizer.Accept(SymType.Delimited))
//    	            	{
//    	            		Console.WriteLine("*** PARSING DELIMITED ***");
//    	                    Identifier iden = source as Identifier;
//    	            		_tokenizer.Accept(SymType.By);//optional BY
//    	            		iden.Delimiter = ParseSource();
//    	            	}
                    }
                }
            }
//            if (source.GetType() == typeof(Identifier))
//            {
//            	Identifier id = source as Identifier;
//            }
            if (goBackOnDot==true && source!=null && _tokenizer.CurrentSymbol!=null && _tokenizer.CurrentSymbol.Type == SymType.Dot)
            {
            	_tokenizer.RestorePosition();
            	//Console.WriteLine(System.Environment.StackTrace.ToString());
				//Console.WriteLine("_tokenizer.CurrentSymbolP = "+_tokenizer.CurrentSymbolP);
				//Console.WriteLine(_program.Substring(_tokenizer.CurrentSymbolP));
            	return null;
            }
            if (source.GetType()==typeof(Identifier))
            {
                _ast.VariableReferences.Add(source);            	
            }
			return source;
		}
		
		private FigurativeConstant ParseFigurativeConstant()
		{
		    FigurativeConstant fc = new FigurativeConstant();
            if (_tokenizer.Accept(SymType.Spaces))
            {
                fc.Type = FigurativeConstantType.Spaces;
                return fc;
            }else{
                return null;
            }
		}
		
		private ProcedureDivision ParseProcedureDivision()
		{
			ProcedureDivision division = new ProcedureDivision();
			//TODO: Look at grammar - more to do here
			Symbol current = _tokenizer.CurrentSymbol;
            string paragraphName;
            while ((paragraphName = ParseParagraphName()) != null)
			{
				Paragraph para = new Paragraph();
                para.Name = paragraphName;
                if (_tokenizer.Accept(SymType.With))
                {
                	_tokenizer.Expect(SymType.Attributes);
                	para.Attributes = ParseLiteral();
                }
				_tokenizer.Expect(SymType.Dot);
				//That identifier was the paragraph name
				
				//Sentence needs done many times
                Sentence sentence;
                while((sentence = ParseSentence())!=null){
				    para.Sentences.Add(sentence);
                }
				division.Paragraphs.Add(para);
				current = _tokenizer.CurrentSymbol;
			}
			return division;
		}

        private Sentence ParseSentence()
		{
            Sentence sentence = new Sentence();
            sentence.LineNumber = _tokenizer.LineNumber;
            string lineNumber = _tokenizer.LineNumber;
			if (_tokenizer.Accept(SymType.DisplayVerb))
			{
                sentence.Command = ParseDisplayVerb();
            }
            else if (_tokenizer.Accept(SymType.AcceptVerb))
            {
                sentence.Command = ParseAcceptVerb();
            }
            else if (_tokenizer.Accept(SymType.StringVerb))
            {
                sentence.Command = ParseStringVerb();
            }
            else if (_tokenizer.Accept(SymType.Perform))
            {
                sentence.Command = ParsePerformVerb();
            }
            else if (_tokenizer.Accept(SymType.Open))
            {
                sentence.Command = ParseOpenStatement();
            }
            else if (_tokenizer.Accept(SymType.Close))
            {
                sentence.Command = ParseCloseStatement();
            }
            else if (_tokenizer.Accept(SymType.Read))
            {
                sentence.Command = ParseReadStatement();
            }
            else if (_tokenizer.Accept(SymType.Write))
            {
                sentence.Command = ParseWriteStatement();
            }
            else if (_tokenizer.Accept(SymType.If))
            {
                sentence.Command = ParseIfStatement();
            }
            else if (_tokenizer.Accept(SymType.Move))
            {
                sentence.Command = ParseMoveStatement();
            }
            else if (_tokenizer.Accept(SymType.Add))
            {
                sentence.Command = ParseAddStatement();
            }
            else if (_tokenizer.Accept(SymType.Subtract))
            {
                sentence.Command = ParseSubtractStatement();
            }
            else if (_tokenizer.Accept(SymType.Multiply))
            {
                sentence.Command = ParseMultiplyStatement();
            }
            else if (_tokenizer.Accept(SymType.Divide))
            {
                sentence.Command = ParseDivideStatement();
            }
            else if (_tokenizer.Accept(SymType.Set))
            {
            	sentence.Command = ParseSetStatement();
            }
            else if (_tokenizer.Accept(SymType.Invoke))
            {
            	sentence.Command = ParseInvokeStatement();
            }
            else if (_tokenizer.Accept(SymType.Exit))
            {
            	sentence.Command = ParseExitStatement();
            }
            else if (_tokenizer.Accept(SymType.Stop))
            {
                if (_tokenizer.Accept(SymType.Run))
                {
                    _tokenizer.Expect(SymType.Dot);
                    //End of program
                    return null;
                }
                //End of program
                Console.WriteLine("ERROR: STOP with no RUN at line "+_tokenizer.LineNumber);
                return null;
            }
            else
            {
                //TODO: Is this an unexpected end here?
                return null;
            }

            //TODO: Dot terminates a sentence or paragraph?
            _tokenizer.Accept(SymType.Dot);

            sentence.Command.LineNumber = lineNumber;
			//TODO: Other verbs
			return sentence;
        }
        
        private OpenStatement ParseOpenStatement()
        {
        	OpenStatement openStatement = new OpenStatement();
        	if (_tokenizer.Accept(SymType.Input))
        	{
        		openStatement.IsInput = true;
        	}
        	else if (_tokenizer.Accept(SymType.Output))
        	{
        		openStatement.IsOutput = true;
        	}
        	else if (_tokenizer.Accept(SymType.IO))
        	{
        		openStatement.IsIO = true;
        	}
        	else if (_tokenizer.Accept(SymType.Extend))
        	{
        		openStatement.IsExtend = true;
        	}else{
        		throw new Compiler.Exceptions.UnexpectedTokenException(
        			_tokenizer.LineNumber,
        			_tokenizer.CurrentSymbol.Spelling);
        	}
            Symbol current = _tokenizer.CurrentSymbol;
        	while (_tokenizer.Accept(SymType.Text))
        	{
        		openStatement.Files.Add(current.Spelling);
        		current = _tokenizer.CurrentSymbol;
        	}
        	return openStatement;
        }
        
        private CloseStatement ParseCloseStatement()
        {
        	CloseStatement closeStatement = new CloseStatement();
            Symbol current = _tokenizer.CurrentSymbol;
        	while (_tokenizer.Accept(SymType.Text))
        	{
        		bool withLock = false;
        		if (_tokenizer.Accept(SymType.With))
        		{
        			_tokenizer.Expect(SymType.Lock);
        			withLock = true;
        		}
        		else if (_tokenizer.Accept(SymType.Lock))
        		{
        			withLock = true;
        		}
        		closeStatement.Files.Add(current.Spelling);
        		closeStatement.WithLocks.Add(withLock);
        		current = _tokenizer.CurrentSymbol;
        	}
        	return closeStatement;
        }
        
        private ReadStatement ParseReadStatement()
        {
			//throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"READ");
        	Symbol current = _tokenizer.CurrentSymbol;
        	ReadStatement readStatement = new ReadStatement();
        	readStatement.Filename = ParseIdentifier();
        	
        	bool seenNext = false;
        	bool seenRecord = false;
        	bool seenInto = false;
        	bool seenEndStatements = false;
        	bool seenNotEndStatements = false;
        	
            bool cont = true;
            while (cont)
            {
            	cont = false;
            	current = _tokenizer.CurrentSymbol;
            	if (!seenNext && _tokenizer.Accept(SymType.Next))
            	{
            		readStatement.Next = true;
            		seenNext = true;
            		cont = true;
            	}
            	else if (!seenRecord && _tokenizer.Accept(SymType.Record))
            	{
            		readStatement.Next = true;
            		seenRecord = true;
            		cont = true;
            	}
            	else if (!seenInto && _tokenizer.Accept(SymType.Into))
            	{
            		readStatement.Into = ParseIdentifier();
            		seenInto = true;
            		cont = true;
            	}
            	else if (!seenEndStatements && (_tokenizer.Accept(SymType.At) || _tokenizer.Accept(SymType.End)))
            	{
            		if (current.Type == SymType.At)
            		{
            			_tokenizer.Expect(SymType.End);
            		}
		            Sentence sentence;
		            //Console.WriteLine("Parsing AT END statements");
		            while ((sentence=ParseSentence())!=null)
		            {
						readStatement.EndStatements.Add(sentence);
		            }
		            //Console.WriteLine("Finished parsing AT END statements");
            		seenEndStatements = true;
            		cont = true;
            	}
            	else if (!seenNotEndStatements && (_tokenizer.Accept(SymType.Not)))
            	{
            		_tokenizer.Accept(SymType.At);
            		_tokenizer.Expect(SymType.End);
		            Sentence sentence;
		            while ((sentence=ParseSentence())!=null)
		            {
						readStatement.NotEndStatements.Add(sentence);
		            }
            		seenNotEndStatements = true;
            		cont = true;
            	}
            }
            _tokenizer.Accept(SymType.EndRead);
            //TODO: Should END-READ be required when there is a statement list?
        	
        	return readStatement;
        }
        
        private WriteStatement ParseWriteStatement()
        {
			//throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"WRITE");
        	WriteStatement writeStatement = new WriteStatement();
        	//TODO: More complex versions of WRITE
    	    writeStatement.RecordName = ParseIdentifier();
            _ast.VariableReferences.Add(writeStatement.RecordName);
        	if (_tokenizer.Accept(SymType.From))
        	{
        	    writeStatement.From = ParseIdentifier();
                _ast.VariableReferences.Add(writeStatement.From);
        	}
        	_tokenizer.Accept(SymType.EndWrite);
        	return writeStatement;
        }
        
        private ExitStatement ParseExitStatement()
        {
        	ExitStatement exitstatement = new ExitStatement();
        	_tokenizer.Expect(SymType.Program);
        	//TODO: Exit other things too, and not just "PROGRAM" ?
        	return exitstatement;
        }
        
        private InvokeStatement ParseInvokeStatement()
        {
        	InvokeStatement invoke = new InvokeStatement();
        	invoke.Object = ParseIdentifier();
            _ast.VariableReferences.Add(invoke.Object);
        	invoke.Method = ParseLiteral();
        	while (_tokenizer.Accept(SymType.Using))
        	{
        		//TODO: If BY is not specified then default to value
        		_tokenizer.Expect(SymType.By);
        		if (_tokenizer.Accept(SymType.Value))
        		{
        			Source src = ParseSource();
        			src.ByReference = false;
        			invoke.Using.Add(src);
        		}
        		else if (_tokenizer.Accept(SymType.Reference))
        		{
        			Source src = ParseSource();
        			src.ByReference = true;
        			invoke.Using.Add(src);
        		}
        		else
        		{
        			//TODO: Error
        			throw new Compiler.Exceptions.UnexpectedTokenException(_tokenizer.LineNumber, _tokenizer.CurrentSymbol.Spelling);
        		}
        	}
        	if (_tokenizer.Accept(SymType.Returning))
        	{
        		invoke.Returning = ParseIdentifier();
                _ast.VariableReferences.Add(invoke.Returning);
        	}
        	return invoke;
        }
        
        private SetStatement ParseSetStatement()
        {
        	SetStatement set = new SetStatement();
        	set.To = ParseIdentifier();
            _ast.VariableReferences.Add(set.To);
        	_tokenizer.Expect(SymType.To);
        	set.From = ParseSource();
        	return set;
        }
        
        private DivideStatement ParseDivideStatement()
        {
        	DivideStatement divide = new DivideStatement();
            Source source;
            divide.Left = ParseSource();
            if (_tokenizer.Accept(SymType.By))
            {
                //TODO: Implement the difference between By and Into
            }else if (_tokenizer.Accept(SymType.Into))
            {
                throw new Compiler.Exceptions.NotImplementedException(_tokenizer.LineNumber,"INTO");
            }else
            {
                //TODO: Exception
                Console.WriteLine("ERROR: INTO or BY expected in DIVIDE statement");
                return null;
            }

            while ((source = ParseSource()) != null)
            {
                divide.Right.Add(source);
                if (!source.IsLiteral && _tokenizer.Accept(SymType.Rounded))
                {
                    source.IsRounded = true;
                }
            }

            if (_tokenizer.Accept(SymType.Giving))
            {
            	divide.UsingGiving = true;
                while ((source = ParseSource()) != null)
                {
                    divide.Giving.Add(source);
                    if (_tokenizer.Accept(SymType.Rounded))
                    {
                        source.IsRounded = true;
                    }
                }
            }

            if (_tokenizer.Accept(SymType.Remainder))
            {
                divide.Remainder = ParseIdentifier();
                _ast.VariableReferences.Add(divide.Remainder);
                //TODO: Implement remainder in IL Generator
            }

            //TODO: Implement ON SIZE ERROR, etc
            _tokenizer.Accept(SymType.EndDivide);

        	return divide;
        }
        
        private AddStatement ParseAddStatement()
        {
        	//This attempts to cover Formats 1 and 2 in the grammar
        	AddStatement add = new AddStatement();
        	if (_tokenizer.Accept(SymType.Corresponding))
        	{
        		//TODO: ADD CORRESPONDING has a different grammar definition from other ADDs
        	}else{
	        	Source param;
	        	while ((param = ParseSource())!=null)
	        	{
		        	add.From.Add(param);
	        	}
	        	if (_tokenizer.Accept(SymType.To))
	        	{
	        		//Optional, but if it's missed out, GIVING must be used later
	        	}
	        	while ((param = ParseSource())!=null)
	        	{
		        	add.To.Add(param);
		        	//TODO: Optional ROUNDED
	        	}
	        	//The 'To' sources must be identifiers unless GIVING is specified
	        	if (_tokenizer.Accept(SymType.Giving))
	        	{
	        		add.UsingGiving = true;
		        	while ((param = ParseSource())!=null)
		        	{
			        	add.Giving.Add(param);
	                    if (_tokenizer.Accept(SymType.Rounded))
	                    {
	                        param.IsRounded = true;
	                    }
		        	}
	        	}

				//	[ [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "NOT" [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "END-ADD" ] .
				_tokenizer.Accept(SymType.On); //Optional "ON"
				if (_tokenizer.Accept(SymType.Size))
				{
					//TODO: "SIZE"
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndAdd
				}
				else if (_tokenizer.Accept(SymType.Not))
				{
					_tokenizer.Accept(SymType.On); //Optional "ON"
					_tokenizer.Expect(SymType.Size);
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndAdd
				}
				_tokenizer.Accept(SymType.EndAdd);//This is required if a statement list preceeds it
        	}
        	return add;
        }
        
        private SubtractStatement ParseSubtractStatement()
        {
        	//This attempts to cover Formats 1 and 2 in the grammar
        	SubtractStatement sub = new SubtractStatement();
        	if (_tokenizer.Accept(SymType.Corresponding))
        	{
        		//TODO: SUBTRACT CORRESPONDING has a different grammar definition from other SUBTRACTSs
        	}else{
	        	Source param;
	        	while ((param = ParseSource())!=null)
	        	{
		        	sub.Left.Add(param);
	        	}
	        	if (_tokenizer.Accept(SymType.From))
	        	{
	        		//Optional, but if it's missed out, GIVING must be used later
	        	}
	        	while ((param = ParseSource())!=null)
	        	{
		        	sub.Right.Add(param);
		        	//TODO: Optional ROUNDED
	        	}
	        	//The 'From' sources must be identifiers unless GIVING is specified
	        	if (_tokenizer.Accept(SymType.Giving))
	        	{
	        		sub.UsingGiving = true;
		        	while ((param = ParseSource())!=null)
		        	{
			        	sub.Giving.Add(param);
	                    if (_tokenizer.Accept(SymType.Rounded))
	                    {
	                        param.IsRounded = true;
	                    }
		        	}
	        	}

				//	[ [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "NOT" [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "END-ADD" ] .
				_tokenizer.Accept(SymType.On); //Optional "ON"
				if (_tokenizer.Accept(SymType.Size))
				{
					//TODO: "SIZE"
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndSubtract
				}
				else if (_tokenizer.Accept(SymType.Not))
				{
					_tokenizer.Accept(SymType.On); //Optional "ON"
					_tokenizer.Expect(SymType.Size);
					//TODO: "SIZE"
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndSubtract
				}
				_tokenizer.Accept(SymType.EndSubtract);//This is required if a statement list preceeds it
        	}
        	return sub;
        }
        
        private MultiplyStatement ParseMultiplyStatement()
        {
        	MultiplyStatement mul = new MultiplyStatement();
            mul.Left = ParseSource();
            if (_tokenizer.Accept(SymType.By))
            {
            	Source param;
                while ((param = ParseSource())!=null)
	        	{
		        	mul.Right.Add(param);
		        	//TODO: Optional ROUNDED
	        	}
	        	//The 'From' sources must be identifiers unless GIVING is specified
	        	if (_tokenizer.Accept(SymType.Giving))
	        	{
	        		mul.UsingGiving = true;
		        	while ((param = ParseSource())!=null)
		        	{
			        	mul.Giving.Add(param);
	                    if (_tokenizer.Accept(SymType.Rounded))
	                    {
	                        param.IsRounded = true;
	                    }
		        	}
	        	}				
	        	//	[ [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "NOT" [ "ON" ] "SIZE" "ERROR" statement-list ]
				//	[ "END-ADD" ] .
				_tokenizer.Accept(SymType.On); //Optional "ON"
				if (_tokenizer.Accept(SymType.Size))
				{
					//TODO: "SIZE"
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndMultiply
				}
				else if (_tokenizer.Accept(SymType.Not))
				{
					_tokenizer.Accept(SymType.On); //Optional "ON"
					_tokenizer.Expect(SymType.Size);
					//TODO: "SIZE"
					_tokenizer.Expect(SymType.Error);
					//TODO: Parse statement list
					//This should end with EndMultiply
				}
				_tokenizer.Accept(SymType.EndMultiply);
            }
            else
            {
                //TODO: Exception
                Console.WriteLine("ERROR: INTO or BY expected in MULTIPLY statement");
                return null;
            }
        	return mul;
        }
        
        private IntrinsicFunction ParseIntrinsicFunction()
        {
        	IntrinsicFunction f = new IntrinsicFunction();
        	if (_tokenizer.Accept(SymType.UpperCase))
        	{
        			f.Function = IntrFunc.UpperCase;
        	}else{
        		//This is not actually an error, it just means that what is being parsed is not a function
        		return null;
        	}
        	if (_tokenizer.Accept(SymType.LBracket))
        	{
	        	Source param = ParseSource();
	        	f.Parameters.Add(param);
	        	while (_tokenizer.Accept(SymType.Comma))
	        	{
	        		param = ParseSource();
		        	f.Parameters.Add(param);
	        	}
	        	_tokenizer.Expect(SymType.RBracket);
        	}
        	return f;
        }
        
        private MoveStatement ParseMoveStatement()
        {
        	MoveStatement move = new MoveStatement();
        	if (_tokenizer.Accept(SymType.Corresponding))
        	{
        		move.Corresponding = true;
        		move.From = ParseIdentifier();
        	}else{
        		move.From = ParseSource();
        	}
        	_tokenizer.Expect(SymType.To);
        	Identifier id;
        	while ((id=ParseIdentifier())!=null)
        	{
        		move.To.Add(id);
                _ast.VariableReferences.Add(id);
        	}
        	return move;
        }

        private StringVerb ParseStringVerb()
        {
            StringVerb stringverb = new StringVerb();
            stringverb.Sources = ParseSources();
            if (_tokenizer.Accept(SymType.Delimited))
            {
                _tokenizer.Accept(SymType.By);
                stringverb.Delimited = ParseSource();
            }
            if (_tokenizer.Accept(SymType.Into))
            {
                stringverb.IntoIdentifier = ParseIdentifier();
                _ast.VariableReferences.Add(stringverb.IntoIdentifier);
                if (_tokenizer.Accept(SymType.Pointer) || (_tokenizer.Accept(SymType.With) && (_tokenizer.Accept(SymType.Pointer))))
                {
                    stringverb.Pointer = ParseIdentifier();
                    _ast.VariableReferences.Add(stringverb.Pointer);
                }
                //TODO: overflow exception (data.Size can be used)
                //TODO: IL for string verb
            }
            return stringverb;
        }

        private IfStatement ParseIfStatement()
        {
            IfStatement ifstatement = new IfStatement();
            //If_Statement =
      	    //          "IF" condition [ "THEN" ] ( { statement }+ | "NEXT" "SENTENCE" )
		    //          [ "ELSE" ( { statement }+ | "NEXT" "SENTENCE" ) ] [ "END-IF" ] .
            //If, Then, Else, EndIf
            
            //TODO: Only the If-else-endif is implemented
            //  The rest of the COBOL if statement still needs to be implemented
            ifstatement.Condition = ParseCondition();
            _tokenizer.Accept(SymType.Then); //Optional
            Sentence sentence;
            while ((sentence=ParseSentence())!=null)
            {
				ifstatement.Then.Add(sentence);
            }
            if (_tokenizer.Accept(SymType.Else))
            {
                while ((sentence = ParseSentence()) != null)
                {
					ifstatement.Else.Add(sentence);
                }
            }
            _tokenizer.Accept(SymType.EndIf); //Optional
            return ifstatement;
        }

        private PerformVerb ParsePerformVerb()
        {
        	//TODO: Check this meets grammar requirements
        	PerformVerb perform = new PerformVerb();
        	Symbol current = _tokenizer.CurrentSymbol;
        	//TODO:
        	//procedure-name [ ( "THROUGH" | "THRU" ) procedure-name ]
        	if (_tokenizer.Accept(SymType.Until))
        	{
        		perform.IsLoop = true;
        		perform.Until = ParseCondition();
        		while (!(_tokenizer.Accept(SymType.EndPerform)))
        		{
		        	Sentence sentence = ParseSentence();
		        	if (sentence==null)
		        	    break;
		        	perform.Sentences.Add(sentence);
        			perform.CallsParagraph = false;
        		}
        	}
        	else if (_tokenizer.Accept(SymType.Text))
        	{
        		//TODO: PaseIdentifier instead of this
        		perform.ProcedureIdentifier = new Identifier();
	            perform.ProcedureIdentifier.Name = current.Spelling;
	            perform.ProcedureIdentifier.LineNumber = _tokenizer.LineNumber;
        		perform.CallsParagraph = true;
	        	if (_tokenizer.Accept(SymType.Through))
	        	{
		        	ParseIdentifier();
	        	}
	        	if (_tokenizer.Accept(SymType.Until))
	        	{
	        		//TODO: Why is UNTIL here?
	        		perform.Until = ParseCondition();
	        	}
        	}
        	else
        	{
        		PerformVaryingPhrase varying;
        		if ((varying=ParsePerformVaryingPhrase())!=null)
        		{
        			perform.Varying = varying;
	        		perform.IsLoop = true;
        		}
        		while (!(_tokenizer.Accept(SymType.EndPerform)))
        		{
		        	Sentence sentence = ParseSentence();
		        	if (sentence==null)
		        	    break;
		        	perform.Sentences.Add(sentence);
        			perform.CallsParagraph = false;
        			current = _tokenizer.CurrentSymbol;
        		}
        	}
        	
        	//TODO: This is here to allow hanoi.cbl to compile.
        	//      Can't find it in the grammar though.
        	_tokenizer.Accept(SymType.Dot);
        	
        	return perform;
        }

        private PerformVaryingPhrase ParsePerformVaryingPhrase()
        {
        	PerformVaryingPhrase varying = new PerformVaryingPhrase();
        	if (!_tokenizer.Accept(SymType.Varying))
        	{
        		return null;
        	}
        	//TODO: [ [ "WITH" ] "TEST" ( "BEFORE" | "AFTER" ) ]
        	varying.Counter = ParseIdentifier();
        	_tokenizer.Expect(SymType.From);
            //TODO: Error message is wrong when this isn't a literal or variable: eg. "10+4"
        	varying.From = ParseSource();
        	_tokenizer.Expect(SymType.By);
            varying.By = ParseSource();
        	_tokenizer.Expect(SymType.Until);
        	varying.Condition = ParseCondition();
        	return varying;
        }
        
//        private Condition ParseCondition()
//        {
//        	//Console.WriteLine("Parsing condition");
//        	Condition condition;
//        	condition = ParseRelationCondition();
//        	while(true)
//        	{
//        		if (_tokenizer.Accept(SymType.Or))
//        		{
//        		}
//        		else if (_tokenizer.Accept(SymType.And))
//        		{
//        		}
//        		else
//        		{
//        			break;
//        		}
//        	}
//        	//TODO: Other parse conditions here
//        	return condition;
//        }
        
        private Condition ParseCondition()
        {
        	//Console.WriteLine("Parsing condition");
        	Condition condition;
        	
        	condition = ParseCombinableCondition();
        	
        	while(true)
        	{
        		if (_tokenizer.Accept(SymType.Or))
        		{
        			Condition orCondition = ParseCombinableCondition();
        			orCondition.CombinedWithOr = true;
        			condition.Combined.Add(orCondition);
        		}
        		else if (_tokenizer.Accept(SymType.And))
        		{
        			Condition andCondition = ParseCombinableCondition();
        			andCondition.CombinedWithAnd = true;
        			condition.Combined.Add(andCondition);
        		}
        		else
        		{
        			break;
        		}
        	}
        	//TODO: Other parse conditions here
        	return condition;
        }
        
        private Condition ParseCombinableCondition()
        {
        	Identifier booleanIdentifier;
        	Condition condition;
        	if (_tokenizer.Accept(SymType.True))
        	{
        		Literal booleanLiteral = new Literal();
        		booleanLiteral.Type = VariableType.Boolean;
        		booleanLiteral.BooleanValue = true;
	        	condition = new Condition();
	        	condition.IsBoolean = true;
	        	condition.BooleanValue = booleanLiteral as Source;
	        	return condition;
        	}
        	else if (_tokenizer.Accept(SymType.False))
        	{
        		Literal booleanLiteral = new Literal();
        		booleanLiteral.Type = VariableType.Boolean;
        		booleanLiteral.BooleanValue = true;
	        	condition = new Condition();
	        	condition.IsBoolean = true;
	        	condition.BooleanValue = booleanLiteral as Source;
	        	return condition;
        	}
        	else if ((condition = ParseRelationCondition()) != null)
        	{
        		return condition;
        	}
        	else if((booleanIdentifier = ParseIdentifier())!=null)
        	{
        		//Boolean variable?
        		//TODO: In contextual analysis, we need to check this
        		//      is actually a boolean
	        	condition = new Condition();
	        	condition.IsBoolean = true;
	        	condition.BooleanValue = booleanIdentifier as Source;
	        	return condition;
        	}
        	return null;
        }

        private RelationCondition ParseRelationCondition()
        {
        	_tokenizer.SavePosition();
        	RelationCondition condition = new RelationCondition();
        	condition.LeftExpression = ParseArithmeticExpression();
        	condition.RelationalOperator = ParseRelationalOperator();
        	if (condition.RelationalOperator==null)
        	{
        		_tokenizer.RestorePosition();
        		return null;
        	}
        	condition.RightExpression = ParseArithmeticExpression();
        	return condition;
        }
        
        private ArithmeticExpression ParseArithmeticExpression()
        {
            ArithmeticExpression expr = new ArithmeticExpression();
            ArithmeticExpression top = expr;
            //times-div { ( "+" | "-" ) times-div }* .
        	expr.TimesDiv = ParseTimesDiv();
            while (true)
            {
                if (_tokenizer.Accept(SymType.PlusSign))
                {
                    expr.Next = ParseArithmeticExpression();
                    expr.Next.Sign = ExpressionSign.Add;
                    expr = expr.Next;
                }
                else if (_tokenizer.Accept(SymType.MinusSign))
                {
                    expr.Next = ParseArithmeticExpression();
                    expr.Next.Sign = ExpressionSign.Subtract;
                    expr = expr.Next;
                }
                else
                {
                    break;
                }
            }
        	return top;
        }
        
        private TimesDiv ParseTimesDiv()
        {
        	TimesDiv timesdiv = new TimesDiv();
            TimesDiv top = timesdiv;
            //power { ( "*" | "/" ) power }* .
            timesdiv.Power = ParsePower();
        	while(true)
        	{
        		if (_tokenizer.Accept(SymType.MultiplySign))
        		{
                    timesdiv.Next = new TimesDiv();
                    timesdiv.Next.Sign = TimesDivSign.Multiply;
                    timesdiv.Next.Power = ParsePower();
                    timesdiv = timesdiv.Next;
        		}
        		else if (_tokenizer.Accept(SymType.DivideSign))
        		{
                    timesdiv.Next = new TimesDiv();
                    timesdiv.Next.Sign = TimesDivSign.Divide;
                    timesdiv.Next.Power = ParsePower();
                    timesdiv = timesdiv.Next;
        		}
        		else
        		{
                    return top;
        		}
        	}
        }
        
        private Power ParsePower()
        {
        	Power power = new Power();
        	//[ ( "+" | "-" ) ] basis { "**" basis }* .
        	if (_tokenizer.Accept(SymType.PlusSign))
        	{
                power.Sign = PowerSign.Add;
        	}
        	else if (_tokenizer.Accept(SymType.MinusSign))
        	{
                power.Sign = PowerSign.Subtract;
        	}
        	power.Basis = ParseBasis();
        	while (_tokenizer.Accept(SymType.PowerSign)){
        		power.Basises.Add(ParseBasis());
        	}
        	return power;
        }
        
        private Basis ParseBasis()
        {
        	Basis basis = new Basis();
        	//( identifier | literal | "(" arithmetic-expression ")" ) .
            if (_tokenizer.Accept(SymType.LBracket))
            {
                basis.Expression = ParseArithmeticExpression();
                _tokenizer.Expect(SymType.RBracket);
            }
            else
            {
                basis.Source = ParseSource();
            }
        	return basis;
        }
        
        private RelationalOperator ParseRelationalOperator()
        {
        	RelationalOperator op = new RelationalOperator();
        	if (_tokenizer.Accept(SymType.EqualTo))
        	{
        		op.Relation = RelationType.EqualTo;
        	}
        	else if (_tokenizer.Accept(SymType.LessThan))
        	{
        		op.Relation = RelationType.LessThan;
        		if (_tokenizer.Accept(SymType.EqualTo))
        		{
        			op.Relation = RelationType.LessThanOrEqualTo;
        		}else if (_tokenizer.Accept(SymType.GreaterThan)){
        			op.Relation = RelationType.NotEqualTo;
        		}
        	}
        	else if (_tokenizer.Accept(SymType.GreaterThan))
        	{
        		op.Relation = RelationType.GreaterThan;
        		if (_tokenizer.Accept(SymType.EqualTo))
        		{
        			op.Relation = RelationType.GreaterThanOrEqualTo;
        		}
        	}
        	else
        	{
        		//throw new CompilerException("Relational operator not yet implemented: "+_tokenizer.CurrentSymbol.Type+" : "+_tokenizer.CurrentSymbol.Spelling);
	        	//TODO: parse more operators
	        	return null;
        	}
        	return op;
        }
        
        private DisplayVerb ParseDisplayVerb()
		{
            DisplayVerb displayVerb = new DisplayVerb();
			displayVerb.Sources = ParseSources();
			if (_tokenizer.Accept(SymType.No))
			{
				if (_tokenizer.Accept(SymType.Advancing))
				{
					displayVerb.NoAdvancing = true;
				}else{
					//TODO: Better exception
					throw new Compiler.Exceptions.CompilerException("ADVANCING expected after NO");
				}
			}
			//TODO: Upon (see grammar)
            return displayVerb;
		}

        private AcceptVerb ParseAcceptVerb()
        {
            AcceptVerb acceptVerb = new AcceptVerb();
            acceptVerb.Identifier = ParseIdentifier();
            _ast.VariableReferences.Add(acceptVerb.Identifier);
            return acceptVerb;
        }
        
        private ArrayList ParseSources()
		{
            ArrayList sources = new ArrayList();
            Source s;
            while ((s = ParseSource(false)) != null)  //Was: true
            {
                sources.Add(s);
                _tokenizer.SavePosition();
            }
            return sources;
		}
	}
}
