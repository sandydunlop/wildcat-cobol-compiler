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
	public class MoveStatement : Command
	{
		private Source _from;
		private ArrayList _to;
		private bool _corresponding;
		
		public Source From
		{
			get{ return _from; }
			set{ _from = value; }
		}
		
		public ArrayList To
		{
			get{ return _to; }
			set{ _to = value; }
		}
		
		public bool Corresponding
		{
			get{ return _corresponding; }
			set{ _corresponding = value; }
		}
		
		public MoveStatement()
		{
			_from = null;
			_to = new ArrayList();
		}
	}
}
