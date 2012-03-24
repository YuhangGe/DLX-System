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
using Microsoft.Win32;

namespace Simulate
{
    /// <summary>
    /// SaveMemoryForm.xaml 的交互逻辑
    /// </summary>
    public partial class SaveMemoryForm : Window
    {
        public SaveMemoryForm()
        {
            InitializeComponent();
            this.CancelButton.Click += new RoutedEventHandler(CancelButton_Click);
            this.OkButton.Click += new RoutedEventHandler(OkButton_Click);
            this.PathButton.Click += new RoutedEventHandler(PathButton_Click);
        }

        void PathButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dlx String File(.sbin)|*.sbin";
            sfd.Title = "Choose Path";
            if (sfd.ShowDialog() == true)
            {
                this.PathTextbox.Text = sfd.FileName;
            }
        }

        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UInt32 b = SmallTool.StringLocationParse(this.BeginTextbox.Text);
                UInt32 eu = SmallTool.StringLocationParse(this.EndTextbox.Text);
                if (CPUInfo.getInstance().save(this.PathTextbox.Text, SmallTool.UinttoInt(b/4*4), SmallTool.UinttoInt(((eu-1)/4+1) - b/4)))
                    this.Close();
                else
                    MessageBox.Show("Invalid Value!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Value!");
            }

        }

        

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
