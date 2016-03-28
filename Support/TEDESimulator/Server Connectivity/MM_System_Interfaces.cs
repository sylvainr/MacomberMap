using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Server_Connectivity
{
   public static class MM_System_Interfaces
    {
        /// <summary>
        /// Log a system error.
        /// </summary>
        /// <param name="ErrorText">The text of the error</param>
        /// <param name="args">The formatting parameters</param>
        public static void LogError(string ErrorText, params object[] args)
        {
            System.Console.WriteLine(ErrorText,args);
        }
    }
}
