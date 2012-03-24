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
using Abraham;
using System.Diagnostics;
namespace Edit
{
    /// <summary>
    /// ErrorShowPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorShowPanel : UserControl
    {
        public delegate void eventdelegate(object sender, object[] args);
        public event eventdelegate DoubleClickItem;
        public ErrorShowPanel()
        {
            InitializeComponent();
        }
        public void SetError(List<DLXException> errors)
        {
            this.ErrorNumberLabel.Content = errors.Count + " Errors";
            this.ErrorShowList.Items.Clear();
            for (int i = 0; i < errors.Count; i++)
            {
                ErrorShowItem esi = new ErrorShowItem();
                esi.SetShow(i + 1, errors[i].Line, errors[i].Colum, errors[i].Message);
                esi.MouseDoubleClick += new MouseButtonEventHandler(esi_MouseDoubleClick);
                this.ErrorShowList.Items.Add(esi);               
            }
        }

        void esi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ErrorShowItem ei = (ErrorShowItem)sender;
            //Debug.WriteLine(ei.IntNo + " " + ei.IntRow + " " + ei.IntCol + " " + ei.StrDes);
            if (this.DoubleClickItem != null)
                this.DoubleClickItem.Invoke(sender, new object[] { ei.IntRow, ei.IntCol, ei.StrDes, ei.IntNo });
        }
    }
}
