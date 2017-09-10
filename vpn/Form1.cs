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
using Microsoft.Win32;
using Converter;
using RegistryTag;
using Aes;
//计算机\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\sysvpn
namespace sysvpn
{
    public partial class Form1 : Form
    {
        Process p;
        Thread th;
        RegistryHelper rh;
        AesHelp ah;
        bool deamon_status = false;
        bool auto_login = false;
        const string sysvpn = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\sysvpn";
        const string tagname = "tag";
        const string checkname = "check";
        const int WM_CHAR = 0x0102;
        delegate void my_delegate(int conn);
        my_delegate handle_conn;
        bool need_exit = false;
        Socket socket;
        IPEndPoint ipep;
        EndPoint Remote;


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

        void stop_deamon()
        {
            Process[] p = Process.GetProcessesByName("ShadowVPN");
            foreach (Process ps in p)
            {
                SendControlC(ps);
            }
        }

        Boolean wait_deamon_run(int retry, int ms)
        {
            Process[] p = null;
            while (retry > 0)
            {
                p = Process.GetProcessesByName("ShadowVPN");
                if (p != null)
                {
                    break;
                }
                Thread.Sleep(ms);
                retry--;
            }
            return retry > 0 ? true : false;
        }
        public Form1()
        {
            p = new Process();
            rh = new RegistryHelper();
            ah = new AesHelp();
            handle_conn = new my_delegate(conn_notify);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55152);
            Remote = (EndPoint)ipep;
            stop_deamon();
            //vpn_cmd_t cmd = new vpn_cmd_t();
            InitializeComponent();
        }

        private void conn_notify(int conn)
        {
            switch (conn) {
                case -1://request fail
                    label_status.Text = "Request fail";
                    break;
                case -2://token fail
                    label_status.Text = "Token not correct";
                    break;
                case -3://check conn fail
                    label_status.Text = "Connect fail";
                    break;
                case -4://error request
                    label_status.Text = "Request type error";
                    break;
                case -5://time out
                    label_status.Text = "Login time out";
                    break;
                case 1://login successfull
                    label_status.Text = "Login successfull";
                    break;
                case 2://logout successfull
                    label_status.Text = "Logout successfull";
                    break;
                case 3://connect successfull
                    label_status.Text = "Connect successfull";
                    break;
                case 4://exit successfull
                    label_status.Text = "Exit successfull";
                    break;
            }
            if (conn == 3 )
            {
                button1.Text = "Stop";
                button1.Enabled = true;
                save_token(conn);
                timer1.Stop();
            }
            else if (conn < 0) {
                SendControlC();
                button1.Enabled = true;
            }
        }

