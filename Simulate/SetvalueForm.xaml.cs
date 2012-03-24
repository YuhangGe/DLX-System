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
using System.Diagnostics;

namespace Simulate
{
    /// <summary>
    /// SetvalueForm.xaml 的交互逻辑
    /// </summary>
    public partial class SetvalueForm : Window
    {
        public SetvalueForm()
        {
            InitializeComponent();
            this.OkButton.IsDefault = true;
            this.CancelButton.IsCancel = true;
            this.ApplicationButton.Click += new RoutedEventHandler(ApplicationButton_Click);
            this.OkButton.Click += new RoutedEventHandler(OkButton_Click);
            this.CancelButton.Click += new RoutedEventHandler(CancelButton_Click);
        }
        public void setLocation(String s)
        {
            this.AddressBox.Text = s;
        }

        private bool SetValue(string l, string v)
        {
            ConditionBreakpoint cbi = this.makeBreakpoint(l, v);
            if (cbi == null)
            {
                MessageBox.Show("Wrong Value!");
                return false;
            }
            else
            {
                if (cbi.isRegister)
                {
                    CPUInfo.getInstance().testRegister(cbi.register).Value = cbi.value;
                }
                else
                {
                    if (cbi.size == 1)
                    {
                        CPUInfo.getInstance().setMemoryValue(cbi.location, (byte)cbi.value);
                    }
                    else
                    {
                        UInt32 uv = SmallTool.InttoUint(cbi.value);
                        UInt32 ul = SmallTool.InttoUint(cbi.location);
                        for (int i = 3; i >= 0; i--)
                        {
                            int il = SmallTool.UinttoInt((UInt32)(ul + i));
                            byte bv=(byte)(uv % 256);
                            CPUInfo.getInstance().setMemoryValue(il, bv);
                            uv = uv / 256;
                        }
                    }
                }
                return true;
            }
        }

        void ApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            SetValue(this.AddressBox.Text, this.ValueBox.Text);
            ChildFormControl.getInstance().update();
        }

        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            bool b=SetValue(this.AddressBox.Text, this.ValueBox.Text);
            ChildFormControl.getInstance().update();
            if (b)
                this.Close();

        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private ConditionBreakpoint makeBreakpoint(String l, String v)
        {
            ConditionBreakpoint cbk;
            try
            {
                String ls = SmallTool.StringStardard(l);
                String vs = SmallTool.StringStardard(v);
                if (SmallTool.isRegisterName(ls))
                {
                    int value = SmallTool.StringValue32Parse(vs);
                    cbk = new ConditionBreakpoint(true, 0, ls, value, 4);
                    return cbk;
                }
                else
                {
                    UInt32 loc = SmallTool.StringLocationParse(ls);
                    if (vs[0] != '4')
                    {
                        byte value = SmallTool.StringValue8Parse(vs);
                        cbk = new ConditionBreakpoint(false, SmallTool.UinttoInt(loc), "", value, 1);
                        return cbk;
                    }
                    else
                    {
                        vs = vs.Substring(1);
                        int value1 = SmallTool.StringValue32Parse(vs);
                        cbk = new ConditionBreakpoint(false, SmallTool.UinttoInt(loc / 4 * 4), "", value1, 4);
                        return cbk;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
