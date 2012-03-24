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
using System.IO;
using Abraham;

namespace DLXCheckerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void check()
        {

            DLXChecker dc = new DLXChecker();
            CheckResult cr = dc.Check(textBox1.Text);
            textBox1.Text = cr.Content.ToString();
            listBox1.Items.Clear();
            foreach (DLXException de in cr.Errors)
            {
                listBox1.Items.Add(string.Format("错误出现在{0}行{1}列：{2}", de.Line, de.Colum, de.Message));
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            check();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            StreamReader sr=new StreamReader(@"C:\Users\Abraham\Documents\Visual Studio 2010\Projects\VM\dlx\00m.txt",Encoding.GetEncoding("GBK"));
            textBox1.Text = sr.ReadToEnd();
            sr.Close();
        }
       
    }
}