        private Byte[] strToByte(string str)
        {
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
        private Byte[] prepare_data(int cmd_type, string data)
        {
            byte[] buffer;
            vpn_cmd_t cmd = new vpn_cmd_t();
            int len;
            cmd.type = cmd_type;
            cmd.rsp = 0;
            cmd.uid = new byte[10];
            cmd.pwd = new byte[20];
            cmd.token = new byte[8];
            if (data != null) { 
                len = (data.Length / 2) > 8 ? 8 : textBox1.Text.Length / 2;
                Array.Copy(strToByte(textBox1.Text), cmd.token, len);
            }
            buffer = ConverterHelper.StructToBytes<vpn_cmd_t>(cmd);
            return buffer;
        }
        private void threadConn()
        {

            byte[] buffer;
            int conn = 0, retry = 10 ;

            List<Socket> socketList = new List<Socket>();

            int cmd_type = 1;
            Thread.Sleep(100);
            while (conn != 3 && conn >= 0){

                socketList.Clear();
                socketList.Add(socket);
                buffer = prepare_data(cmd_type, textBox1.Text);
                if(retry > 0)
                    socket.SendTo(buffer, Remote);

                Socket.Select(socketList, null, null, 500000);
                if(socketList.Count > 0)
                {
                    try
                    {
                        ((Socket)socketList[0]).Receive(buffer);
                    }
                    catch
                    {
                        //break;
                        Thread.Sleep(500);
                        continue;
                    }
                    vpn_cmd_t cmd = (vpn_cmd_t)ConverterHelper.BytesToStruct<vpn_cmd_t>(buffer);
                    if(cmd.rsp != 0){
                        if(cmd.rsp == -1)
                        {
                            retry--;
                            Thread.Sleep(500);
                        }else
                            conn = cmd.rsp;
                        if (cmd.type == 1 && cmd.rsp == 1)
                        {
                            cmd_type = 3;
                            Thread.Sleep(500);
                        }
                    }
                }

                if(conn != 0)//if (conn == 3 || conn == -2)
                {
                    try
                    {
                        this.Invoke(handle_conn, conn);
                    }
                    catch
                    {
                        break;
                    }
                }
                if (need_exit)
                    break;
            }
        }

        void save_token(int conn)
        {
            string token = textBox1.Text;
            if (auto_login == false && conn == 3)//auto login not set, means token is set by user 
            {
                token = ah.EncryptByAES(token, tagname);
                rh.SetRegistryData(Registry.LocalMachine, sysvpn, tagname, token);
            }
        }
        private void vpn_start()
        {
            if (button1.Text.StartsWith("Start"))
            {
                //button1.Text = "Stop";
                button1.Enabled = false;
                if (deamon_status == false)
                {
                    startProcess();
                }

                if (th == null || th.IsAlive == false)
                {
                    th = new Thread(new ThreadStart(threadConn));
                    th.Start();
                    timer1.Interval = 15000;
                    timer1.Start();
                }
            }
            else
            {
                button1.Enabled = false;
                SendControlC(p);
                p.Close();
                Thread.Sleep(100);
                button1.Text = "Start";
                label_status.Text = "";
               button1.Enabled = true;
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            vpn_start();
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
            p.StartInfo.Arguments = "-c client.conf -v";
            p.StartInfo.Verb = "runas";
            p.Exited += P_Exited;
            p.Start();
            wait_deamon_run(10,100);
            deamon_status = true;
        }

        private void P_Exited(object sender, EventArgs e)
        {
            if( button1.Text.StartsWith("Start"))//try starting
            {
                startProcess();//restart again
            }
            else
            {
                deamon_status = false;
                button1.Enabled = true;
                //normal exit
            }
        }

        void SendControlC(Process p = null)
        {
            int cmd_type = 4;//exit
            byte[] buffer = prepare_data(cmd_type, null);
            socket.SendTo(buffer, Remote);
            deamon_status = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            conn_notify(-5);
            button1.Text = "Start";
            button1.Enabled = true;
            timer1.Stop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                need_exit = true;
                if (button1.Text.StartsWith("Stop"))//already start
                    SendControlC(p);
                else
                    SendControlC();
            }
            catch {
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string tag, key,check;

            if (rh.IsRegistryKeyExist(Registry.LocalMachine, sysvpn, tagname))
            {
                tag = rh.GetRegistryData(Registry.LocalMachine, sysvpn, tagname);
                key = ah.DecryptByAES(tag, tagname);
                if (key.Length != 0)
                    textBox1.Text = key;
            }
            if (rh.IsRegistryKeyExist(Registry.LocalMachine, sysvpn, checkname))
            {
                check = rh.GetRegistryData(Registry.LocalMachine, sysvpn, checkname);
                checkBox1.Checked = Convert.ToBoolean(check);
                checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            }
            if (checkBox1.Checked && textBox1.Text.Length != 0)
            {
                auto_login = true;
                vpn_start();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string check = Convert.ToString(checkBox1.Checked);
            rh.SetRegistryData(Registry.LocalMachine, sysvpn, checkname, check);
        }
        /*
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) ||
                (e.KeyCode == Keys.Back) ||
                (e.KeyCode == Keys.Enter))
            {
                if (e.KeyCode == Keys.Enter)
                    vpn_start();
                
            }else
                e.Handled = true;
        }*/

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Char.IsNumber(e.KeyChar)) ||
                (e.KeyChar >= 'A' && e.KeyChar <= 'F') ||
                (e.KeyChar >= 'a' && e.KeyChar <= 'f') ||
                (e.KeyChar == (char)Keys.Back) ||
                (e.KeyChar == (char)Keys.Enter))
            {
                if (e.KeyChar == (char)Keys.Enter)
                    vpn_start();
                //e.Handled = false;
            }
            else if (e.KeyChar == (char)22)
            {
                try
                {
                    strToByte(Clipboard.GetText());//检查是否16进制数字
                    //Convert.ToInt64(Clipboard.GetText());  
                    Clipboard.SetText(Clipboard.GetText().Trim()); //去空格
                }
                catch (Exception)
                {
                    e.Handled = true;
                    MessageBox.Show("只能输入数字及A~F");
                }
            }
            else
            {
                e.Handled = true;
                MessageBox.Show("只能输入数字及A~F");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 16)
                button1.Enabled = true;
            else
                button1.Enabled = false;

        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
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
        ushort reserved;
        UInt32 client_ip;
        public int rsp;//OK if it is same as type
    };
}
