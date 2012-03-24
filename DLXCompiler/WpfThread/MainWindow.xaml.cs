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
using System.Threading;
namespace WpfThread
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
        private Thread t_test;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(System.Text.RegularExpressions.Regex.IsMatch("   ","s*").ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            t_test = new Thread(new ThreadStart(add));
            t_test.Start();
        }
        private int i = 0;
        private delegate  void addDelegate(int i);
        private void do_add(int i)
        {
            textBlock1.Text = i.ToString();
            textBlock2.Text = i.ToString();
        }
        private void add()
        {
            while(i<30){
                button1.Dispatcher.Invoke(new addDelegate(do_add),i);
                i++;
                Thread.Sleep(1000);
            }
            
        }
    }
}
