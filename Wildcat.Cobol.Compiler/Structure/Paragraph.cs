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
	public class Paragraph : AST
	{
		private string _name;
        private ArrayList _sentences;
        private Literal _attributes;
				
        public ArrayList Sentences
        {
            get { return _sentences; }
            set { _sentences = value; }
        }
				
		public string Name
		{
			get{ return _name; }
			set{ _name = value; }
		}

		public Literal Attributes
		{
			get{ return _attributes; }
			set{ _attributes = value; }
		}

		public Paragraph()
		{
            _sentences = new ArrayList();
		}
	}
}
