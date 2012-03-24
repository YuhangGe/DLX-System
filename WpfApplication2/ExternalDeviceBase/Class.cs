using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace DlxExternalDevice
{
    public delegate void ValueEventDelegate(int v1,int v2);
    public abstract class ExternalDeviceBase
    {
        public abstract String DeviceName();
        public abstract int NumberOfCast();
        public abstract int AddressOfCast(int n);
        public abstract void ValueChanged(int address, int value);
        public abstract event ValueEventDelegate ChangeValue;
        public abstract int Run();
        public abstract String Describle();
    }
    public class DeviceInfo
    {
        private String _Name;
        public String Name
        {
            get
            {
                return _Name;
            }
        }
        
        private Object _Instance;
        public Object Instance
        {
            get
            {
                return _Instance;
            }
        }

        private int _NumberOfCast;
        public int NumberOfCast
        {
            get
            {
                return _NumberOfCast;
            }
        }

        private int[] _AddressOfCast;
        public int[] AddressOfCast
        {
            get
            {
                return _AddressOfCast;
            }
        }

        private String _Describe;
        public String Describe
        {
            get { return _Describe; }
        }

        private Type _DeviceType;
        public Type DeviceType
        {
            get { return _DeviceType; }
        }

        public bool PowerOn = false;

        public Thread RunThread = null;

        public ValueEventDelegate ChangeValue = null;
        public override string ToString()
        {
            return base.ToString()+": "+_Name+" "+_NumberOfCast+" "+_Instance+" "+_DeviceType;
        }
        
        public static DeviceInfo GetDeviceInfo(string path)
        {
            try
            {
                Assembly assem = Assembly.LoadFile(path);
                Type[] tps = assem.GetTypes();
                Type tp = null;
                for (int i = 0; i < tps.Length; i++)
                {
                    if (tps[i].Name == "ExternalDevice")
                    {
                        tp = tps[i];
                        break;
                    }
                    if (i == tps.Length - 1)
                        return null;
                }
                if (tp.BaseType != typeof(ExternalDeviceBase))
                    return null;
                Object instance = Activator.CreateInstance(tp);
                MethodInfo mi = tp.GetMethod("DeviceName");
                DeviceInfo deviceInfo = new DeviceInfo();
                deviceInfo._DeviceType = tp;
                deviceInfo._Instance = instance;
                deviceInfo._Name = (String)mi.Invoke(instance, new Object[] { });
                mi = tp.GetMethod("Describle");
                deviceInfo._Describe = (String)mi.Invoke(instance, new Object[] { });
                mi = tp.GetMethod("NumberOfCast");
                deviceInfo._NumberOfCast = (int)mi.Invoke(instance, new Object[] { });
                deviceInfo._AddressOfCast = new int[deviceInfo._NumberOfCast];
                mi = tp.GetMethod("AddressOfCast");
                for (int i = 0; i < deviceInfo._NumberOfCast; i++)
                {
                    deviceInfo._AddressOfCast[i] = (int)mi.Invoke(instance, new Object[] { i });
                }
                return deviceInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
        
        public bool AddEventHandle(ValueEventDelegate ved)
        {
            try
            {
                this.ChangeValue = ved;
                EventInfo ei = _DeviceType.GetEvent("ChangeValue");
                ei.AddEventHandler(_Instance, new ValueEventDelegate(this.Function));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
        private void Function(int address, int value)
        {
            if (this.ChangeValue != null)
            {
                this.ChangeValue.Invoke(this._AddressOfCast[address], value);
            }
        }
                
        public void ValueChanged(int address, int value)
        {
            int i = 0;
            for (i = 0; i < _NumberOfCast; i++)
                if (_AddressOfCast[i] == address)
                    break;
            if (i == _NumberOfCast)
                return;
            MethodInfo mi = _DeviceType.GetMethod("ValueChanged");
            mi.Invoke(this._Instance, new object[] { i, value });
        }

        public int Run()
        {
            MethodInfo mi = _DeviceType.GetMethod("Run");
            return (int)mi.Invoke(this._Instance, new object[] { });           
        }

        public String Conflict(int address)
        {
            for (int i = 0; i < _AddressOfCast.Length; i++)
                if (_AddressOfCast[i] == address)
                    return this.Name;
            return null;
        }

        public bool Off()
        {
            if (this.RunThread != null)
            {
                try
                {
                    this.RunThread.Abort();
                    return true;
                }
                catch (Exception ex) { return false; }
            }
            return true;
        }
    }

    public class DeviceManage
    {
        Mutex EventMutex = new Mutex();

        int[] IOaddress = { };
        List<DeviceInfo> Devices = new List<DeviceInfo>();

        public List<DeviceInfo> DeviceInfos
        {
            get
            {
                return this.Devices;
            }
        }

        public delegate void EventDelegate(DeviceInfo s, int f, Exception ex);
        public event EventDelegate DeviceOff = null;


        public String AddDevice(DeviceInfo device)
        {
            for (int i = 0; i < device.AddressOfCast.Length; i++)
            {
                for (int j = 0; j < IOaddress.Length; j++)
                    if (device.AddressOfCast[i] == IOaddress[j])
                        return "Standard IO";
                for (int j = 0; j < Devices.Count; j++)
                    if (Devices[j].Conflict(device.AddressOfCast[i]) != null)
                        return Devices[j].Conflict(device.AddressOfCast[i]);
            }
            this.Devices.Add(device);
            return null;
        }

        public bool DeviceRun(DeviceInfo device)
        {
            if (device.PowerOn)
                return false;
            Thread t = new Thread(new ParameterizedThreadStart(this.ThreadFunction));
            t.IsBackground = true;
            t.Start(device);
            device.PowerOn = true;
            device.RunThread = t;
            return true;
        }
        private void ThreadFunction(Object device)
        {
            if (!(device is DeviceInfo))
                return;
            bool point = false;
            try
            {
                int f = ((DeviceInfo)device).Run();
                EventMutex.WaitOne();
                point = true;
                if (this.DeviceOff != null)
                    this.DeviceOff.Invoke((DeviceInfo)device, f, null);
                EventMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                if (this.DeviceOff != null)
                    this.DeviceOff.Invoke((DeviceInfo)device, -1, ex);
                if (point)
                    EventMutex.ReleaseMutex();
            }
        }

        public bool RemoveDevice(DeviceInfo device)
        {
            if (device.RunThread != null)
            {
                try
                {
                    device.RunThread.Abort();
                }
                catch (Exception ex) { }
            }
            device.PowerOn = false;
            this.Devices.Remove(device);
            return true;
        }
    }
}
