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
using System.Collections.Generic;

namespace Wildcat.Cobol.Compiler.Structure
{
    public class OpenStatement : Command
    {
    	private bool _isInput;
    	private bool _isOutput;
    	private bool _isIO;
    	private bool _isExtend;
    	private List<string> _files;

		public List<string> Files
		{
			get { return _files; }
			set {_files = value; }
		}
		
		public bool IsInput
		{
			get { return _isInput; }
			set {_isInput = value; }
		}
		
		public bool IsOutput
		{
			get { return _isOutput; }
			set {_isOutput = value; }
		}
		
		public bool IsIO
		{
			get { return _isIO; }
			set {_isIO = value; }
		}
		
		public bool IsExtend
		{
			get { return _isExtend; }
			set {_isExtend = value; }
		}
		
        public OpenStatement()
        {
        	_files = new List<string>();
        }
    }
}
