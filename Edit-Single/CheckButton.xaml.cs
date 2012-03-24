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

namespace Edit_Single
{
    /// <summary>
    /// CheckButton.xaml 的交互逻辑
    /// </summary>
    public partial class CheckButton : UserControl
    {
        public delegate void CheckChangeHandle(object sender, bool value);
        public event CheckChangeHandle CheckChanged = null;
        public bool _check = false;
        public bool Check
        {
            get { return _check; }
            set
            {
                _check = value;
                if (value)
                {
                    this.rect1.Visibility = Visibility.Hidden;
                    this.rect2.Visibility = Visibility.Visible;
                    this.rect3.Visibility = Visibility.Visible;
                    this.rect4.Visibility = Visibility.Hidden;
                    this.image.Style = null;                    
                }
                else
                {
                    this.rect2.Visibility = Visibility.Hidden;
                    this.rect1.Visibility = Visibility.Visible;
                    this.rect3.Visibility = Visibility.Hidden;
                    this.rect4.Visibility = Visibility.Visible;
                    this.image.Style = (Style)this.FindResource("ImageOpacity");
                }
                if (this.CheckChanged != null)
                    this.CheckChanged.Invoke(this, value);
            }
        }
        
        public CheckButton()
        {
            InitializeComponent();
            this.Check = false;
            this.MouseDown += new MouseButtonEventHandler(CheckButton_MouseDown);
        }

        void CheckButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Check)
                this.ChangeCheck(false);
            else
                this.ChangeCheck(true);
        }
        public void ChangeCheck(bool b)
        {
            this.Check = b;
        }

        public void SetImage(String str)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(str, UriKind.RelativeOrAbsolute);
            bi.EndInit();       
            this.image.Source = bi;            
        }
    }
}
