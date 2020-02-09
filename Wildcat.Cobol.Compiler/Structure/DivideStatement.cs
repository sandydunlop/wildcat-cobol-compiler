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
    public class DivideStatement : Command
    {
        private Source _left;
        private ArrayList _right;
        private ArrayList _giving;
        private bool _usingGiving;
        private Identifier _remainder;

        public Identifier Remainder
        {
            get { return _remainder; }
            set { _remainder = value; }
        }

        public Source Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public ArrayList Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public ArrayList Giving
        {
            get { return _giving; }
            set { _giving = value; }
        }
        
        public bool UsingGiving
        {
            get { return _usingGiving; }
            set { _usingGiving = value; }
        }

        public DivideStatement()
        {
        	_right = new ArrayList();
        	_giving = new ArrayList();
        	_usingGiving = false;
        }
    }
}
