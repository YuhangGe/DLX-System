using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DlxExternalDevice;
using System.Threading;
using System.Diagnostics;
namespace ClassLibrary1
{
    public class ExternalDevice : ExternalDeviceBase
    {
        public ExternalDevice() { }
        public override int NumberOfCast()
        {
            return 1;
        }
        public override int AddressOfCast(int n)
        {
            return 3441243;
        }
        public delegate void eventdelegate(object sender,EventArgs e);
        public override event ValueEventDelegate ChangeValue = null;
        public override void ValueChanged(int address, int value)
        {
            MessageBox.Show(address +" "+value);
            if (ChangeValue != null)
                ChangeValue.Invoke(0,4);
        }
        public override int Run()
        {
            Application.Run(new Form1());
            
            return 0;
        }
        public override string DeviceName()
        {
            return "abc";
        }
        public override string Describle()
        {
            return "fdaslfjasdlfj";
        }
    }
}
