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
    public class IfStatement : Command
    {
        private Condition _condition;
        private ArrayList _then;
        private ArrayList _else;

        public ArrayList Then
        {
            get { return _then; }
            set { _then = value; }
        }

        public ArrayList Else
        {
            get { return _else; }
            set { _else = value; }
        }

        public Condition Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public IfStatement()
        {
            _condition = null;
            _then = new ArrayList();
            _else = new ArrayList();
        }
    }
}
