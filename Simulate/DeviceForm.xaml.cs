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
using System.Windows.Shapes;
using Microsoft.Win32;
using DlxExternalDevice;

namespace Simulate
{
    /// <summary>
    /// DeviceForm.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceForm : Window
    {

        private DeviceItem _currentItem = null;
        private DeviceItem currentItem
        {
            get { return _currentItem; }
            set
            {                
                if (value == null)
                {
                    this.Describe.Text = "";
                    if (this._currentItem != null)
                        this._currentItem.Choose = false;
                    this._currentItem = value;
                }
                else
                {
                    this.Describe.Text = value.device.Describe;
                    if (this._currentItem != null)
                        this._currentItem.Choose = false;
                    this._currentItem = value;
                    this._currentItem.Choose = true;
                }
            }
        }

        public DeviceForm()
        {
            InitializeComponent();
            this.AddButton.Click += new RoutedEventHandler(AddButton_Click);
            this.RemoveAllButton.Click += new RoutedEventHandler(RemoveAllButton_Click);
        }    

        void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ItemStack.Children.Count; i++)
                ((DeviceItem)ItemStack.Children[i]).OnClose();
        }

        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.FileName == "")
                return;
            DeviceInfo device = DeviceInfo.GetDeviceInfo(ofd.FileName);
            if (device == null)
            {
                MessageBox.Show("Open File Failure!");
                return;
            }
            String s=DeviceControl.getInstance().deviceManage.AddDevice(device);
            if (s == null)
            {
                DeviceItem di = new DeviceItem();
                this.ItemStack.Children.Add(di);
                di.device = device;
                di.CloseClick += new DeviceItem.EventDelegate(Items_CloseClick);
                di.ItemClick += new DeviceItem.EventDelegate(Items_ItemClick);
                this.currentItem = di;
            }
            else
            {
                MessageBox.Show(s + " Conflict!");
            }
            
        }

        void Items_ItemClick(DeviceItem sender, DeviceInfo device)
        {
            if(this.ItemStack.Children.IndexOf(sender)!=-1)
                this.currentItem = sender;
        }

        void Items_CloseClick(DeviceItem sender, DeviceInfo device)
        {
            device.Off();
            DeviceControl.getInstance().deviceManage.RemoveDevice(device);
            this.ItemStack.Children.Remove(sender);
            if (this.currentItem == sender)
                this.currentItem = null;
        }
    }
}
