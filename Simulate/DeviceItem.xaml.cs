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
using DlxExternalDevice;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace Simulate
{
    /// <summary>
    /// DeviceItem.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceItem : UserControl
    {

        public delegate void EventDelegate(DeviceItem sender, DeviceInfo device);
        public event EventDelegate ItemClick, CloseClick;

        private DeviceInfo _device;
        public DeviceInfo device
        {
            get
            {
                return _device;
            }
            set
            {
                this._device = value;
                this.Name.Content = value.Name;
                
            }
        }

        private bool _choose;
        public bool Choose
        {
            get { return _choose; }
            set
            {
                _choose = value;
                if (value)
                {
                    Background.Style = (Style)this.FindResource("Selected");
                }
                else
                {
                    Background.Style = (Style)this.FindResource("UnSelected");
                }
            }
        }

        private bool _run = false;
        public bool run
        {
            get { return _run; }
            set
            {
                if (value)
                {
                    Sign.Style = (Style)this.FindResource("RunSign");
                    rectangle1.Style = (Style)this.FindResource("RunButton");
                    _run = value;
                }
                else
                {
                    Sign.Style = (Style)this.FindResource("UnRunSign");
                    rectangle1.Style = (Style)this.FindResource("UnRunButton");
                    _run = value;
                }
            }
        }

        Thread TextPowerOn;

        public DeviceItem()
        {
            InitializeComponent();
            this.MouseDown += new MouseButtonEventHandler(DeviceItem_MouseDown);
            this.SwitchCanvas.MouseDown += new MouseButtonEventHandler(SwitchCanvas_MouseDown);
            this.CloseCanvas.MouseDown += new MouseButtonEventHandler(CloseCanvas_MouseDown);
            TextPowerOn = new Thread(new ThreadStart(ThreadFun));
            TextPowerOn.IsBackground = true;
            TextPowerOn.Start();
        }

        private delegate void BOOLPA(bool b);
        void ThreadFun()
        {
            bool r = false;
            while (true)
            {
                Thread.Sleep(20);
                if (this._device != null)
                {
                    if (r != _device.PowerOn)
                    {
                        r = _device.PowerOn;
                        this.Dispatcher.Invoke(new BOOLPA(ChangeRun), device.PowerOn);
                    }
                }
            }
        }
        private void ChangeRun(bool b)
        {
           
            this.run = b;
        }

        void CloseCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.OnClose();
        }

        void DeviceItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemClick != null)
                ItemClick.Invoke(this, this._device);
        }

        void SwitchCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (device.PowerOn)
            {
                device.Off();
            }
            else
            {
                DeviceControl.getInstance().deviceManage.DeviceRun(device);
            }
        }

        public void OnClose()
        {
            if (CloseClick != null)
            {
                this.CloseClick.Invoke(this, this._device);
                try
                {
                    this.TextPowerOn.Abort();
                }
                catch (Exception ex) { }
            }
        }
    }
}
