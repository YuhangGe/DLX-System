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

namespace Edit
{
    /// <summary>
    /// TabItemContent.xaml 的交互逻辑
    /// </summary>
    public partial class TabItemContent : UserControl
    {
        public TabItemContent()
        {
            InitializeComponent();
        }
        public void content(UIElement e)
        {
            this.MainPanel.Children.Clear();
            this.MainPanel.Children.Add(e);
        }
        public UIElement getContent()
        {
            if (this.MainPanel.Children.Count == 0)
                return null;
            else
                return this.MainPanel.Children[0];
        }
    }
}
