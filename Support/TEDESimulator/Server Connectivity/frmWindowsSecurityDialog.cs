using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator
{
    /// <summary>
    /// This class, based on http://stackoverflow.com/questions/10242067/windows-security-custom-login-validation, provides a Windows 7/Vista secure authentication dialog
    /// </summary>
    public class frmWindowsSecurityDialog
    {
        /// <summary>The caption text</summary>
        public string CaptionText { get; set; }

        /// <summary>The text of the message</summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Free memory
        /// </summary>
        /// <param name="ptr"></param>
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }


        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                   IntPtr pAuthBuffer,
                                                                   uint cbAuthBuffer,
                                                                   StringBuilder pszUserName,
                                                                   ref int pcchMaxUserName,
                                                                   StringBuilder pszDomainName,
                                                                   ref int pcchMaxDomainame,
                                                                   StringBuilder pszPassword,
                                                                   ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);

        /// <summary>
        /// Show a login dialog
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="Domain"></param>
        /// <returns></returns>
        public bool ShowLoginDialog(out string Username, out String Password, out String Domain)
        {
            var credui = new CREDUI_INFO { pszCaptionText = CaptionText, pszMessageText = MessageText };
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer;
            uint outCredSize;
            bool save = false;
            var authError = 0;

            var result = CredUIPromptForWindowsCredentials(ref credui, authError, ref authPackage, IntPtr.Zero, 0, out outCredBuffer, out outCredSize, ref save, 0x201);//0x202 /* Generic */);
            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            var maxUserName = 100;
            var maxDomain = 100;
            var maxPassword = 100;
            if (result == 0 && CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName, domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
            {
                CoTaskMemFree(outCredBuffer);
                Username = usernameBuf.ToString();
                Password = passwordBuf.ToString();
                Domain = domainBuf.ToString();
                return true;
            }
            else
            {
                Username = Password = Domain = null;
                return false;
            }
        }
    }
}