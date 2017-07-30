using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace vpn
{
    public partial class Form1 : Form
    {
        Process p;
        StreamWriter input;

        const int WM_CHAR = 0x0102;
        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        delegate bool HandlerRoutine(int dwCtrlType);
        [DllImport("Kernel32")]
        private static extern Boolean SetConsoleCtrlHandler(HandlerRoutine Handler, Boolean Add);
        [DllImport("user32.dll")]

        

        private static extern int SendMessage(IntPtr hwnd, int msg, int wParam, int lParam);

        [return: MarshalAs(UnmanagedType.Bool)]

        [DllImport("user32.dll", SetLastError = true)]

        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
        public Form1()
        {
            p = new Process();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            //System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            if (button1.Text.StartsWith("Start"))
            {
                SetConsoleCtrlHandler(null, false);
                button1.Text = "Stop";
                p.StartInfo.FileName = "shadowvpn.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                //input = p.StandardInput.
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = "-c client.conf";
                p.StartInfo.Verb = "runas";
                p.Start();
            }
            else {

                button1.Text = "Start";
                //p.StandardInput.WriteLine("\x3");
                //Thread.Sleep(50);
                //p.StandardInput.Close();
                //GenerateConsoleCtrlEvent(0,0);
                //p.CloseMainWindow();
                //SendMessage(p.MainWindowHandle, WM_CHAR, 0x03, 1);
                //TerminateProcess(p.Handle, 0);
                //p.Dispose();
                //SendKeys.SendWait("^(c)");
                //p.Kill();
                SendControlC(p.Id);
                //p.Dispose();
                p.Close();
            }
  
        }

        void SendControlC(int pid)
        {
            AttachConsole(pid); // attach to process console
            SetConsoleCtrlHandler(null, true); // disable Control+C handling for our app
            GenerateConsoleCtrlEvent(0, 0); // generate Control+C event
            p.WaitForExit(2000);
            FreeConsole();
        }

    }
}
