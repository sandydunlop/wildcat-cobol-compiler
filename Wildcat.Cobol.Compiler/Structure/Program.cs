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
using Wildcat.Cobol.Compiler;

namespace Wildcat.Cobol.Compiler.Structure
{
	public class Program : AST
	{
        private ArrayList _divisions;
        private IdentificationDivision _identification = null;
        private DataDivision _data = null;
        private EnvironmentDivision _environment = null;
        private ProcedureDivision _procedure = null;
        private Hashtable _variableDeclarations;
        private ArrayList _variableReferences;
        private Hashtable _classDeclarations;
        private ArrayList _references;

        public ArrayList References
        {
            get { return _references; }
            set { _references = value; }
        }

        public Hashtable ClassDeclarations
        {
            get { return _classDeclarations; }
            set { _classDeclarations = value; }
        }

        public Hashtable VariableDeclarations
        {
            get { return _variableDeclarations; }
            set { _variableDeclarations = value; }
        }

        public ArrayList VariableReferences
        {
            get { return _variableReferences; }
            set { _variableReferences = value; }
        }

		public ArrayList Divisions
		{
			get{ return _divisions; }
		}
		
		public IdentificationDivision Identification
		{
			get{
				if (_identification==null)
					_identification = GetDivision(DivType.Identification) as IdentificationDivision;
				return _identification;
			}
		}
		
		public DataDivision Data
		{
			get{
				if (_data==null)
					_data = GetDivision(DivType.Data) as DataDivision;
				return _data;
			}
		}
		
		public ProcedureDivision Procedure
		{
			get{
				if (_procedure==null)
					_procedure = GetDivision(DivType.Procedure) as ProcedureDivision;
				return _procedure;
			}
		}

		public EnvironmentDivision Environment
		{
			get{
				if (_environment==null)
					_environment = GetDivision(DivType.Environment) as EnvironmentDivision;
				return _environment;
			}
		}
		
		public Program()
		{
			_divisions = new ArrayList();
            _variableDeclarations = new Hashtable();
            _variableReferences = new ArrayList();
            _classDeclarations = new Hashtable();
            _references = new ArrayList();
		}
		
		private Division GetDivision(DivType type)
		{
			foreach (Division d in _divisions)
			{
				if (d.Type == type)
				{
					return d;
				}
			}
			return null;
		}
	}
}
