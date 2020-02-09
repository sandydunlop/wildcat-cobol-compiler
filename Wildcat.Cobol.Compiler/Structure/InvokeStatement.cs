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
	public class InvokeStatement : Command
	{
		private Identifier _object;
		private Literal _method;
		private ArrayList _using;
		private Identifier _returning;
		
		public ArrayList Using
		{
			get{ return _using; }
			set{ _using = value; }
		}

		public Identifier Returning
		{
			get{ return _returning; }
			set{ _returning = value; }
		}
		
		public Identifier Object
		{
			get{ return _object; }
			set{ _object = value; }
		}
		
		public Literal Method
		{
			get{ return _method; }
			set{ _method = value; }
		}
		
		public InvokeStatement()
		{
			_using = new ArrayList();
		}
	}
}
