// Copyright (C) 2006-2023 Sandy Dunlop (sandy@sorn.net)
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
	public class DataDescription : AST
	{
		private string _name;
        private DataType _type;
        private string _value;
        private int _size;
        private int _level;
        private bool _isSpaces;
        private bool _isHighValues;
        private string _redefines;
        private DataDescription _redefinesDefinition;
        private int _occurs;
        private bool _isBinary;
        private bool _isGroup;
        private ArrayList _elements;
        private DataDescription _parentGroup;
        private string _lineNumber;
        private Identifier _classId;
        private bool _isClass;
        private bool _isArray;
        private ClassDefinition _classDef;
        private string _extendedType;  //TODO: This shouldn't be a string
        private bool _isVariableDeclaration;
        private FileAndSortDescriptionEntry _fsde;
        private bool _isAnonymous;
        
        public bool IsAnonymous
        {
            get{ return _isAnonymous; }
            set{_isAnonymous = value; }
        }

        public FileAndSortDescriptionEntry FSDE
        {
            get { return _fsde; }
            set { _fsde = value; }
        }
        
        public bool IsVariableDeclaration
        {
            get { return _isVariableDeclaration; }
            set { _isVariableDeclaration = value; }
        }

        public bool IsHighValues
        {
            get { return _isHighValues; }
            set { _isHighValues = value; }
        }

        public string ExtendedType
        {
            get { return _extendedType; }
            set { _extendedType = value; }
        }
        
        public ClassDefinition ClassDefinition
        {
            get { return _classDef; }
            set { _classDef = value; }
        }

        public DataDescription RedefinesDefinition
        {
            get { return _redefinesDefinition; }
            set { _redefinesDefinition = value; }
        }

        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; }
        }

        public bool IsClass
        {
            get { return _isClass; }
            set { _isClass = value; }
        }

        public Identifier ClassId
        {
            get { return _classId; }
            set { _classId = value; }
        }

        public string LineNumber
        {
            get { return _lineNumber; }
            set { _lineNumber = value; }
        }

        public DataDescription ParentGroup
        {
            get { return _parentGroup; }
            set { _parentGroup = value; }
        }

        public ArrayList Elements
        {
            get { return _elements; }
            set { _elements = value; }
        }

        public bool IsGroup
        {
            get { return _isGroup; }
            set { _isGroup = value; }
        }

        public bool IsBinary
        {
            get { return _isBinary; }
            set { _isBinary = value; }
        }

        public int Occurs
        {
            get { return _occurs; }
            set { _occurs = value; }
        }

        public string Redefines
        {
            get { return _redefines; }
            set { _redefines = value; }
        }

        public bool IsSpaces
        {
            get { return _isSpaces; }
            set { _isSpaces = value; }
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public DataType Type
        {
            get { return _type; }
            set { _type = value; }
        }
		
		public string Name
		{
			get{ return _name; }
			set{ _name = value; }
		}
		
		public DataDescription()
		{
            _type = DataType.Unknown;
            _isSpaces = false;
            _isBinary = false;
            _elements = new ArrayList();
            _redefinesDefinition = null;
            _redefines = null;
		}
		
		public override string ToString()
		{
			return _name;
		}
	}

    public enum DataType
    {
        Unknown,
        String,
        Integer,
        Boolean
    }
}
