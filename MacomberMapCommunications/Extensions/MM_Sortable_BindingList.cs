using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Extensions
{
    /// <summary>
    /// (C) 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides an extension to support a sortable binding list. With thanks to http://stackoverflow.com/questions/1063917/bindinglistt-sort-to-behave-like-a-listt-sort
    /// </summary>
    public static class MM_Sortable_BindingList
    {


        /// <summary>
        /// Sort ascending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="bindingList"></param>
        /// <param name="sortProperty"></param>
        public static void SortAscending<T, P>(this BindingList<T> bindingList, Func<T, P> sortProperty)
        {
            bindingList.Sort(null, (a, b) => ((IComparable<P>)sortProperty(a)).CompareTo(sortProperty(b)));
        }

        /// <summary>
        /// Sort descending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="bindingList"></param>
        /// <param name="sortProperty"></param>
        public static void SortDescending<T, P>(this BindingList<T> bindingList, Func<T, P> sortProperty)
        {
            bindingList.Sort(null, (a, b) => ((IComparable<P>)sortProperty(b)).CompareTo(sortProperty(a)));
        }

        /// <summary>
        /// Sort with a default comparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingList"></param>
        public static void Sort<T>(this BindingList<T> bindingList)
        {
            bindingList.Sort(null, null);
        }


        /// <summary>
        /// Sort with a specified comparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingList"></param>
        /// <param name="comparer"></param>
        public static void Sort<T>(this BindingList<T> bindingList, IComparer<T> comparer)
        {
            bindingList.Sort(comparer, null);
        }

        /// <summary>
        /// Sort with a specified comparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingList"></param>
        /// <param name="comparison"></param>
        public static void Sort<T>(this BindingList<T> bindingList, Comparison<T> comparison)
        {
            bindingList.Sort(null, comparison);
        }

        /// <summary>
        /// Sort with a specified comparer and comparison
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingList"></param>
        /// <param name="p_Comparer"></param>
        /// <param name="p_Comparison"></param>
        private static void Sort<T>(this BindingList<T> bindingList, IComparer<T> p_Comparer, Comparison<T> p_Comparison)
        {

            //Extract items and sort separately
            List<T> sortList = new List<T>();
            bindingList.ForEach(item => sortList.Add(item));//Extension method for this call
            if (p_Comparison == null)
            {
                sortList.Sort(p_Comparer);
            }//if
            else
            {
                sortList.Sort(p_Comparison);
            }//else

            //Disable notifications, rebuild, and re-enable notifications
            bool oldRaise = bindingList.RaiseListChangedEvents;
            bindingList.RaiseListChangedEvents = false;
            try
            {
                bindingList.Clear();
                sortList.ForEach(item => bindingList.Add(item));
            }
            finally
            {
                bindingList.RaiseListChangedEvents = oldRaise;
                bindingList.ResetBindings();
            }

        }

        /// <summary>
        /// Produce a sorted list that can be enumerated through
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (T item in source)
                action(item);
        }
    }
}