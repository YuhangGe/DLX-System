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
using System.Collections;

namespace Simulate
{
    /// <summary>
    /// ConditionForm.xaml 的交互逻辑
    /// </summary>
    public partial class ConditionForm : Window
    {
        public ConditionForm()
        {
            InitializeComponent();
            this.RefreshItem();
            this.RemoveButton.Click += new RoutedEventHandler(RemoveButton_Click);
            this.AddButton.Click += new RoutedEventHandler(AddButton_Click);
            this.AddButton.IsDefault = true;
        }

        void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = CPUInfo.getInstance().getBreakpoints().Count - 1; i >= 0; i--)
            {
                if (CPUInfo.getInstance().getBreakpoints()[i] is ConditionBreakpoint)
                    CPUInfo.getInstance().getBreakpoints().RemoveAt(i);
            }
            this.RefreshItem();
        }
        public void RefreshItem()
        {
            ArrayList bps = CPUInfo.getInstance().getBreakpoints();
            this.stackPanel.Children.Clear();
            for (int i = 0; i < bps.Count; i++)
                if (bps[i] is ConditionBreakpoint)
                {
                    ConditionBreakpointItem c = new ConditionBreakpointItem(this);
                    c.AddCBP((ConditionBreakpoint)bps[i]);
                    this.stackPanel.Children.Add(c);
                }
        }
        void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ConditionBreakpointItem cbi = new ConditionBreakpointItem(this);
            try
            {
                ConditionBreakpoint cbp= cbi.AddCBP(this.AddressBox.Text, this.ValueBox.Text);
                this.stackPanel.Children.Add(cbi);
                CPUInfo.getInstance().getBreakpoints().Add(cbp);
            }
            catch (Exception ex)
            {
                this.AddressBox.Text = "";
                this.ValueBox.Text = "";
                MessageBox.Show("Add Condition Breakpoint Error!");
            }
        }

    }
}
