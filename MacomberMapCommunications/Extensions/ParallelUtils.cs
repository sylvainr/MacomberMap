using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Extensions
{
    /// <summary>
    /// Extension class for doing operations in parallel
    /// </summary>
    public class ParallelUtils
    {
        public static void While(Func<bool> condition, Action body)
        {
            Parallel.ForEach(IterateUntilFalse(condition), ignored => body());
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (condition()) yield return true;
        }
    }
}