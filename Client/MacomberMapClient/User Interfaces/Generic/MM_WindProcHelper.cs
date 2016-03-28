using System;
using System.Runtime.InteropServices;

namespace MacomberMapClient.User_Interfaces.Generic
{
    public static class MM_WindProcHelper {
        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        public const Int32 _RenameSysMenuID = 1000;
        public const Int32 _AboutSysMenuID = 1001;

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
    }
}