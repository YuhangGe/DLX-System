using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DlxExternalDevice;
using System.Windows;
using System.Threading;

namespace Simulate
{
    class DeviceControl
    {
        private static DeviceControl instance = null;
        public event ValueEventDelegate ChangeValue = null;
        private DeviceManage _deviceManage;
        public DeviceManage deviceManage
        {
            get { return this._deviceManage; }
        }

        protected DeviceControl()
        {
            _deviceManage = new DeviceManage();
            _deviceManage.AddEvent(new ValueEventDelegate(deviceManage_ChangeValue));
            _deviceManage.DeviceOff += new DeviceManage.EventDelegate(_deviceManage_DeviceOff);
        }

        void _deviceManage_DeviceOff(DeviceInfo s, int f, Exception ex)
        {
            if (ex is ThreadAbortException){
                MessageBox.Show("Device: \""+s.Name+"\" Exit."+Environment.NewLine+"Thread Abort!");
                return;
            }
            if (ex != null)
                MessageBox.Show("Device: \"" + s.Name + "\" Throws Exception:" + Environment.NewLine + ex.ToString());
            else
                MessageBox.Show("Device Exit: \"" + s.Name + "\"" + Environment.NewLine + "Style: " + f);
        }

        void deviceManage_ChangeValue(int v1, byte v2)
        {
            if (ChangeValue != null)
                ChangeValue.Invoke(v1, v2);
        }
        public static DeviceControl getInstance()
        {
            if (instance == null)
                instance = new DeviceControl();
            return instance;
        }
    }
}
