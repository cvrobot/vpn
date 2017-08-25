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
using System.Net;
using System.Net.Sockets;
using Converter
using Registry
using Aes

namespace vpn
{
    public partial class Form1 : Form
    {
        Process p;
        StreamWriter input;
        Thread th;
        bool conn = false;
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
            th = new Thread(new ThreadStart(threadConn));
            vpn_cmd_t cmd = new vpn_cmd_t();
            MessageBox.Show(Marshal.SizeOf(cmd).ToString()); 
            InitializeComponent();
        }

        private void threadConn()
        {
            Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55151);
            EndPoint Remote = (EndPoint)ipep;

            List<Socket> socketList = new List<Socket>();
            socketList.Add(socket);
            vpn_cmd_t cmd = new vpn_cmd_t();
            cmd.type = 1;
            cmd.rsp = 0;
            cmd.uid = new byte[10];
            cmd.pwd = new byte[20];
            cmd.token = new byte[8];
            Array.Copy(Encoding.ASCII.GetBytes(textBox1.Text), cmd.token, 8);
            byte[] buffer = ConverterHelper.StructToBytes<vpn_cmd_t>(cmd);

            while (button1.Text.StartsWith("Start")){
                Thread.Sleep(100);
                cmd.rsp = 0;
                socketList.Clear();
                socketList.Add(socket);
                //cmd.ToString();
                socket.SendTo(Encoding.ASCII.GetBytes(textBox1.Text), Remote);

                Socket.Select(socketList, null, null, 500);
                if(socketList.Count > 0)
                {
                    ((Socket)socketList[0]).Receive(buffer);
                    cmd = (vpn_cmd_t)ConverterHelper.BytesToStruct<vpn_cmd_t>(buffer);
                    if(cmd.rsp != 0){
                        MessageBox.Show(cmd.type.ToString() + cmd.rsp.ToString());
                        if(cmd.rsp == 3){
                            conn = true;
                        }
                        cmd.rsp = 0;
                    }
                }
                if (conn == true)
                {
                    button1.Text = "Stop";
                    button1.Enabled = true;
                }
            }
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
                th.Start();

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct vpn_cmd_t{
        public int type;//1:login,2:logout,3:exit
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] uid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] pwd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] token;
        UInt32 client_ip;
        public int rsp;//OK if it is same as type
    };
}
