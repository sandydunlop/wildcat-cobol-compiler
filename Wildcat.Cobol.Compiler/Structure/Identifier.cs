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

namespace Wildcat.Cobol.Compiler.Structure
{
    public class Identifier : Source
    {
        private string _value;
        private string _name;
        private DataDescription _definition;
        private ClassDefinition _classDefinition;
        private ArithmeticExpression _substringLeft;
        private ArithmeticExpression _substringRight;
        private ArithmeticExpression _substringLength;
        private ArithmeticExpression _subscript;
        private Source _delimiter;

        public ClassDefinition ClassDefinition
        {
            get { return _classDefinition; }
            set { _classDefinition = value; }
        }

        public Source Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }

        public ArithmeticExpression Subscript
        {
            get { return _subscript; }
            set { _subscript = value; }
        }

        public ArithmeticExpression SubstringLength
        {
            get { return _substringLength; }
            set { _substringLength = value; }
        }

        public ArithmeticExpression SubstringLeft
        {
            get { return _substringLeft; }
            set { _substringLeft = value; }
        }
        
        public ArithmeticExpression SubstringRight
        {
            get { return _substringRight; }
            set { _substringRight = value; }
        }
        
        public bool UsesSubstring
        {
        	get
        	{
        		return (_substringLeft!=null || _substringRight!=null || _substringLength!=null);
        	}
        }
        
        public DataDescription Definition
        {
            get { return _definition; }
            set { _definition = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Identifier()
        {
            _name = null;
            _value = null;
            _definition = null;
            this.IsLiteral = false;
            _delimiter = null;
        }

        public override string ToString()
        {
        	return _name;
        }
    }
}
