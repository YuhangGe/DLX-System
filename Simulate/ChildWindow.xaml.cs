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
using System.Windows.Shapes;

namespace Simulate
{
    /// <summary>
    /// ChildWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChildWindow : Window
    {

        ChildFormControl formControl = ChildFormControl.getInstance();
        List<ChildWindow> relation = new List<ChildWindow>();

        public ChildWindow()
        {
            InitializeComponent();
            this.LocationChanged += new EventHandler(ChildWindow_LocationChanged);
            this.Activated += new EventHandler(ChildWindow_Activated);
        }

        void ChildWindow_Activated(object sender, EventArgs e)
        {
            this.formControl.active(this);
        }

        void ChildWindow_LocationChanged(object sender, EventArgs e)
        {            
            this.formControl.absorb(this);
        }

        public DockPanel getMainPanel()
        {
            return this.MainPanel;
        }
    }
}
