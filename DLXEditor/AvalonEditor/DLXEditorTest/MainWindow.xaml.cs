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
namespace DLXEditorTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dLXEditor1.HighlightFile =  @"C:\Documents and Settings\Abraham\Documents\Visual Studio 2010\Projects\VM\DLXEditor\AvalonEditor\Highlight.xshd";
            BitmapImage[] images = new BitmapImage[3];
            //images[0] = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\data\instruction.bmp"));
           // images[1] = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\data\label.bmp"));
            //images[2] = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\data\directive.bmp"));
            dLXEditor1.TipImages = images;
           // dLXEditor1.ShowAutoComplete = false;
        }

        private void dLXEditor1_DocumentFormatted(object sender, Abraham.CheckResult e)
        {
            List1.Items.Clear();
            foreach (DLXException ex in e.Errors)
            {
                List1.Items.Add(ex);
            }

        }

        private void List1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DLXException ex = (DLXException)List1.SelectedItem;

            if (ex != null)
            {

                dLXEditor1.SelectDocument(ex.Line, ex.Colum);
            }

        }
    }
}
