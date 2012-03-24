using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Reflection;
using DlxExternalDevice;
using System.Threading;

namespace WpfApplication2
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceWindow : Window
    {
        DeviceInfo device = null;

        Thread thread;

        DeviceManage deviceManage = new DeviceManage();

        public DeviceWindow()
        {
            InitializeComponent();
            this.deviceManage.DeviceOff += new DeviceManage.EventDelegate(deviceManage_DeviceOff);
        }

        void deviceManage_DeviceOff(DeviceInfo s, int f, Exception ex)
        {
            if (ex != null)
            {
                MessageBox.Show(s.Name+" Device Exception: " + ex.ToString());
                return;
            }
            else if (f != 0)
            {
                MessageBox.Show(s.Name+" Device Exit Failure: " + f);
                return;
            }
            else
            {
                MessageBox.Show(s.Name + " Device Exit");
            }
            
        }
        public delegate void p(int v1, int v2);
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();
            if (o.FileName != null)
            {
                device=DeviceInfo.GetDeviceInfo(o.FileName);
                if (device == null)
                {
                    MessageBox.Show("fail");
                    return;
                }
                
                String s=this.deviceManage.AddDevice(device);
                if (s != null)
                {
                    MessageBox.Show(s + " Address Conflict");
                }
                
             }
        }
        private void a(int a,int b)
        {
            MessageBox.Show(a+"aaaa"+b);
        }
        
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(device.Describe);
            this.deviceManage.DeviceRun(device);
        }

        private void message(string s)
        {
            MessageBox.Show(s);
        }
        private delegate void NULLP(string s);
        private void run()
        {
            int a=device.Run();
            if (a == 1)
                this.Dispatcher.Invoke(new NULLP(message), new object[] { "success" });
        }        
    
    
    }
}
