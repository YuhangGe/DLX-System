using System;
using System.Collections.Generic;
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

namespace Simulate
{
    /// <summary>
    /// ScrollPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollPanel : UserControl
    {
        public object content;
        public ScrollPanel(object p)
        {
            InitializeComponent();
            this.scrollPanel.Content = p;
            this.content = p;
        }
        public object getContent()
        {
            return content;
        }
    }
}
