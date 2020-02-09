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
//started at 19:27

namespace Wildcat.Cobol.Compiler.Structure
{
    public class ReadStatement : Command
    {
    	private List<Sentence> _endStatements;
    	private List<Sentence> _notEndStatements;
    	private Identifier _into;
    	private Identifier _filename;
    	private bool _next;
    	private bool _record;
    	
     	public bool Next
    	{
    		get{ return _next; }
    		set{_next = value; }
    	}
    	
     	public bool Record
    	{
    		get{ return _record; }
    		set{_record = value; }
    	}
    	
    	public List<Sentence> EndStatements
    	{
    		get{ return _endStatements; }
    		set{_endStatements = value; }
    	}

    	public List<Sentence> NotEndStatements
    	{
    		get{ return _notEndStatements; }
    		set{_notEndStatements = value; }
    	}

     	public Identifier Into
    	{
    		get{ return _into; }
    		set{_into = value; }
    	}

     	public Identifier Filename
    	{
    		get{ return _filename; }
    		set{_filename = value; }
    	}

        public ReadStatement()
        {
        	_endStatements = new List<Sentence>();
        	_notEndStatements = new List<Sentence>();
        }
    }
}
