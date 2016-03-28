using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace MacomberMapCommunications
{
    /// <summary>
    /// This class holds a UI helper, that tracks CPU and memory usage
    /// </summary>
    public class MM_System_Profiling
    {      

        #region Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out ComTypes.FILETIME lpIdleTime, out ComTypes.FILETIME lpKernelTime, out ComTypes.FILETIME lpUserTime);

        /// <summary>
        /// The memory status class
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MEMORYSTATUSEX
        {

            /// <summary></summary>
            public uint dwLength;

            /// <summary></summary>
            public uint dwMemoryLoad;

            /// <summary></summary>
            public ulong ullTotalPhys;

            /// <summary></summary>
            public ulong ullAvailPhys;

            /// <summary></summary>
            public ulong ullTotalPageFile;

            /// <summary></summary>
            public ulong ullAvailPageFile;

            /// <summary></summary>
            public ulong ullTotalVirtual;

            /// <summary></summary>
            public ulong ullAvailVirtual;

            /// <summary></summary>
            public ulong ullAvailExtendedVirtual;

            /// <summary>Initialize a new memory status</summary>
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
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
        private static long _lastWorkingSet;
        #endregion

        #region Constants
        private const uint GENERIC_READ = (0x80000000);
        private const uint GENERIC_WRITE = (0x40000000);
        private const uint OPEN_EXISTING = 3;
        private const int BUFFER_SIZE = 4096;
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        private static MEMORYSTATUSEX _lastmemStatus=new MEMORYSTATUSEX();
        #endregion

        

        /// <summary>
        /// Determine the current CPU usage in percent
        /// </summary>
        /// <param name="WorkingSet">The working set</param>
        /// <param name="memStatus"></param>
        /// <returns></returns>
        public static short GetCPUUsage(out long WorkingSet, out MEMORYSTATUSEX memStatus)
        {
            short cpuCopy = _cpuUsage;
            if (Interlocked.Increment(ref _runCount) == 1)
            {
                if (!EnoughTimePassed)
                {
                    Interlocked.Decrement(ref _runCount);
                    WorkingSet = _lastWorkingSet;
                    memStatus = _lastmemStatus;
                    return cpuCopy;
                }

                ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                TimeSpan procTime;

                using (Process process = Process.GetCurrentProcess())
                {
                    procTime = process.TotalProcessorTime;
                    WorkingSet = _lastWorkingSet = process.WorkingSet64;
                    memStatus = _lastmemStatus = new MEMORYSTATUSEX();
                    GlobalMemoryStatusEx(memStatus);
                }

                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                {
                    Interlocked.Decrement(ref _runCount);
                    WorkingSet = _lastWorkingSet;
                    return cpuCopy;
                }

                if (!IsFirstRun)
                {

                    UInt64 sysKernelDiff = SubtractTimes(sysKernel, _prevSysKernel);
                    UInt64 sysUserDiff = SubtractTimes(sysUser, _prevSysUser);
                    UInt64 sysTotal = sysKernelDiff + sysUserDiff;
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
            else
            {
                WorkingSet = _lastWorkingSet;
                memStatus = _lastmemStatus;
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
        private static UInt64 SubtractTimes(ComTypes.FILETIME a, ComTypes.FILETIME b)
        {
            UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32)) | (UInt64)a.dwLowDateTime;
            UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32)) | (UInt64)b.dwLowDateTime;
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


        public static bool IsVirtualMachine()
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var items = searcher.Get())
                {
                    foreach (var item in items)
                    {
                        string manufacturer = item["Manufacturer"].ToString().ToLower();
                        if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                            || manufacturer.Contains("vmware")
                            || item["Model"].ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsTerminalSession()
        {
            return SystemInformation.TerminalServerSession;
        }

        public static bool IsTerminalOrVirtual()
        {
            if (IsTerminalSession())
                return true;
            if (IsVirtualMachine())
                return true;

            return false;
        }
    }
}
