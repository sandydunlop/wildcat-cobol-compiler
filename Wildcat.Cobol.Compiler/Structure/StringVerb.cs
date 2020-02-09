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
    public class StringVerb : Command
    {
        private ArrayList _sources;
        private Identifier _intoIdentifier;
        private Identifier _pointer;
        private Source _delimited;

        public Source Delimited
        {
            get { return _delimited; }
            set { _delimited = value; }
        }

        public Identifier Pointer
        {
            get { return _pointer; }
            set { _pointer = value; }
        }

        public Identifier IntoIdentifier
        {
            get { return _intoIdentifier; }
            set { _intoIdentifier = value; }
        }

        public ArrayList Sources
        {
            get { return _sources; }
            set { _sources = value; }
        }

        public StringVerb()
        {
            _sources = new ArrayList();
        }
    }
}
