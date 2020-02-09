using System;
using System.Collections;
using System.Text;
using System.Reflection;

namespace Wildcat.Cobol.Compiler.References
{
    public class ReferenceManager
    {
        private ArrayList _references;
        private Hashtable _attributes;
        private Hashtable _assemblyName;
        private Hashtable _types;

        public Hashtable Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public ReferenceManager(ArrayList references)
        {
            _references = references;
            Initialize();
        }

        public string GetAssemblyName(string type)
        {
            return _assemblyName[type] as string;
        }

        public string GetAttributeCILName(string att)
        {
            string r = "";
            string attClass = _attributes[att] as string;
            string asm = _assemblyName[attClass] as string;
            r = "[" + asm + "]" + attClass;
            return r;
        }

        public Type GetTypeFromName(string type)
        {
            Type t = _types[type] as Type;
            return t;
        }

        private void Initialize()
        {
            _assemblyName = new Hashtable();
            _attributes = new Hashtable();
            _types = new Hashtable();

            bool foundSystem = false;
            foreach (string reference in _references)
            {
                if (reference.ToLower() == "mscorlib")
                {
                    foundSystem = true;
                }
            }
            if (!foundSystem)
            {
                _references.Add("mscorlib");
            }
            
            foreach (string reference in _references)
            {
                Assembly a;
                try
                {
                    a = Assembly.Load(reference);
                }
                catch (Exception e)
                {
                    a = Assembly.LoadFrom(reference);
                }
                
                //TODO: Get class names, assembly names, attribute classes and properties
                //assembly version and key info, etc
                InitializeTypes(a);
                //TODO: Implement other initializations
            }
        }

        private void InitializeTypes(Assembly a)
        {
            Type[] types = a.GetTypes();
            foreach (Type t in types)
            {
                string name = t.Name;
                string cilname = t.Module.Name;
                int p;
                if ((p = cilname.LastIndexOf(".dll")) > -1)
                {
                    cilname = cilname.Substring(0, p);
                }
                _assemblyName[name] = cilname;
                _assemblyName[t.FullName] = cilname;
                _types[name]=t;
                _types[t.FullName] = t;
                
                if (name.Length > 9 && name.Substring(name.Length - 9) == "Attribute")
                {
                    string att = name;
                    int dot = att.LastIndexOf(".");
                    if (dot>-1)
                    {
                        att = att.Substring(dot+1);
                    }
                    att = att.Substring(0,att.Length-9);
                    _attributes[att] = t.FullName;
                }
            }
        }

    }
}
