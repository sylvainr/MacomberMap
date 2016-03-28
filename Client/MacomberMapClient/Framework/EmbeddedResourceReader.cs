using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MacomberMapClient.Framework
{
    /// <summary>
    /// Read the contents of an embedded resource and provide caching options
    /// </summary>
    public class EmbeddedResourceReader
    {
        static Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// Get a chached value of a resource or load it from the manifest and add it to the cache.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static string GetResource(string resourceName)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(resourceName))
                    return _cache[resourceName];

                // else
                var res = ReadTextResource(resourceName);
                _cache.Add(resourceName, res);
                return res;
            }
        }

        /// <summary>
        /// Read an embedded resource without caching it.
        /// </summary>
        /// <param name="resourceName">The path to the resource to read.</param>
        /// <param name="assembly">The assembly to read from. If null uses calling assembly.</param>
        /// <returns>Resource contents as string.</returns>
        public static string ReadTextResource(string resourceName, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            //var resourceName = "AssemblyName.MyFile.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// List of available resources in an assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan. If null uses calling assembly.</param>
        /// <returns>string array of resource names</returns>
        public static string[] GetResourceNames(Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceNames();
        }
    }
}
