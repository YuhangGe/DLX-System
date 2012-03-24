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

namespace Edit_Single
{
    /// <summary>
    /// OutputItem.xaml 的交互逻辑
    /// </summary>
    public partial class OutputItem : UserControl
    {
        public bool ChooseAble = false;
        private bool _isChoose = false;
        public bool isChoose
        {
            get { return _isChoose; }
            set
            {
                _isChoose = value;
                if (value && ChooseAble)
                {
                    this.rect.Visibility = Visibility.Visible;
                    this.rect_Copy.Visibility = Visibility.Hidden;
                    this.text.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                else if (value && !ChooseAble)
                {
                    this.rect.Visibility = Visibility.Hidden;
                    this.rect_Copy.Visibility = Visibility.Visible;
                    this.text.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                else
                {
                    this.rect.Visibility = Visibility.Hidden;
                    this.rect_Copy.Visibility = Visibility.Hidden;
                    this.text.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
            }
        }

        public String message = "";

        public delegate void DoubleClickHandle(OutputItem sender, String message);
        public event DoubleClickHandle DoubleClick;

        public OutputItem()
        {
            InitializeComponent();
            this.AddContextMenu();
            this.MouseDoubleClick += new MouseButtonEventHandler(OutputItem_MouseDoubleClick);
        }

        void OutputItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.OnDoubleClick();
        }
        public void OnDoubleClick()
        {
            if (this.DoubleClick != null)
                this.DoubleClick.Invoke(this, this.message);
        }

        public void Show(String s)
        {
            this.text.Content = s;
        }

        private void AddContextMenu()
        {
            ContextMenu cm = new ContextMenu();
            cm.SizeChanged += new SizeChangedEventHandler(cm_SizeChanged);
            MenuItem m1 = new MenuItem();
            m1.Header = "Active";
            cm.Items.Add(m1);
            m1.Click += new RoutedEventHandler(m1_Click);
            MenuItem m2 = new MenuItem();
            m2.Header = "Copy";
            cm.Items.Add(m2);
            m2.Click += new RoutedEventHandler(m2_Click);
            this.ContextMenu = cm;
        }

        void cm_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.DoubleClick == null)
            {
                if (((ContextMenu)sender).Items.Count > 0)
                    ((MenuItem)((ContextMenu)sender).Items[0]).IsEnabled = false;
            }
        }

        void m1_Click(object sender, RoutedEventArgs e)
        {
            this.OnDoubleClick();
        }
        void m2_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, this.text.Content);
        }
    }
}
