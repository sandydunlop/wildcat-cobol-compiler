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
using System.Diagnostics;

namespace Wildcat.Cobol.Compiler.Exceptions
{
    public class CompilerException : Exception
    {
        private string _msg;
        public CompilerException(string msg)
            : base(msg)
        {
            _msg = msg;
        }

        public CompilerException(string line, string msg)
            : base("Error on line "+line+": "+msg)
        {
            _msg = "Error on line "+line+": "+msg;
        }

        public new string ToString()
        {
            return _msg;
        }
    }

    public class InvalidSyntaxException : CompilerException
    {
        public InvalidSyntaxException(string line, string message)
            : base("Invalid syntax on line " + line +
                           ": " + message)
        {
        }
    }

    public class UnexpectedStatementException : CompilerException
    {
        public UnexpectedStatementException(string line, string message)
            : base("Unexpected statement on line " + line +
                           ": " + message + "\n" +
                           "Perhaps you forgot to begin a new paragraph here")
        {
        }
    }

    public class UnexpectedTokenException : CompilerException
    {
        public UnexpectedTokenException(string line, string message)
            : base("Unexpected token on line " + line +
                           ": " + message)
        {
        }
    }

    public class UnknownKeywordException : CompilerException
    {
        public UnknownKeywordException(string line, string keyword)
            : base("Unknown keyword on line " + line +
                           ": " + keyword)
        {
        }
    }

    public class UndefinedVariableException : CompilerException
    {
        public UndefinedVariableException(string line, string varname)
            : base("Undefined variable on line " + line +
                           ": " + varname)
        {
        }
    }

    public class NotImplementedException : CompilerException
    {
        public NotImplementedException(string line, string feature)
            : base("Feature not implemented on line " + line +
                           ": " + feature)
        {
        }
    }

    public class BasisException : Exception
    {
        public BasisException()
        {
        }
    }
}
