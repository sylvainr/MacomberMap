using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacomberMapClient.User_Interfaces.Generic
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides static system interfaces, such as an input box, and console interactions for logging purposes
    /// </summary>
    public static class MM_System_Interfaces
    {
        #region Variable declarations
        /// <summary>A queue of events</summary>
        public static Queue<String> Events = new Queue<string>();

        /// <summary>The error log</summary>
        public static StreamWriter ErrorLog = null;

        /// <summary>Whether to activate the system console</summary>
        public static bool Console = false;
        #endregion

        public static bool Headless = false;

        #region Console access
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// Activate the system console on demand
        /// </summary>
        /// <returns>True</returns>
        private static bool ActivateConsole()
        {
            //If we're running in a command window, capture that.
            int ProcessId;
            GetWindowThreadProcessId(GetForegroundWindow(), out ProcessId);
            Process CurrentProcess = Process.GetProcessById(ProcessId);
            if (CurrentProcess.ProcessName == "cmd")
                AttachConsole(CurrentProcess.Id);
            //If not, create a new console window
            else
                AllocConsole();
            return true;
        }

        /// <summary>
        /// If our console is enabled, write text out to it
        /// </summary>
        /// <param name="ConsoleText">The text to write out to the console</param>
        public static void WriteConsoleLine(String ConsoleText)
        {
            if (Console)
            {
                System.Console.WriteLine(ConsoleText);
            }
            else
                Debug.WriteLine(ConsoleText);
        }

        /// <summary>
        /// Log a system error
        /// </summary>
        /// <param name="ex">The exception to be logged</param>
        public static void LogError(Exception ex)
        {
            if (Headless)
            {
                System.Console.WriteLine(ex.Message + " : " + ex.StackTrace);
                return;
            }

            StringBuilder OutMsg = new StringBuilder();
            int CurTab = 0;
            Exception CurEx = ex;

            do
            {
                OutMsg.AppendLine(new string('\t', CurTab) + "Error in " + CurEx.Source + ": " + CurEx.Message);
                String InLine;
                if (!String.IsNullOrEmpty(CurEx.StackTrace))
                    using (StringReader sRd = new StringReader(CurEx.StackTrace))
                        while (!String.IsNullOrEmpty(InLine = sRd.ReadLine()))
                            OutMsg.AppendLine(new string('\t', CurTab) + InLine);
                CurEx = CurEx.InnerException;
            } while (CurEx != null);
            LogError(OutMsg.ToString());
        }

        /// <summary>
        /// Log a system error.
        /// </summary>
        /// <param name="ErrorText">The text of the error</param>
        /// <param name="args">The formatting parameters</param>
        public static void LogError(string ErrorText, params object[] args)
        {
            if (Headless)
            {
                System.Console.WriteLine(ErrorText);
                return;
            }

            if (args != null && args.Length > 0)
                ErrorText = String.Format(ErrorText, args);
            WriteConsoleLine(ErrorText);
            //Helen
            if (Events.Count > 4000)
                Events.Clear();

            Events.Enqueue(ErrorText);
            if (ErrorLog != null)
                try
                {
                    ErrorLog.WriteLine(ErrorText);
                }
                catch (Exception)
                { }
        }


        #endregion

        #region Input box a-la VB
        /// <summary>
        /// Offer a simple input box, a-la VB
        /// </summary>
        /// <param name="title"></param>
        /// <param name="promptText"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            if (Headless)
                return DialogResult.Cancel;
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private delegate DialogResult SafeMessageBox(String Text, String Caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        /// <summary>
        /// Display a messagebox against the MM thread
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        public static DialogResult MessageBox(String text, String caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (Headless)
                return DialogResult.OK;
            Form ActiveForm = Form.ActiveForm;
            if (ActiveForm == null)
                return System.Windows.Forms.MessageBox.Show(text, caption, buttons, icon);
            else if (ActiveForm.InvokeRequired)
                return (DialogResult)ActiveForm.Invoke(new SafeMessageBox(MessageBox), text, caption, buttons, icon);
            else
                return System.Windows.Forms.MessageBox.Show(ActiveForm, text, caption, buttons, icon);
        }

        /// <summary>
        /// Kick off a messagebox in a separate thread
        /// </summary>
        /// <param name="state"></param>
        public static void MessageBoxInSeparateThread(Object state)
        {
            object[] InVal = (object[])state;
            if (Headless)
                return;
            using (Form TempForm = new Form())
            {
                TempForm.TopMost = true;
                TempForm.ShowInTaskbar = true;
                System.Windows.Forms.MessageBox.Show(TempForm, (string)InVal[0], (string)InVal[1], (MessageBoxButtons)InVal[2], (MessageBoxIcon)InVal[3]);
            }
        }
        #endregion

    }
}
