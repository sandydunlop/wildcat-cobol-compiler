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
    public class Literal : Source
    {
        private string _value;
        private VariableType _type;
        private bool _booleanValue;

        public VariableType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool BooleanValue
        {
            get { return _booleanValue; }
            set { _booleanValue = value; }
        }

        public Literal()
        {
            _value = null;
            this.IsLiteral = true;
        }

        public override string ToString()
        {
        	return _value;
        }
    }

    public enum VariableType
    {
        Unknown,
        String,
        Integer,
        Boolean,
    }
}
