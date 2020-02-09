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
    public class Condition : AST
    {
    	private ArrayList _combined;
    	private bool _combinedWithOr;
    	private bool _combinedWithAnd;
    	private bool _isBoolean;
    	private Source _booleanValue;
    	
    	public bool IsBoolean
    	{
    		get { return _isBoolean; }
    		set {_isBoolean = value; }
    	}

    	public Source BooleanValue
    	{
    		get { return _booleanValue; }
    		set {_booleanValue = value; }
    	}

    	public ArrayList Combined
    	{
    		get {return _combined;}
    	}
    	
    	public bool CombinedWithOr
    	{
    		get {return _combinedWithOr;}
    		set {_combinedWithOr=value;}
    	}
    	
    	public bool CombinedWithAnd
    	{
    		get {return _combinedWithAnd;}
    		set {_combinedWithAnd=value;}
    	}
    	
        public Condition()
        {
        	_combinedWithOr = false;
        	_combinedWithAnd = false;
        	_combined = new ArrayList();
        }
    }
}
