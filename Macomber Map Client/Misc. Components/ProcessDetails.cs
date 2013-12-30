using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Diagnostics;
namespace Macomber_Map.Misc._Components
{
    /// <summary>
    /// This class provides details of CPU and memory usage on the current process    
    /// Adapted from http://www.philosophicalgeek.com/2009/01/03/determine-cpu-usage-of-current-process-c-and-c/
    /// </summary>
    public static class ProcessDetails
    {
        #region Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out ComTypes.FILETIME lpIdleTime, out ComTypes.FILETIME lpKernelTime, out ComTypes.FILETIME lpUserTime);
        #endregion

        #region Variable declarations
        /// <summary>The previous system kernel usage</summary>
        private static ComTypes.FILETIME _prevSysKernel;

        /// <summary>The previous system user usage</summary>
        private static ComTypes.FILETIME _prevSysUser;

        private static TimeSpan _prevProcTotal;
        private static Int16 _cpuUsage;
        private static DateTime _lastRun;
        private static long _runCount;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our CPU monitoring system
        /// </summary>
        public static void Initialize()
        {
            _cpuUsage = -1;
            _lastRun = DateTime.MinValue;
            _prevSysUser.dwHighDateTime = _prevSysUser.dwLowDateTime = 0;
            _prevSysKernel.dwHighDateTime = _prevSysKernel.dwLowDateTime = 0;
            _prevProcTotal = TimeSpan.MinValue;
            _runCount = 0;
        }
        #endregion

        #region CPU usage retrieval
        /// <summary>
        /// Determine the current CPU usage in percent
        /// </summary>
        /// <returns></returns>
        public static short GetUsage()
        {
            short cpuCopy = _cpuUsage;
            if (Interlocked.Increment(ref _runCount) == 1)
            {
                if (!EnoughTimePassed)
                {
                    Interlocked.Decrement(ref _runCount);
                    return cpuCopy;
                }

                ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                TimeSpan procTime;

                Process process = Process.GetCurrentProcess();
                procTime = process.TotalProcessorTime;

                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                {
                    Interlocked.Decrement(ref _runCount);
                    return cpuCopy;
                }

                if (!IsFirstRun)
                {

                    Int32 sysKernelDiff = SubtractTimes(sysKernel, _prevSysKernel);
                    Int32 sysUserDiff = SubtractTimes(sysUser, _prevSysUser);
                    Int32 sysTotal = sysKernelDiff + sysUserDiff;
                    Int64 procTotal = procTime.Ticks - _prevProcTotal.Ticks;
                    if (sysTotal > 0)
                        _cpuUsage = (short)((100.0 * procTotal) / sysTotal);
                }

                _prevProcTotal = procTime;
                _prevSysKernel = sysKernel;
                _prevSysUser = sysUser;
                _lastRun = DateTime.Now;
                cpuCopy = _cpuUsage;
            }
            Interlocked.Decrement(ref _runCount);
            return cpuCopy;
        }

        /// <summary>
        /// Subtract two times
        /// </summary>
        /// <param name="a">Time 1</param>
        /// <param name="b">Time 2</param>
        /// <returns></returns>
        private static Int32 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
        {
            Int32 aInt = ((Int32)(a.dwHighDateTime << 32)) | (Int32)a.dwLowDateTime;
            Int32 bInt = ((Int32)(b.dwHighDateTime << 32)) | (Int32)b.dwLowDateTime;
            return aInt - bInt;
        }

        /// <summary>
        /// Determine whether enough time has passed for an accurate reading
        /// </summary>
        private static bool EnoughTimePassed
        {
            get
            {
                const int minimumElapsedMS = 250;
                TimeSpan sinceLast = DateTime.Now - _lastRun;
                return sinceLast.TotalMilliseconds > minimumElapsedMS;
            }
        }

        /// <summary>
        /// Determine whether this is the first CPU run
        /// </summary>
        private static bool IsFirstRun
        {
            get { return (_lastRun == DateTime.MinValue); }
        }
        #endregion
    }
}
