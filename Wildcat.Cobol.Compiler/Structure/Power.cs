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
    public class Power : AST
    {
    	private Basis _basis;
        private PowerSign _sign;
        private ArrayList _basises; //power-sign and Basis pairs

        public ArrayList Basises
        {
            get { return _basises; }
            set { _basises = value; }
        }

        public PowerSign Sign
        {
            get { return _sign; }
            set { _sign = value; }
        }
    	
    	public Basis Basis
    	{
    		get{ return _basis; }
    		set{ _basis = value; }
    	}
    	
        public Power()
        {
            _sign = PowerSign.None;
            _basises = new ArrayList();
            _basis = null;
        }
    }

    public enum PowerSign
    {
        None,
        Add,
        Subtract
    }
}
