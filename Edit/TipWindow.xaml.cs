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
using System.Diagnostics;
using System.Threading;

namespace Edit
{
    /// <summary>
    /// TipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TipWindow : Window
    {

        public double visualWidth
        {
            get
            {
                return this.Grid.ActualWidth;
            }
        }
        public double visualHeight
        {
            get
            {
                return this.Grid.ActualHeight;
            }
        }

        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate ChooseLabel;

        public TipWindow()
        {
            InitializeComponent();


            Thread t = new Thread(new ThreadStart(ActiveTest));
            t.IsBackground = true;               
            t.Start();
        }
        public void AddItems(List<DlxTabItem> all, DlxTabItem selected)
        {
            this.stackPanel.Children.Clear();
            for (int i = 0; i < all.Count; i++)
            {
                TextBlock l = new TextBlock();
                l.Width = double.NaN;
                l.Height = double.NaN;
                l.FontSize = 13;
                l.Style=(Style)this.FindResource("TabLabelStyle");
                if (all[i] == selected)
                    l.Text = ">  " + all[i].header;
                else
                    l.Text = "    " + all[i].header;
                this.stackPanel.Children.Add(l);
                l.MouseDown += new MouseButtonEventHandler(l_MouseDown);
            }
            if (this.stackPanel.Children.Count != 0)
                this.Grid.Visibility = Visibility.Visible;
            else
                this.Grid.Visibility = Visibility.Hidden;
        }

        void l_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index=this.stackPanel.Children.IndexOf((UIElement)sender);
            if (index != -1)
            {
                if (this.ChooseLabel != null)
                {
                    this.ChooseLabel.Invoke(this, new object[] { index, ((TextBlock)sender).Text });
                }
            }
        }



        private delegate void NoneParmFun();
        bool active = true;
        private void ActiveTest()
        {
            while (this.active)
            {
                Thread.Sleep(5);
                this.Dispatcher.Invoke(new NoneParmFun(activeWindow));
            }
            this.Dispatcher.Invoke(new NoneParmFun(closeWindow));

        }
        private void closeWindow()
        {
            this.Close();
        }
        private void activeWindow()
        {
            if (this.IsActive)
                this.active = true;
            else
                this.active = false;
        }
    }
}
