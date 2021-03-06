﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapClient.Integration
{
    /// <summary>
    /// This class allows any types within MM to be located
    /// </summary>
    public static class MM_Type_Finder
    {
        #region Static namespace identification

        private static SortedDictionary<String, Assembly[]> _Namespaces = null;

        /// <summary>
        /// The list of connected namespaces
        /// </summary>
        public static SortedDictionary<String, Assembly[]> Namespaces
        {
            get
            {
                if (_Namespaces == null)
                {
                    _Namespaces = new SortedDictionary<string, Assembly[]>(StringComparer.CurrentCultureIgnoreCase);

                    List<Assembly> AssembliesToAnalyze = new List<Assembly>();

                    //First, make sure all of our referenced assemblies are loaded
                    AssembliesToAnalyze.Add(Assembly.GetCallingAssembly());
                    foreach (AssemblyName asmName in Assembly.GetCallingAssembly().GetReferencedAssemblies())
                        try
                        {
                            AssembliesToAnalyze.Add(Assembly.ReflectionOnlyLoad(asmName.FullName));
                        }
                        catch
                        {
                        }

                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        if (!AssembliesToAnalyze.Contains(asm))
                            AssembliesToAnalyze.Add(asm);

                    foreach (Assembly asm in AssembliesToAnalyze)
                        try
                        {
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
                        catch { }
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
            bool IsArray = TypeName.EndsWith("[]");
            if (IsArray)
                TypeName = TypeName.Substring(0, TypeName.Length - 2);

            if (TypeName == "float")
                TypeName = "Single";
            else if (TypeName == "int")
                TypeName = "Int32";
            //First, check through our enums.
            if (Enums != null)
                foreach (String EnumToCheck in Enums)
                    if (EnumToCheck == TypeName)
                        return typeof(Enum);

            //Now, check through our namespaces


            foreach (KeyValuePair<String, Assembly[]> Namespace in Namespaces)
                foreach (Assembly AsmToCheck in Namespace.Value)
                {
                    Type ThisType = AsmToCheck.GetType(Namespace.Key + "." + TypeName, false, true);
                    if (ThisType != null && FoundType != null)
                        throw new InvalidOperationException("Duplicate type found for " + TypeName + ": " + FoundType.FullName + " and " + ThisType.FullName);
                    else if (ThisType != null)
                        FoundType = ThisType;
                }



            if (FoundType == null)
                return null; //throw new InvalidOperationException("Unable to locate type " + TypeName);
            else if (IsArray)
                return Type.GetType(FoundType.FullName + "[]");
            else
                return FoundType;
        }
        #endregion
    }
}

