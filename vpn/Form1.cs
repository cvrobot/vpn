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
                //button1.Text = "Stop";
                button1.Enabled = false;
                startProcess();
                timer1.Start();

            }
            else {
                button1.Enabled = false;
                SendControlC(p.Id);
                p.Close();
                button1.Text = "Start";
                button1.Enabled = true;
            }
  
        }

        void startProcess()
        {
            SetConsoleCtrlHandler(null, false);

            p.StartInfo.FileName = "shadowvpn.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = "-c client.conf";
            p.StartInfo.Verb = "runas";
            p.Exited += P_Exited;
            p.Start();
        }

        private void P_Exited(object sender, EventArgs e)
        {
            if( button1.Text.StartsWith("Start"))//try starting
            {
                startProcess();//restart again
            }
            else
            {
                //normal exit
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(p.HasExited)
            {
                //startProcess();
            }
            else
            {
                button1.Text = "Stop";
                button1.Enabled = true;
                timer1.Stop();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (button1.Text.StartsWith("Stop"))//already start
                    SendControlC(p.Id);
            }
            catch {
            }
        }
    }
}
