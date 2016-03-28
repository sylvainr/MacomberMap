using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.EMS
{
    /// <summary>
    /// This class provides the serialization tools for interprocess communications
    /// </summary>
    public static class MM_Serialization
    {
        /// <summary>
        /// Create and deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="HeaderInfo"></param>
        /// <param name="InLine"></param>
        /// <returns></returns>
        public static T Deserialize<T>(PropertyInfo[] HeaderInfo, String[] InLine)
        {
            T NewObj = Activator.CreateInstance<T>();
            return (T)Deserialize(HeaderInfo, InLine, NewObj);
        }
        
        /// <summary>
        /// Deserialize a line of input information to an object
        /// </summary>
        /// <param name="HeaderInfo"></param>
        /// <param name="InLine"></param>
        /// <param name="InObject"></param>
        public static object Deserialize(PropertyInfo[] HeaderInfo, String[] InLine, object InObject)
        {
            for (int a = 0; a < Math.Min(HeaderInfo.Length, InLine.Length); a++)
                if (HeaderInfo[a] != null)
                    try
                    {
                        if (HeaderInfo[a].PropertyType == typeof(bool))
                            HeaderInfo[a].SetValue(InObject, InLine[a].StartsWith("T", StringComparison.CurrentCultureIgnoreCase));
                        else if (HeaderInfo[a].PropertyType == typeof(DateTime))
                            HeaderInfo[a].SetValue(InObject, new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(InLine[a])));
                        else if (HeaderInfo[a].PropertyType.IsEnum)
                        {
                            if (!String.IsNullOrEmpty(InLine[a]))
                                HeaderInfo[a].SetValue(InObject, Enum.Parse(HeaderInfo[a].PropertyType, InLine[a], true));
                        }
                        else
                            HeaderInfo[a].SetValue(InObject, Convert.ChangeType(InLine[a], HeaderInfo[a].PropertyType));
                    }
            catch
                    {

                    }
            return InObject;
        }

        /// <summary>
        /// Deserialize a database object against database headers
        /// </summary>
        /// <param name="HeaderInfo"></param>
        /// <param name="dRd"></param>
        /// <param name="InObject"></param>
        /// <returns></returns>
        public static object Deserialize(PropertyInfo[] HeaderInfo, DbDataReader dRd, object InObject)
        {
            for (int a = 0; a < HeaderInfo.Length; a++)
                if (HeaderInfo[a] != null && dRd[a] != DBNull.Value)
                    HeaderInfo[a].SetValue(InObject, Convert.ChangeType(dRd[a], HeaderInfo[a].PropertyType));
            return InObject;
        }

        /// <summary>
        /// Deserialize a database object against database headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="HeaderInfo"></param>
        /// <param name="dRd"></param>
        /// <returns></returns>
        public static T Deserialize<T>(PropertyInfo[] HeaderInfo, DbDataReader dRd)
        {
            T OutObject = Activator.CreateInstance<T>();
            return (T)Deserialize(HeaderInfo, dRd, OutObject);
        }

        /// <summary>
        /// Build header info for an object/SQL match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dRd"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetHeaderInfo<T>(DbDataReader dRd)
        {
            return GetHeaderInfo(typeof(T), dRd);
        }

        /// <summary>
        /// Build header info for an object/SQL match
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="dRd"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetHeaderInfo(Type TargetType, DbDataReader dRd)
        {
            List<PropertyInfo> TargetFields = new List<PropertyInfo>();
            for (int a = 0; a < dRd.FieldCount; a++)
                TargetFields.Add(TargetType.GetProperty(dRd.GetName(a), BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public));
            return TargetFields.ToArray();
        }

        /// <summary>
        /// Retrieve header information from a header line to match to a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="HeaderLine"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetHeaderInfo<T>(String[] HeaderLine)
        {
            return GetHeaderInfo(typeof(T), HeaderLine);
        }


        /// <summary>
        /// Retrieve header information from a header line to match to a particular type
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="HeaderLine"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetHeaderInfo(Type TargetType, String[] HeaderLine)
        {
            List<PropertyInfo> TargetFields = new List<PropertyInfo>();
            for (int a = 0; a < HeaderLine.Length; a++)
                TargetFields.Add(TargetType.GetProperty(HeaderLine[a], BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public));
            return TargetFields.ToArray();
        }

        /// <summary>
        /// Build a SQL query based on the properties of a type
        /// </summary>
        /// <param name="WhereComponent"></param>
        /// <returns></returns>
        public static String BuildSQLQuery<T>(String WhereComponent = null)
        {
            StringBuilder sB = new StringBuilder();
            foreach (PropertyInfo pI in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public))
                if (pI.CanWrite)                    
                    sB.Append((sB.Length == 0 ? "SELECT " : ", ") + pI.Name);
            sB.Append(" FROM " + typeof(T).Name);
            if (!String.IsNullOrEmpty(WhereComponent))
                sB.Append(" WHERE " + WhereComponent);
            return sB.ToString();
        }
    }
}