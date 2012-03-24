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
using System.Diagnostics;

namespace Edit
{
    /// <summary>
    /// TabItemHeader.xaml 的交互逻辑
    /// </summary>
    public partial class TabItemHeader : UserControl
    {
        public String header;
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate CloseClick,SelectClick;
        public TabItemHeader()
        {
            InitializeComponent();
            this.header = "";
            this.HeaderLabel.Content = "";
            this.CloseItem.MouseDown += new MouseButtonEventHandler(CloseItem_MouseDown);
            this.HeaderLabel.MouseDown += new MouseButtonEventHandler(HeaderLabel_MouseDown);
        }
       
        public void SetContent(String h)
        {
            this.header = h;
            this.HeaderLabel.Content = h;
        }
        public void SetSelect(bool b)
        {
            if (b)
            {
                this.BackgroundColor.Fill = (Brush)this.FindResource("SelectBrush");
                this.BackgroundColor.Height = 19.5;
                this.BackgroundColor.StrokeThickness = 1.5;
            }
            else
            {
                this.BackgroundColor.Fill = (Brush)this.FindResource("UnselectBrush");
                this.BackgroundColor.Height = 18;
                this.BackgroundColor.StrokeThickness = 1;
            }
        }
        void CloseItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CloseClick != null)
            {
                this.CloseClick.Invoke(this, new object[] { this.header, new TabCloseEvent() });
            }
        }
        void HeaderLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.SelectClick!=null)
                this.SelectClick.Invoke(this, new object[] { this.header });
        }
        public void OnClose()
        {
            if (this.CloseClick != null)
            {
                this.CloseClick.Invoke(this, new object[] { this.header, new TabCloseEvent() });
            }
        }
    }
    public class TabCloseEvent
    {
        public bool Cancel = false;
    }
}
