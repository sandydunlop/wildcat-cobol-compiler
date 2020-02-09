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
    public class Pic : AST
    {
        private string _name;
        private int _size;
        private DataType _type;
        private string _extendedType;  //TODO: This shouldn't be a string

        public string ExtendedType
        {
            get { return _extendedType; }
            set { _extendedType = value; }
        }
        
        public DataType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Pic()
        {
        }
    }
}
