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
    /// ErrorShowItem.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorShowItem : UserControl
    {
        public int IntRow;
        public int IntCol;
        public int IntNo;
        public string StrDes;
        public ErrorShowItem()
        {
            InitializeComponent();
        }
        public void SetShow(int no, int row, int col, string des)
        {
            this.Number.Content = no;
            this.Row.Content = row;
            this.Column.Content = col;
            this.Expression.Text = des;
            this.IntNo = no;
            this.IntRow = row;
            this.IntCol = col;
            this.StrDes = des;
        }
    }
}
