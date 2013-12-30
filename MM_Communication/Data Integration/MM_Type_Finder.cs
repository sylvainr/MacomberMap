using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MM_Communication.Data_Integration
{
    /// <summary>
    /// This class allows any types within MM to be located
    /// </summary>
    public static class MM_Type_Finder
    {
        #region Static namespace identification

        private static Dictionary<String, Assembly[]> _Namespaces = null;

        /// <summary>
        /// The list of connected namespaces
        /// </summary>
        public static Dictionary<String, Assembly[]> Namespaces
        {
            get
            {
                if (_Namespaces == null)
                {
                    _Namespaces = new Dictionary<string, Assembly[]>();
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        foreach (Type asmType in asm.GetTypes())
                            if (asmType.Namespace != null)
                                if (!_Namespaces.ContainsKey(asmType.Namespace))
                                    _Namespaces.Add(asmType.Namespace, new Assembly[] { asm });
                                else
                                {
                                    bool FoundAsm = false;
                                    foreach (Assembly TestAsm in _Namespaces[asmType.Namespace])
                                        if (TestAsm == asm)
                                        {
                                            FoundAsm = true;
                                            break;
                                        }
                                    if (!FoundAsm)
                                    {
                                        Assembly[] OutAssembly = new Assembly[_Namespaces[asmType.Namespace].Length + 1];
                                        _Namespaces[asmType.Namespace].CopyTo(OutAssembly, 1);
                                        OutAssembly[0] = asm;
                                        _Namespaces[asmType.Namespace] = OutAssembly;
                                    }
                                }
                }
                return _Namespaces;
            }
        }
        #endregion


        #region Type location
        /// <summary>
        /// Locate a type by its name
        /// </summary>
        /// <param name="TypeName">The type to search for</param>
        /// <param name="Enums">Any custom enumerations (if any)</param>
        /// <returns></returns>
        public static Type LocateType(string TypeName, IEnumerable<String> Enums)
        {
            Type FoundType = null;

            //First, check through our enums.
            if (Enums != null)
                foreach (String EnumToCheck in Enums)
                    if (EnumToCheck == TypeName)
                        return typeof(Enum);

            //Now, check through our namespaces
            foreach (KeyValuePair<String, Assembly[]> Namespace in Namespaces)
                foreach (Assembly AsmToCheck in Namespace.Value)
                {
                    Type ThisType = AsmToCheck.GetType(Namespace.Key + "." + TypeName);
                    if (ThisType != null && FoundType != null)
                        throw new InvalidOperationException("Duplicate type found for " + TypeName + ": " + FoundType.FullName + " and " + ThisType.FullName);
                    else if (ThisType != null)
                        FoundType = ThisType;
                }
            if (FoundType == null)
                throw new InvalidOperationException("Unable to locate type " + TypeName);
            else
                return FoundType;
        }
        #endregion
    }
}

