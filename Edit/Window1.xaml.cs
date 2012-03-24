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
using ICSharpCode.AvalonEdit.CodeCompletion;
namespace Edit
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            DlxTabItem d1 = new DlxTabItem();
            d1.header = "aa";
            DlxTabItem d2 = new DlxTabItem();
            d2.header = "bbbb";
            DlxTabItem d3 = new DlxTabItem();
            d3.header = "cccc";
            d1.content = new Button();
            d1.CloseClick += new DlxTabItem.eventdelegate(d1_CloseClick);
            this.DlxTab.AddItem(d1);
            this.DlxTab.AddItem(d2);
            this.DlxTab.AddItem(d3);
        }

        void d1_CloseClick(object sender, object[] args)
        {
            ((TabCloseEvent)args[1]).Cancel = true;
        }
    }
}
