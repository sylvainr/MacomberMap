using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacomberMapClient.Integration
{
    /// <summary>
    /// This class is responsible for performing core conversions
    /// </summary>
    public static class MM_Converter
    {
        /// <summary>
        /// Retrieve a single-precision number, first checking for NaN
        /// </summary>
        /// <param name="Incoming">The incoming value</param>
        /// <returns></returns>
        public static Single ToSingle(Object Incoming)
        {
            if (Incoming is DBNull)
                return float.NaN;
            else if (Incoming is Single)
                return (Single)Incoming;
            else if (Incoming is Decimal)
                return Convert.ToSingle(Incoming);
            else if ((String)Incoming == "NaN")
                return float.NaN;
            else
                    return Convert.ToSingle((String)Incoming);              
        }

        /// <summary>
        /// Convert an object to a Int32 (needed for TEIDs) if possible
        /// </summary>
        /// <param name="InValue">Our incoming value</param>
        /// <param name="OutVal">Our outgoing value</param>
        /// <returns>Whether the import was succesful</returns>
        public static bool ToInt32(object InValue, out Int32 OutVal)
        {
            if (InValue is Int32)
            {
                OutVal = Convert.ToInt32(InValue);
                return true;
            }
            else if (InValue is Decimal)
            {
                OutVal = Convert.ToInt32(InValue);
                return true;
            }
            else
            {
                OutVal = 0;
                return false;
            }

        }

        #region Case changing
        /// <summary>
        /// Convert text so that the first letter of each word is capitalized
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        public static string TitleCase(string InString)
        {
            if (InString == null)
                return null;

            //Start with a standard current UI culture title case string
            TextInfo CurText = CultureInfo.CurrentUICulture.TextInfo;

            //Run our RegEx tests to determine whether we have Mc/Mac, salutations or roman numerals (based on http://www.codeproject.com/KB/string/ProperCaseFormatProvider.aspx)
            String McAndMac = @"^(ma?c)(?!s[ead]$)((.+))$";
            String RomanNumerals = @"^((?=[MDCLXVI])((M{0,3})((C[DM])|(D?C{0,3}))?((X[LC])|(L?XX{0,2})|L)?((I[VX])|(V?(II{0,2}))|V)?)),?$";

            //Now, run through each of our words, and handle accordingly
            List<string> OutStrings = new List<string>();
            foreach (String Word in InString.ToLower().Split(' '))
                if (Regex.IsMatch(Word, RomanNumerals, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    OutStrings.Add(CurText.ToUpper(Word));
                else if (Regex.IsMatch(Word, McAndMac, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    Match MatchedToken = Regex.Match(Word, McAndMac, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    OutStrings.Add(MatchedToken.Groups[1].Value.Substring(0, 1).ToUpperInvariant() + MatchedToken.Groups[1].Value.Substring(1).ToLowerInvariant() + MatchedToken.Groups[2].Value.Substring(0, 1).ToUpperInvariant() + MatchedToken.Groups[2].Value.Substring(1).ToLowerInvariant());
                }
                else
                    OutStrings.Add(CurText.ToTitleCase(Word));
            return String.Join(" ", OutStrings.ToArray());
        }
        #endregion
    }
}
